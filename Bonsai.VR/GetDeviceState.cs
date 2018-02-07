using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Valve.VR;

namespace Bonsai.VR
{
    [Description("Extracts the specified tracked device state from the specified VR system data.")]
    public class GetDeviceState : Transform<VRDataFrame, DeviceState>
    {
        [TypeConverter(typeof(SerialNumberConverter))]
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
                    if (input.RenderPoses[i].IsDeviceConnected &&
                        serial == input.Hmd.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_SerialNumber_String))
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

        class SerialNumberConverter : StringConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                var initialized = false;
                var system = OpenVR.System;
                try
                {
                    if (system == null)
                    {
                        var initError = EVRInitError.None;
                        system = OpenVR.Init(ref initError, EVRApplicationType.VRApplication_Other);
                        if (initError != EVRInitError.None)
                        {
                            return base.GetStandardValues(context);
                        }
                        else initialized = true;
                    }

                    var serialNumbers = new List<string>();
                    var devices = new uint[OpenVR.k_unMaxTrackedDeviceCount];
                    var count = system.GetSortedTrackedDeviceIndicesOfClass(ETrackedDeviceClass.Other, devices, 0);
                    for (uint i = 0; i < count; i++)
                    {
                        var serial = system.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_SerialNumber_String);
                        serialNumbers.Add(serial);
                    }

                    return new StandardValuesCollection(serialNumbers);
                }
                finally
                {
                    if (initialized)
                    {
                        OpenVR.Shutdown();
                    }
                }
            }
        }
    }
}
