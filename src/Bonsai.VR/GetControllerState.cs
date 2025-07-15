using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Valve.VR;

namespace Bonsai.VR
{
    [Description("Extracts the specified controller state from the specified VR system data.")]
    public class GetControllerState : Transform<VRDataFrame, ControllerState>
    {
        [Description("The optional index of the controller for which to extract the state.")]
        public int? Index { get; set; }

        [Description("The optional role of the controller for which to extract the state.")]
        public ETrackedControllerRole? ControllerRole { get; set; }

        public override IObservable<ControllerState> Process(IObservable<VRDataFrame> source)
        {
            return Observable.Defer(() =>
            {
                var state = new VRControllerState_t();
                var stateSize = (uint)Marshal.SizeOf(typeof(VRControllerState_t));
                return source.Select(input =>
                {
                    var result = new ControllerState();
                    var index = Index.GetValueOrDefault();
                    var role = ControllerRole.GetValueOrDefault();
                    if (role != ETrackedControllerRole.Invalid)
                    {
                        index = (int)input.System.GetTrackedDeviceIndexForControllerRole(role);
                    }

                    if (index >= 0)
                    {
                        var valid = input.System.GetControllerState((uint)index, ref state, stateSize);
                        DataHelper.ToVector2(ref state.rAxis0, out result.Axis0);
                        DataHelper.ToVector2(ref state.rAxis1, out result.Axis1);
                        DataHelper.ToVector2(ref state.rAxis2, out result.Axis2);
                        DataHelper.ToVector2(ref state.rAxis3, out result.Axis3);
                        DataHelper.ToVector2(ref state.rAxis4, out result.Axis4);
                        result.ButtonPressed = state.ulButtonPressed;
                        result.ButtonTouched = state.ulButtonTouched;
                        result.PacketNumber = state.unPacketNum;
                        result.Velocity = input.RenderPoses[index].Velocity;
                        result.AngularVelocity = input.RenderPoses[index].AngularVelocity;
                        result.DevicePose = input.RenderPoses[index].DeviceToAbsolutePose;
                        result.IsValid = valid && input.RenderPoses[index].IsValid;
                        result.DeviceIndex = index;
                    }
                    return result;
                });
            });
        }
    }
    
}
