using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace Bonsai.VR
{
    public class ControllerState
    {
        public Matrix4 ControllerPose;
        public uint PacketNum;
        public ulong ButtonPressed;
        public ulong ButtonTouched;
        public VRControllerAxis_t Axis0; //VRControllerAxis_t[5]
        public VRControllerAxis_t Axis1;
        public VRControllerAxis_t Axis2;
        public VRControllerAxis_t Axis3;
        public VRControllerAxis_t Axis4;
    }
}
