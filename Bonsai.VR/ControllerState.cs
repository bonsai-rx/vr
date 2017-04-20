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
        public VRControllerAxis_t Axis0; //For Vive: (x,y) = TouchPad x,y
        public VRControllerAxis_t Axis1; //For Vive: x = Trigger position
        public VRControllerAxis_t Axis2; //For vive not used
        public VRControllerAxis_t Axis3; //For vive not used
        public VRControllerAxis_t Axis4; //For vive not used

        public ulong ButtonPressedId;
        public ulong ButtonTouchedId;
        public string ButtonPressedName;
        public string ButtonTouchedName;
        public bool IsValid;
    }
}
