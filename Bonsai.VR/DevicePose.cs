using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace Bonsai.VR
{
    public struct DevicePose
    {
        public Vector3 Velocity;
        public Vector3 AngularVelocity;
        public Matrix4 DeviceToAbsolutePose;
        public ETrackingResult TrackingResult;
        public bool IsDeviceConnected;
        public bool IsValid;
    }
}
