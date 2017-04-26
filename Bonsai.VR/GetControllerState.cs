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
                    var result = new ControllerState();
                    var index = Index.GetValueOrDefault();
                    var role = ControllerRole.GetValueOrDefault();
                    if (role != ETrackedControllerRole.Invalid)
                    {
                        index = (int)input.Hmd.GetTrackedDeviceIndexForControllerRole(role);
                    }

                    if (index >= 0)
                    {
                        var valid = input.Hmd.GetControllerState((uint)index, ref state);
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
                    }
                    return result;
                });
            });
        }
    }
    
}
