﻿using System;
using System.Linq;
using System.Reactive.Linq;
using Valve.VR;

namespace Bonsai.VR
{
    public class GetTrackingReference : Transform<VRDataFrame, TrackingReference>
    {
        public int Index { get; set; }

        public override IObservable<TrackingReference> Process(IObservable<VRDataFrame> source)
        {
            return Observable.Defer(() =>
            {
                var deviceIndices = new uint[OpenVR.k_unMaxTrackedDeviceCount];
                return source.Select(input =>
                {
                    var index = Index;
                    var result = new TrackingReference();
                    var count = (int)input.Hmd.GetSortedTrackedDeviceIndicesOfClass(ETrackedDeviceClass.TrackingReference, deviceIndices, OpenVR.k_unTrackedDeviceIndex_Hmd);
                    if (index >= 0 && index < count)
                    {
                        var deviceIndex = deviceIndices[index];
                        result.ReferencePose = input.RenderPoses[deviceIndex].DeviceToAbsolutePose;
                        result.IsValid = input.RenderPoses[deviceIndex].IsValid;
                    }
                    return result;
                });
            });
        }
    }
    
}