using System;
using System.Linq;
using System.Reactive.Linq;
using Valve.VR;

namespace Bonsai.VR
{
    public class GetControllerState : Transform<VRDataFrame, ControllerState>
    {
        public int? Index { get; set; }

        public ETrackedControllerRole? ControllerRole { get; set; }

        public override IObservable<ControllerState> Process(IObservable<VRDataFrame> source)
        {
            return Observable.Defer(() =>
            {
                var state = new VRControllerState_t();
                return source.Select(input =>
                {
                    var index = (uint)Index.GetValueOrDefault();
                    var role = ControllerRole.GetValueOrDefault();
                    if (role != ETrackedControllerRole.Invalid)
                    {
                        index = input.Hmd.GetTrackedDeviceIndexForControllerRole(role);
                    }

                    var valid = input.Hmd.GetControllerState(index, ref state);
                    var result = new ControllerState();
                    result.Axis0 = state.rAxis0;
                    result.Axis1 = state.rAxis1;
                    result.Axis2 = state.rAxis2;
                    result.Axis3 = state.rAxis3;
                    result.Axis4 = state.rAxis4;
                    result.ButtonPressed = state.ulButtonPressed;
                    result.ButtonTouched = state.ulButtonTouched;
                    result.PacketNumber = state.unPacketNum;
                    result.ControllerPose = input.RenderPoses[index].DeviceToAbsolutePose;
                    result.IsValid = valid && input.RenderPoses[index].IsValid;
                    return result;
                });
            });
        }
    }
    
}
