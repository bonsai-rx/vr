using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Valve.VR;

namespace Bonsai.VR
{
    [Description("Extracts the specified tracked device state from the specified VR system data.")]
    public class GetTrackerState : Transform<VRDataFrame, TrackerState>
    {
        [TypeConverter(typeof(SerialNumberConverter))]
        [Description("The serial number of the device for which to extract the state.")]
        public string SerialNumber { get; set; }

        public override IObservable<TrackerState> Process(IObservable<VRDataFrame> source)
        {
            return Observable.Defer(() =>
            {
                var state = new VRControllerState_t();
                var stateSize = (uint)Marshal.SizeOf(typeof(VRControllerState_t));
                return source.Select(input =>
                {
                    var index = -1;
                    var serial = SerialNumber;
                    var result = new TrackerState();
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
                        var valid = input.Hmd.GetControllerState((uint)index, ref state, stateSize);
                        result.ButtonPressed = state.ulButtonPressed;
                        result.PacketNumber = state.unPacketNum;
                        result.Velocity = input.RenderPoses[index].Velocity;
                        result.AngularVelocity = input.RenderPoses[index].AngularVelocity;
                        result.DevicePose = input.RenderPoses[index].DeviceToAbsolutePose;
                        result.IsValid = valid && input.RenderPoses[index].IsValid;
                    }
                    return result;
                });
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
                    var count = system.GetSortedTrackedDeviceIndicesOfClass(ETrackedDeviceClass.GenericTracker, devices, 0);
                    for (uint i = 0; i < count; i++)
                    {
                        var serial = system.GetStringTrackedDeviceProperty(devices[i], ETrackedDeviceProperty.Prop_SerialNumber_String);
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
