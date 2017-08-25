using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Valve.VR;

namespace Bonsai.VR
{
    [Description("Extracts the specified tracking reference pose from the specified VR system data.")]
    public class GetTrackingReference : Transform<VRDataFrame, DeviceState>
    {
        [Description("The index of the tracking reference for which to extract the pose.")]
        public int Index { get; set; }

        public override IObservable<DeviceState> Process(IObservable<VRDataFrame> source)
        {
            return Observable.Defer(() =>
            {
                var deviceIndices = new uint[OpenVR.k_unMaxTrackedDeviceCount];
                return source.Select(input =>
                {
                    var index = Index;
                    var result = new DeviceState();
                    var count = (int)input.Hmd.GetSortedTrackedDeviceIndicesOfClass(ETrackedDeviceClass.TrackingReference, deviceIndices, OpenVR.k_unTrackedDeviceIndex_Hmd);
                    if (index >= 0 && index < count)
                    {
                        var deviceIndex = deviceIndices[index];
                        result.Velocity = input.RenderPoses[deviceIndex].Velocity;
                        result.AngularVelocity = input.RenderPoses[deviceIndex].AngularVelocity;
                        result.DevicePose = input.RenderPoses[deviceIndex].DeviceToAbsolutePose;
                        result.IsValid = input.RenderPoses[deviceIndex].IsValid;
                    }
                    return result;
                });
            });
        }
    }
    
}
