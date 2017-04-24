using OpenTK;
using Valve.VR;

namespace Bonsai.VR
{
    public class ControllerState
    {
        public Matrix4 ControllerPose;
        public uint PacketNumber;
        public VRControllerAxis_t Axis0; //For Vive: (x,y) = TouchPad x,y
        public VRControllerAxis_t Axis1; //For Vive: x = Trigger position
        public VRControllerAxis_t Axis2; //For vive not used
        public VRControllerAxis_t Axis3; //For vive not used
        public VRControllerAxis_t Axis4; //For vive not used
        public ulong ButtonPressed;
        public ulong ButtonTouched;
        public bool IsValid;

        public bool GetButtonPressed(VRButtonId button)
        {
            return (ButtonPressed & (1ul << (int)button)) != 0;
        }

        public bool GetButtonTouched(VRButtonId button)
        {
            return (ButtonTouched & (1ul << (int)button)) != 0;
        }
    }

    public enum VRButtonId
    {
        System = EVRButtonId.k_EButton_System,
        ApplicationMenu = EVRButtonId.k_EButton_ApplicationMenu,
        Grip = EVRButtonId.k_EButton_Grip,
        DPadLeft = EVRButtonId.k_EButton_DPad_Left,
        DPadUp = EVRButtonId.k_EButton_DPad_Up,
        DPadRight = EVRButtonId.k_EButton_DPad_Right,
        DPadDown = EVRButtonId.k_EButton_DPad_Down,
        A = EVRButtonId.k_EButton_A,
        Axis0 = EVRButtonId.k_EButton_Axis0,
        Axis1 = EVRButtonId.k_EButton_Axis1,
        Axis2 = EVRButtonId.k_EButton_Axis2,
        Axis3 = EVRButtonId.k_EButton_Axis3,
        Axis4 = EVRButtonId.k_EButton_Axis4,
        SteamVRTouchpad = EVRButtonId.k_EButton_SteamVR_Touchpad,
        SteamVRTrigger = EVRButtonId.k_EButton_SteamVR_Trigger,
        DashboardBack = EVRButtonId.k_EButton_Dashboard_Back
    }
}
