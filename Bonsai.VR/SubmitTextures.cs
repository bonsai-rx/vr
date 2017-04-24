﻿using Bonsai.Shaders;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive.Linq;
using Valve.VR;

namespace Bonsai.VR
{
    public class SubmitTextures : Sink
    {
        [Editor("Bonsai.Shaders.Configuration.Design.TextureConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string LeftEyeTexture { get; set; }

        [Editor("Bonsai.Shaders.Configuration.Design.TextureConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string RightEyeTexture { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                var leftEyeTexture = 0;
                var leftName = LeftEyeTexture;
                if (string.IsNullOrEmpty(leftName))
                {
                    throw new InvalidOperationException("A texture sampler name must be specified.");
                }

                var rightEyeTexture = 0;
                var rightName = RightEyeTexture;
                if (string.IsNullOrEmpty(rightName))
                {
                    throw new InvalidOperationException("A texture sampler name must be specified.");
                }

                return source.CombineEither(
                    ShaderManager.WindowSource.Do(window =>
                    {
                        window.Update(() =>
                        {
                            var tex = window.Textures[leftName];
                            if (tex == null)
                            {
                                throw new InvalidOperationException(string.Format(
                                    "The texture \"{0}\" was not found.",
                                    leftName));
                            }

                            leftEyeTexture = tex.Id;

                            tex = window.Textures[rightName];
                            if (tex == null)
                            {
                                throw new InvalidOperationException(string.Format(
                                    "The texture \"{0}\" was not found.",
                                    leftName));
                            }

                            rightEyeTexture = tex.Id;
                        });
                    }),
                    (input, window) =>
                    {
                        Texture_t leftEye, rightEye;
                        leftEye.handle = (IntPtr)leftEyeTexture;
                        rightEye.handle = (IntPtr)rightEyeTexture;
                        leftEye.eColorSpace = rightEye.eColorSpace = EColorSpace.Auto;
                        leftEye.eType = rightEye.eType = EGraphicsAPIConvention.API_OpenGL;
                        OpenVR.Compositor.Submit(EVREye.Eye_Left, ref leftEye, IntPtr.Zero, EVRSubmitFlags.Submit_Default);
                        OpenVR.Compositor.Submit(EVREye.Eye_Right, ref rightEye, IntPtr.Zero, EVRSubmitFlags.Submit_Default);
                        OpenVR.Compositor.PostPresentHandoff();
                        return input;
                    });
            });
        }
    }
}
