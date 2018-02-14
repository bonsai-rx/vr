using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Valve.VR;

namespace Bonsai.VR
{
    [Description("Produces a sequence of events with the latest VR system state.")]
    public class GetDevicePoses : Combinator<VRDataFrame>
    {
        public GetDevicePoses()
        {
            ApplicationType = EVRApplicationType.VRApplication_Scene;
        }

        public EVRApplicationType ApplicationType { get; set; }

        static string GetHmdString(CVRSystem system, uint unDevice, ETrackedDeviceProperty prop, ref ETrackedPropertyError peError)
        {
            var unRequiredBufferLen = system.GetStringTrackedDeviceProperty(unDevice, prop, null, 0, ref peError);
            if (unRequiredBufferLen == 0)
            {
                return string.Empty;
            }

            var pchBuffer = new StringBuilder();
            unRequiredBufferLen = system.GetStringTrackedDeviceProperty(unDevice, prop, pchBuffer, unRequiredBufferLen, ref peError);
            return pchBuffer.ToString();
        }

        static CVRSystem InitOpenVR(EVRApplicationType applicationType)
        {
            var initError = EVRInitError.None;
            var system = OpenVR.Init(ref initError, applicationType);
            if (initError != EVRInitError.None)
            {
                throw new VRException(OpenVR.GetStringForHmdError(initError));
            }

            if (applicationType == EVRApplicationType.VRApplication_Scene)
            {
                var propertyError = ETrackedPropertyError.TrackedProp_Success;
                var driver = GetHmdString(system, OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_TrackingSystemName_String, ref propertyError);
                var model = GetHmdString(system, OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_ModelNumber_String, ref propertyError);
                var serial = GetHmdString(system, OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_SerialNumber_String, ref propertyError);
                var freq = system.GetFloatTrackedDeviceProperty(OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_DisplayFrequency_Float, ref propertyError);

                //get the proper resolution of the hmd
                uint hmdWidth = 0, hmdHeight = 0;
                system.GetRecommendedRenderTargetSize(ref hmdWidth, ref hmdHeight);
                Console.WriteLine("HMD: {0} '{1}' #{2} ({3} x {4} @ {5} Hz)\n", driver, model, serial, hmdWidth, hmdHeight, freq);

                // Initialize the compositor
                var compositor = OpenVR.Compositor;
                if (compositor == null)
                {
                    Console.WriteLine("OpenVR Compositor initialization failed. See log file for details\n");
                    OpenVR.Shutdown();
                }
            }

            return system;
        }

        public override IObservable<VRDataFrame> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                var applicationType = ApplicationType;
                var system = InitOpenVR(applicationType);

                return source.Select(input =>
                {
                    var poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
                    if (applicationType == EVRApplicationType.VRApplication_Scene) OpenVR.Compositor.WaitGetPoses(poses, null);
                    else system.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, poses);

                    var renderPoses = new TrackedDevicePose[poses.Length];
                    for (int i = 0; i < renderPoses.Length; i++)
                    {
                        DataHelper.ToVector3(ref poses[i].vVelocity, out renderPoses[i].Velocity);
                        DataHelper.ToVector3(ref poses[i].vAngularVelocity, out renderPoses[i].AngularVelocity);
                        DataHelper.ToMatrix4(ref poses[i].mDeviceToAbsoluteTracking, out renderPoses[i].DeviceToAbsolutePose);
                        renderPoses[i].IsDeviceConnected = poses[i].bDeviceIsConnected;
                        renderPoses[i].TrackingResult = poses[i].eTrackingResult;
                        renderPoses[i].IsValid = poses[i].bPoseIsValid;
                    }

                    return new VRDataFrame(system, renderPoses);
                }).Finally(() =>
                {
                    OpenVR.Shutdown();
                });
            });
        }
    }
}
