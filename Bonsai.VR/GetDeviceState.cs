using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Valve.VR;

namespace Bonsai.VR
{
    [Description("Extracts the specified tracked device state from the specified VR system data.")]
    public class GetDeviceState : Transform<VRDataFrame, DeviceState>
    {
        [Description("The serial number of the device for which to extract the state.")]
        public string SerialNumber { get; set; }

        public override IObservable<DeviceState> Process(IObservable<VRDataFrame> source)
        {
            return source.Select(input =>
            {
                var index = -1;
                var serial = SerialNumber;
                var result = new DeviceState();
                for (uint i = 0; i < input.RenderPoses.Length; i++)
                {
                    if (serial == input.Hmd.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_SerialNumber_String))
                    {
                        index = (int)i;
                        break;
                    }
                }

                if (index >= 0 && index < input.RenderPoses.Length)
                {
                    var deviceIndex = index;
                    result.Velocity = input.RenderPoses[deviceIndex].Velocity;
                    result.AngularVelocity = input.RenderPoses[deviceIndex].AngularVelocity;
                    result.DevicePose = input.RenderPoses[deviceIndex].DeviceToAbsolutePose;
                    result.IsValid = input.RenderPoses[deviceIndex].IsValid;
                }
                return result;
            });
        }
    }
}
