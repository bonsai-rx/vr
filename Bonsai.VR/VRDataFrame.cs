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
        public VRDataFrame(CVRSystem hmd, DevicePose[] renderPoses)
        {
            Hmd = hmd;
            RenderPoses = renderPoses;
        }

        public CVRSystem Hmd { get; private set; }

        public DevicePose[] RenderPoses { get; private set; }
    }
}
