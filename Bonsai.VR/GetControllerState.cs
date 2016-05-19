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

    namespace Bonsai.VR
    {
        public static class ButtonMask
        {
            public const ulong System = (1ul << (int)EVRButtonId.k_EButton_System); // reserved
            public const ulong ApplicationMenu = (1ul << (int)EVRButtonId.k_EButton_ApplicationMenu);
            public const ulong Grip = (1ul << (int)EVRButtonId.k_EButton_Grip);
            public const ulong Axis0 = (1ul << (int)EVRButtonId.k_EButton_Axis0);
            public const ulong Axis1 = (1ul << (int)EVRButtonId.k_EButton_Axis1);
            public const ulong Axis2 = (1ul << (int)EVRButtonId.k_EButton_Axis2);
            public const ulong Axis3 = (1ul << (int)EVRButtonId.k_EButton_Axis3);
            public const ulong Axis4 = (1ul << (int)EVRButtonId.k_EButton_Axis4);
            public const ulong Touchpad = (1ul << (int)EVRButtonId.k_EButton_SteamVR_Touchpad);
            public const ulong Trigger = (1ul << (int)EVRButtonId.k_EButton_SteamVR_Trigger);
        }

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
                        var poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
                        var compositor = OpenVR.Compositor;
                        //compositor.SetTrackingSpace(trackingSpace);

                        prevState = state;
                        valid = input.Hmd.GetControllerStateWithPose(trackingSpace, Index, ref state, ref pose);
                        //UpdateHairTrigger();

                        ControllerState result_state = new ControllerState();


                        ulong Trigger = (1ul << (int)EVRButtonId.k_EButton_SteamVR_Trigger);

                        result_state.Axis0 = state.rAxis0;
                        result_state.Axis1 = state.rAxis1;
                        result_state.Axis2 = state.rAxis2;
                        result_state.Axis3 = state.rAxis3;
                        result_state.Axis4 = state.rAxis4;
                        result_state.ButtonPressed = state.ulButtonPressed;
                        result_state.ButtonTouched = state.ulButtonTouched;
                        result_state.PacketNum = state.unPacketNum;
                        result_state.ControllerPose = input.RenderPoses[Index].DeviceToAbsolutePose;
                        //DataHelper.ToMatrix4(ref pose.mDeviceToAbsoluteTracking, out result_state.ControllerPose);


                        return result_state;
                    });
                });
            }
        }
    }
}
