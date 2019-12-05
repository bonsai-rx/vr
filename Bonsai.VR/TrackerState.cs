using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace Bonsai.VR
{
    public class TrackerState : DeviceState
    {
        public uint PacketNumber;
        public ulong ButtonPressed;

        public bool GetButtonPressed(VRButtonId button)
        {
            return (ButtonPressed & (1ul << (int)button)) != 0;
        }

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
