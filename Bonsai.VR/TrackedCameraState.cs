using OpenCV.Net;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace Bonsai.VR
{
    public class TrackedCameraState : DeviceState
    {
        public uint Width;
        public uint Height;
        public uint BytesPerPixel;
        public uint FrameSequence;
        public ulong FrameExposureTime;
        public EVRTrackedCameraFrameType FrameType;
        internal ulong Handle;
    }
}
