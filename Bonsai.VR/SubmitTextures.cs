using Bonsai.Shaders;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive.Linq;
using Valve.VR;

namespace Bonsai.VR
{
    [Description("Submits the specified left and right eye textures to the headset for presentation.")]
    public class SubmitTextures : Sink<Tuple<Texture, Texture>>
    {
        public override IObservable<Tuple<Texture, Texture>> Process(IObservable<Tuple<Texture, Texture>> source)
        {
            return source.Do(input =>
            {
                Texture_t leftEye, rightEye;
                leftEye.handle = (IntPtr)input.Item1.Id;
                rightEye.handle = (IntPtr)input.Item2.Id;
                leftEye.eColorSpace = rightEye.eColorSpace = EColorSpace.Auto;
                leftEye.eType = rightEye.eType = EGraphicsAPIConvention.API_OpenGL;
                OpenVR.Compositor.Submit(EVREye.Eye_Left, ref leftEye, IntPtr.Zero, EVRSubmitFlags.Submit_Default);
                OpenVR.Compositor.Submit(EVREye.Eye_Right, ref rightEye, IntPtr.Zero, EVRSubmitFlags.Submit_Default);
                OpenVR.Compositor.PostPresentHandoff();
            });
        }
    }
}
