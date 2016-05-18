using Bonsai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

// TODO: replace this with the transform input and output types.
using TSource = System.String;
using TResult = System.String;
using Valve.VR;
using OpenTK;

namespace Bonsai.VR
{
    public class GetEyeTransformations : Combinator<Tuple<Matrix4, Matrix4, Matrix4, Matrix4>>
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

        static CVRSystem InitOpenVR(ref uint hmdWidth, ref uint hmdHeight)
        {
            EVRInitError eError = EVRInitError.None;
            var hmd = OpenVR.Init(ref eError, EVRApplicationType.VRApplication_Scene);

            if (eError != EVRInitError.None)
            {
                Console.WriteLine("OpenVR Initialization Error: {0}\n", OpenVR.GetStringForHmdError(eError));
                return null;
            }

            ETrackedPropertyError peError = ETrackedPropertyError.TrackedProp_Success;
            var driver = GetHmdString(hmd, OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_TrackingSystemName_String, ref peError);
            var model = GetHmdString(hmd, OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_ModelNumber_String, ref peError);
            var serial = GetHmdString(hmd, OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_SerialNumber_String, ref peError);
            var freq = hmd.GetFloatTrackedDeviceProperty(OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_DisplayFrequency_Float, ref peError);

            //get the proper resolution of the hmd
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

        static void GetEyePoses(
            CVRSystem hmd,
            TrackedDevicePose_t[] trackedDevicePose,
            float nearPlaneZ,
            float farPlaneZ,
            out Matrix4 headPose,
            out Matrix4 ltEyeToHead,
            out Matrix4 rtEyeToHead,
            out Matrix4 ltProjectionMatrix,
            out Matrix4 rtProjectionMatrix)
        {
            OpenVR.Compositor.WaitGetPoses(trackedDevicePose, null);
            var head = trackedDevicePose[OpenVR.k_unTrackedDeviceIndex_Hmd].mDeviceToAbsoluteTracking;
            var ltMatrix = hmd.GetEyeToHeadTransform(EVREye.Eye_Left);
            var rtMatrix = hmd.GetEyeToHeadTransform(EVREye.Eye_Right);

            DataHelper.ToMatrix4(ref head, out headPose);
            DataHelper.ToMatrix4(ref ltMatrix, out ltEyeToHead);
            DataHelper.ToMatrix4(ref rtMatrix, out rtEyeToHead);

            var ltProj = hmd.GetProjectionMatrix(EVREye.Eye_Left, nearPlaneZ, farPlaneZ, EGraphicsAPIConvention.API_OpenGL);
            var rtProj = hmd.GetProjectionMatrix(EVREye.Eye_Right, nearPlaneZ, farPlaneZ, EGraphicsAPIConvention.API_OpenGL);

            DataHelper.ToMatrix4(ref ltProj, out ltProjectionMatrix);
            DataHelper.ToMatrix4(ref rtProj, out rtProjectionMatrix);
        }

        public override IObservable<Tuple<Matrix4, Matrix4, Matrix4, Matrix4>> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                uint width = 0, height = 0;
                var hmd = InitOpenVR(ref width, ref height);

                return source.Select(input =>
                {
                    Matrix4 headPose, ltEyeToHead, rtEyeToHead, ltProjectionMatrix, rtProjectionMatrix;
                    var poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
                    GetEyePoses(hmd, poses, 0.1f, 1000.0f, out headPose, out ltEyeToHead, out rtEyeToHead, out ltProjectionMatrix, out rtProjectionMatrix);

                    var leftView = (ltEyeToHead * headPose).Inverted();
                    var rightView = (rtEyeToHead * headPose).Inverted();
                    return Tuple.Create(leftView, ltProjectionMatrix, rightView, rtProjectionMatrix);
                }).Finally(() =>
                {
                    OpenVR.Shutdown();
                });
            });
        }
    }
}
