using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
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
                var headerSize = (uint)Marshal.SizeOf(typeof(CameraVideoStreamFrameHeader_t));
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
                        headerSize);
                    ThrowExceptionForErrorCode(error);

                    var result = new TrackedCameraState();
                    result.DeviceIndex = (int)deviceIndex;
                    result.IsValid = header.trackedDevicePose.bPoseIsValid;
                    DataHelper.ToVector3(ref header.trackedDevicePose.vVelocity, out result.Velocity);
                    DataHelper.ToVector3(ref header.trackedDevicePose.vAngularVelocity, out result.AngularVelocity);
                    DataHelper.ToMatrix4(ref header.trackedDevicePose.mDeviceToAbsoluteTracking, out result.DevicePose);
                    result.Width = header.nWidth;
                    result.Height = header.nHeight;
                    result.BytesPerPixel = header.nBytesPerPixel;
                    result.FrameSequence = header.nFrameSequence;
                    result.FrameExposureTime = header.ulFrameExposureTime;
                    result.FrameType = frameType;
                    result.Handle = handle;
                    return result;
                }).Finally(() =>
                {
                    if (handle != 0)
                    {
                        OpenVR.TrackedCamera.ReleaseVideoStreamingService(handle);
                    }
                });
            });
        }
    }
}
