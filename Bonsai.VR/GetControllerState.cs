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
                    DataHelper.ToVector2(ref state.rAxis0, out result.Axis0);
                    DataHelper.ToVector2(ref state.rAxis1, out result.Axis1);
                    DataHelper.ToVector2(ref state.rAxis2, out result.Axis2);
                    DataHelper.ToVector2(ref state.rAxis3, out result.Axis3);
                    DataHelper.ToVector2(ref state.rAxis4, out result.Axis4);
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
