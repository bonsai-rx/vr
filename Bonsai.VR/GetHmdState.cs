﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace Bonsai.VR
{
    public class GetHmdState : Transform<VRDataFrame, HmdState>
    {
        public GetHmdState()
        {
            NearClip = 0.1f;
            FarClip = 100f;
        }

        public float NearClip { get; set; }

        public float FarClip { get; set; }

        static void GetEyePoses(
            CVRSystem hmd,
            float nearPlaneZ,
            float farPlaneZ,
            out Matrix4 ltEyeToHead,
            out Matrix4 rtEyeToHead,
            out Matrix4 ltProjectionMatrix,
            out Matrix4 rtProjectionMatrix)
        {
            var ltMatrix = hmd.GetEyeToHeadTransform(EVREye.Eye_Left);
            var rtMatrix = hmd.GetEyeToHeadTransform(EVREye.Eye_Right);
            DataHelper.ToMatrix4(ref ltMatrix, out ltEyeToHead);
            DataHelper.ToMatrix4(ref rtMatrix, out rtEyeToHead);

            var ltProj = hmd.GetProjectionMatrix(EVREye.Eye_Left, nearPlaneZ, farPlaneZ, EGraphicsAPIConvention.API_OpenGL);
            var rtProj = hmd.GetProjectionMatrix(EVREye.Eye_Right, nearPlaneZ, farPlaneZ, EGraphicsAPIConvention.API_OpenGL);
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
                    var poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
                    state.HeadPose = input.RenderPoses[OpenVR.k_unTrackedDeviceIndex_Hmd].DeviceToAbsolutePose;
                    GetEyePoses(input.Hmd, NearClip, FarClip, out leftEyeToHead, out rightEyeToHead, out state.LeftProjectionMatrix, out state.RightProjectionMatrix);

                    Matrix4.Mult(ref leftEyeToHead, ref state.HeadPose, out state.LeftViewMatrix);
                    Matrix4.Mult(ref rightEyeToHead, ref state.HeadPose, out state.RightViewMatrix);
                    state.LeftViewMatrix.Invert();
                    state.RightViewMatrix.Invert();
                    return state;
                });
            });
        }
    }
}