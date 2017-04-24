using OpenTK;
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
