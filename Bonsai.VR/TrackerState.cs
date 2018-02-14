using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
