using OpenCV.Net;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace Bonsai.VR
{
    public class TrackedCameraState : DeviceState
    {
        internal static readonly uint HeaderSize = (uint)Marshal.SizeOf(typeof(CameraVideoStreamFrameHeader_t));

        public uint Width;
        public uint Height;
        public uint BytesPerPixel;
        public uint FrameSequence;
        public ulong FrameExposureTime;
        public EVRTrackedCameraFrameType FrameType;
        internal ulong Handle;

        internal void UpdateFrameHeader(ref CameraVideoStreamFrameHeader_t header)
        {
            IsValid = header.trackedDevicePose.bPoseIsValid;
            DataHelper.ToVector3(ref header.trackedDevicePose.vVelocity, out Velocity);
            DataHelper.ToVector3(ref header.trackedDevicePose.vAngularVelocity, out AngularVelocity);
            DataHelper.ToMatrix4(ref header.trackedDevicePose.mDeviceToAbsoluteTracking, out DevicePose);
            Width = header.nWidth;
            Height = header.nHeight;
            BytesPerPixel = header.nBytesPerPixel;
            FrameSequence = header.nFrameSequence;
            FrameExposureTime = header.ulFrameExposureTime;
        }
    }
}
