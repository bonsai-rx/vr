using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.VR
{
    using OpenTK;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Valve.VR;

 
    enum Buttons
    {
        System = (int)EVRButtonId.k_EButton_System, // reserved
        ApplicationMenu = (int)EVRButtonId.k_EButton_ApplicationMenu,
        Grip = (int)EVRButtonId.k_EButton_Grip,
        Axis0 = (int)EVRButtonId.k_EButton_Axis0,
        Axis1 = (int)EVRButtonId.k_EButton_Axis1,
        Axis2 = (int)EVRButtonId.k_EButton_Axis2,
        Axis3 = (int)EVRButtonId.k_EButton_Axis3,
        Axis4 = (int)EVRButtonId.k_EButton_Axis4,
        Touchpad = (int)EVRButtonId.k_EButton_SteamVR_Touchpad,
        Trigger = (int)EVRButtonId.k_EButton_SteamVR_Trigger
    };

    public class GetControllerState : Transform<VRDataFrame, ControllerState>
    {

        VRControllerState_t prevState;
        TrackedDevicePose_t pose;
        ETrackingUniverseOrigin trackingSpace = ETrackingUniverseOrigin.TrackingUniverseStanding;

           

        bool valid { get; set; }

        public uint Index { get; set; }

        public ETrackedControllerRole ControllerRole { get; set; }

        public GetControllerState()
        {
                
        }


        public override IObservable<ControllerState> Process(IObservable<VRDataFrame> source)
        {
            return Observable.Defer(() =>
            {
                    
                return source.Select(input =>
                {
                    var state = new VRControllerState_t();
                    var compositor = OpenVR.Compositor;
                    //compositor.SetTrackingSpace(trackingSpace);

                    prevState = state;
                    valid = input.Hmd.GetControllerStateWithPose(trackingSpace, Index, ref state, ref pose);

                    ControllerState result_state = new ControllerState();

                    result_state.Axis0 = state.rAxis0;
                    result_state.Axis1 = state.rAxis1;
                    result_state.Axis2 = state.rAxis2;
                    result_state.Axis3 = state.rAxis3;
                    result_state.Axis4 = state.rAxis4;
                    result_state.ButtonPressedId = state.ulButtonPressed;
                    result_state.ButtonTouchedId = state.ulButtonTouched;
                    result_state.PacketNum = state.unPacketNum;
                    result_state.ControllerPose = input.RenderPoses[Index].DeviceToAbsolutePose;
                    result_state.IsValid = input.RenderPoses[Index].IsValid;

                    foreach (string button in Enum.GetNames(typeof(Buttons)))
                    {
                        if((state.ulButtonPressed & (1ul << (int)Enum.Parse(typeof(Buttons), button))) != 0){
                            result_state.ButtonPressedName = button;
                        }

                        if ((state.ulButtonTouched & (1ul << (int)Enum.Parse(typeof(Buttons), button))) != 0)
                        {
                            result_state.ButtonTouchedName = button;
                        }
                    }

                    return result_state;
                });
            });
        }
    }
    
}
