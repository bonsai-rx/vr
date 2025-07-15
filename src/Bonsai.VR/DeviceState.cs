using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.VR
{
    public class DeviceState
    {
        public int DeviceIndex;
        public Vector3 Velocity;
        public Vector3 AngularVelocity;
        public Matrix4 DevicePose;
        public bool IsValid;
    }
}
