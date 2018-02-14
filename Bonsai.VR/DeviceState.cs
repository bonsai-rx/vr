using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace Bonsai.VR
{
    public class DeviceState
    {
        public int DeviceIndex;
        public Vector3 Velocity;
        public Vector3 AngularVelocity;
        public Matrix4 DevicePose;
        public bool IsValid;

        public void TriggerHapticPulse(int axis, int durationMicroseconds)
        {
            var system = OpenVR.System;
            if (system != null)
            {
                system.TriggerHapticPulse((uint)DeviceIndex, (uint)axis, (ushort)durationMicroseconds);
            }
        }
    }
}
