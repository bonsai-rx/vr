using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace Bonsai.VR
{
    public class VRDataFrame
    {
        public VRDataFrame(
            CVRSystem system,
            TrackedDevicePose[] renderPoses,
            TrackedDevicePose[] absolutePoses,
            float secondsSinceLastVsync,
            ulong frameCounter)
        {
            System = system;
            RenderPoses = renderPoses;
            AbsolutePoses = absolutePoses;
            SecondsSinceLastVsync = secondsSinceLastVsync;
            FrameCounter = frameCounter;
        }

        public CVRSystem System { get; private set; }

        public TrackedDevicePose[] RenderPoses { get; private set; }

        public TrackedDevicePose[] AbsolutePoses { get; private set; }

        public float SecondsSinceLastVsync { get; private set; }

        public ulong FrameCounter { get; private set; }
    }
}
