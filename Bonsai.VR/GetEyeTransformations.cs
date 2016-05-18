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

        static void ToMatrix4(ref HmdMatrix44_t matrix, out Matrix4 result)
        {
            result = new Matrix4(
                matrix.m0,
                matrix.m1,
                matrix.m2,
                matrix.m3,
                matrix.m4,
                matrix.m5,
                matrix.m6,
                matrix.m7,
                matrix.m8,
                matrix.m9,
                matrix.m10,
                matrix.m11,
                matrix.m12,
                matrix.m13,
                matrix.m14,
                matrix.m15
            );
        }

        static void ToMatrix4(ref HmdMatrix34_t matrix, out Matrix4 result)
        {
            result = new Matrix4(
                matrix.m0,
                matrix.m1,
                matrix.m2,
                matrix.m3,
                matrix.m4,
                matrix.m5,
                matrix.m6,
                matrix.m7,
                matrix.m8,
                matrix.m9,
                matrix.m10,
                matrix.m11,
                0,
                0,
                0,
                1
            );
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

            ToMatrix4(ref head, out headPose);
            ToMatrix4(ref ltMatrix, out ltEyeToHead);
            ToMatrix4(ref rtMatrix, out rtEyeToHead);
            headPose.Transpose();
            ltEyeToHead.Transpose();
            rtEyeToHead.Transpose();

            var ltProj = hmd.GetProjectionMatrix(EVREye.Eye_Left, nearPlaneZ, farPlaneZ, EGraphicsAPIConvention.API_OpenGL);
            var rtProj = hmd.GetProjectionMatrix(EVREye.Eye_Right, nearPlaneZ, farPlaneZ, EGraphicsAPIConvention.API_OpenGL);

            ToMatrix4(ref ltProj, out ltProjectionMatrix);
            ToMatrix4(ref rtProj, out rtProjectionMatrix);
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
                    var leftProjection = ltProjectionMatrix;
                    var rightProjection = rtProjectionMatrix;
                    leftProjection.Transpose();
                    rightProjection.Transpose();
                    return Tuple.Create(leftView, leftProjection, rightView, rightProjection);
                }).Finally(() =>
                {
                    OpenVR.Shutdown();
                });
            });
        }
    }
}
