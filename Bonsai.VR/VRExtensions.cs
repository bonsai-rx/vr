using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace Bonsai.VR
{
    static class VRExtensions
    {
        public static string GetStringTrackedDeviceProperty(this CVRSystem hmd, uint unDeviceIndex, ETrackedDeviceProperty prop)
        {
            var error = ETrackedPropertyError.TrackedProp_Success;
            var builder = new StringBuilder((int)OpenVR.k_unMaxPropertyStringSize);
            var result = hmd.GetStringTrackedDeviceProperty(unDeviceIndex, prop, builder, OpenVR.k_unMaxPropertyStringSize, ref error);
            if (error != ETrackedPropertyError.TrackedProp_Success)
            {
                throw new VRException(hmd.GetPropErrorNameFromEnum(error));
            }
            return builder.ToString();
        }
    }
}
