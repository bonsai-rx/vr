using System;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Valve.VR;

namespace Bonsai.VR
{
    public class GetDevicePoses : Combinator<VRDataFrame>
    {
        static string GetHmdString(CVRSystem pHmd, uint unDevice, ETrackedDeviceProperty prop, ref ETrackedPropertyError peError)
        {
            var unRequiredBufferLen = pHmd.GetStringTrackedDeviceProperty(unDevice, prop, null, 0, ref peError);
            if (unRequiredBufferLen == 0)
            {
                return string.Empty;
            }

            var pchBuffer = new StringBuilder();
            unRequiredBufferLen = pHmd.GetStringTrackedDeviceProperty(unDevice, prop, pchBuffer, unRequiredBufferLen, ref peError);
            return pchBuffer.ToString();
        }

        static CVRSystem InitOpenVR()
        {
            var initError = EVRInitError.None;
            var hmd = OpenVR.Init(ref initError, EVRApplicationType.VRApplication_Scene);
            if (initError != EVRInitError.None)
            {
                throw new VRException(OpenVR.GetStringForHmdError(initError));
            }

            var propertyError = ETrackedPropertyError.TrackedProp_Success;
            var driver = GetHmdString(hmd, OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_TrackingSystemName_String, ref propertyError);
            var model = GetHmdString(hmd, OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_ModelNumber_String, ref propertyError);
            var serial = GetHmdString(hmd, OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_SerialNumber_String, ref propertyError);
            var freq = hmd.GetFloatTrackedDeviceProperty(OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_DisplayFrequency_Float, ref propertyError);

            //get the proper resolution of the hmd
            uint hmdWidth = 0, hmdHeight = 0;
            hmd.GetRecommendedRenderTargetSize(ref hmdWidth, ref hmdHeight);
            Console.WriteLine("HMD: {0} '{1}' #{2} ({3} x {4} @ {5} Hz)\n", driver, model, serial, hmdWidth, hmdHeight, freq);

            // Initialize the compositor
            var compositor = OpenVR.Compositor;
            if (compositor == null)
            {
                Console.WriteLine("OpenVR Compositor initialization failed. See log file for details\n");
                OpenVR.Shutdown();
            }

            return hmd;
        }

        public override IObservable<VRDataFrame> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                var hmd = InitOpenVR();

                return source.Select(input =>
                {
                    var poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
                    OpenVR.Compositor.WaitGetPoses(poses, null);

                    var renderPoses = new DevicePose[poses.Length];
                    for (int i = 0; i < renderPoses.Length; i++)
                    {
                        DataHelper.ToVector3(ref poses[i].vVelocity, out renderPoses[i].Velocity);
                        DataHelper.ToVector3(ref poses[i].vAngularVelocity, out renderPoses[i].AngularVelocity);
                        DataHelper.ToMatrix4(ref poses[i].mDeviceToAbsoluteTracking, out renderPoses[i].DeviceToAbsolutePose);
                        renderPoses[i].IsDeviceConnected = poses[i].bDeviceIsConnected;
                        renderPoses[i].TrackingResult = poses[i].eTrackingResult;
                        renderPoses[i].IsValid = poses[i].bPoseIsValid;
                    }

                    return new VRDataFrame(hmd, renderPoses);
                }).Finally(() =>
                {
                    OpenVR.Shutdown();
                });
            });
        }
    }
}
