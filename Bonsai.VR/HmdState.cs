using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.VR
{
    public class HmdState
    {
        public Vector3 Velocity;
        public Vector3 AngularVelocity;
        public Matrix4 DevicePose;
        public Matrix4 LeftViewMatrix;
        public Matrix4 LeftProjectionMatrix;
        public Matrix4 RightViewMatrix;
        public Matrix4 RightProjectionMatrix;
    }
}
