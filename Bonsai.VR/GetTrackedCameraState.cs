using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace Bonsai.VR
{
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Extracts the specified tracked camera state from the VR system.")]
    public class GetTrackedCameraState : Combinator<TrackedCameraState>
    {
        [Description("The index of the video streaming device for which to extract the state.")]
        public int DeviceIndex { get; set; }

        [Description("The type of frame stream that will be requested from the device.")]
        public EVRTrackedCameraFrameType FrameType { get; set; }

        static void ThrowExceptionForErrorCode(EVRTrackedCameraError error)
        {
            if (error != EVRTrackedCameraError.None && error != EVRTrackedCameraError.NoFrameAvailable)
            {
                var message = OpenVR.TrackedCamera.GetCameraErrorNameFromEnum(error);
                throw new InvalidOperationException(message);
            }
        }

        public override IObservable<TrackedCameraState> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                ulong handle = 0;
                var deviceIndex = (uint)DeviceIndex;
                var header = new CameraVideoStreamFrameHeader_t();
                EVRTrackedCameraError error;

                return source.Select(input =>
                {
                    if (handle == 0)
                    {
                        error = OpenVR.TrackedCamera.AcquireVideoStreamingService(deviceIndex, ref handle);
                        ThrowExceptionForErrorCode(error);
                    }

                    var frameType = FrameType;
                    error = OpenVR.TrackedCamera.GetVideoStreamFrameBuffer(
                        handle,
                        frameType,
                        IntPtr.Zero, 0,
                        ref header,
                        TrackedCameraState.HeaderSize);
                    ThrowExceptionForErrorCode(error);

                    var result = new TrackedCameraState();
                    result.DeviceIndex = (int)deviceIndex;
                    result.UpdateFrameHeader(ref header);
                    result.FrameType = frameType;
                    result.Handle = handle;
                    return result;
                }).Finally(() =>
                {
                    if (handle != 0)
                    {
                        error = OpenVR.TrackedCamera.ReleaseVideoStreamingService(handle);
                        ThrowExceptionForErrorCode(error);
                    }
                });
            });
        }
    }
}
