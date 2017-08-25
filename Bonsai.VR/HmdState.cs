using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.VR
{
    public class HmdState : DeviceState
    {
        public Matrix4 LeftViewMatrix;
        public Matrix4 LeftProjectionMatrix;
        public Matrix4 RightViewMatrix;
        public Matrix4 RightProjectionMatrix;
    }
}
