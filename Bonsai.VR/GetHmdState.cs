using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace Bonsai.VR
{
    [Description("Extracts the head pose, eye view and projection matrices from the specified VR system data.")]
    public class GetHmdState : Transform<VRDataFrame, HmdState>
    {
        public GetHmdState()
        {
            NearClip = 0.1f;
            FarClip = 100f;
        }

        [Description("The distance to the near clip plane.")]
        public float NearClip { get; set; }

        [Description("The distance to the far clip plane.")]
        public float FarClip { get; set; }

        static void GetEyePoses(
            CVRSystem system,
            float nearPlaneZ,
            float farPlaneZ,
            out Matrix4 ltEyeToHead,
            out Matrix4 rtEyeToHead,
            out Matrix4 ltProjectionMatrix,
            out Matrix4 rtProjectionMatrix)
        {
            var ltMatrix = system.GetEyeToHeadTransform(EVREye.Eye_Left);
            var rtMatrix = system.GetEyeToHeadTransform(EVREye.Eye_Right);
            DataHelper.ToMatrix4(ref ltMatrix, out ltEyeToHead);
            DataHelper.ToMatrix4(ref rtMatrix, out rtEyeToHead);

            var ltProj = system.GetProjectionMatrix(EVREye.Eye_Left, nearPlaneZ, farPlaneZ);
            var rtProj = system.GetProjectionMatrix(EVREye.Eye_Right, nearPlaneZ, farPlaneZ);
            DataHelper.ToMatrix4(ref ltProj, out ltProjectionMatrix);
            DataHelper.ToMatrix4(ref rtProj, out rtProjectionMatrix);
        }

        public override IObservable<HmdState> Process(IObservable<VRDataFrame> source)
        {
            return Observable.Defer(() =>
            {
                Matrix4 leftEyeToHead, rightEyeToHead;
                return source.Select(input =>
                {
                    var state = new HmdState();
                    state.IsValid = input.RenderPoses[OpenVR.k_unTrackedDeviceIndex_Hmd].IsValid;
                    state.Velocity = input.RenderPoses[OpenVR.k_unTrackedDeviceIndex_Hmd].Velocity;
                    state.AngularVelocity = input.RenderPoses[OpenVR.k_unTrackedDeviceIndex_Hmd].AngularVelocity;
                    state.DevicePose = input.RenderPoses[OpenVR.k_unTrackedDeviceIndex_Hmd].DeviceToAbsolutePose;
                    GetEyePoses(input.System, NearClip, FarClip, out leftEyeToHead, out rightEyeToHead, out state.LeftProjectionMatrix, out state.RightProjectionMatrix);

                    Matrix4.Mult(ref leftEyeToHead, ref state.DevicePose, out state.LeftViewMatrix);
                    Matrix4.Mult(ref rightEyeToHead, ref state.DevicePose, out state.RightViewMatrix);
                    state.LeftViewMatrix.Invert();
                    state.RightViewMatrix.Invert();
                    return state;
                });
            });
        }
    }
}
