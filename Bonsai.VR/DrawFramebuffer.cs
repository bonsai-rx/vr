using Bonsai.Shaders;
using Bonsai.Shaders.Configuration;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Valve.VR;

namespace Bonsai.VR
{
    [Description("Renders all currently stored draw commands to a VR compatible framebuffer.")]
    public class DrawFramebuffer : Combinator<Texture>
    {
        readonly StateConfigurationCollection renderState = new StateConfigurationCollection();

        public DrawFramebuffer()
        {
            ClearColor = Color.Black;
            ClearMask = ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit;
        }

        [Category("State")]
        [XmlArrayItem(typeof(EnableState))]
        [XmlArrayItem(typeof(DisableState))]
        [XmlArrayItem(typeof(ViewportState))]
        [XmlArrayItem(typeof(LineWidthState))]
        [XmlArrayItem(typeof(PointSizeState))]
        [XmlArrayItem(typeof(DepthMaskState))]
        [XmlArrayItem(typeof(BlendFunctionState))]
        [XmlArrayItem(typeof(DepthFunctionState))]
        [XmlArrayItem(typeof(MemoryBarrierState))]
        [Description("Specifies any render states that are required to render the framebuffer.")]
        [Editor("Bonsai.Shaders.Configuration.Design.StateConfigurationCollectionEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public StateConfigurationCollection RenderState
        {
            get { return renderState; }
        }

        [XmlIgnore]
        [Description("The color used to clear the framebuffer before rendering.")]
        public Color ClearColor { get; set; }

        [Browsable(false)]
        [XmlElement("ClearColor")]
        public string ClearColorHtml
        {
            get { return ColorTranslator.ToHtml(ClearColor); }
            set { ClearColor = ColorTranslator.FromHtml(value); }
        }

        [Description("Specifies which buffers to clear before rendering.")]
        public ClearBufferMask ClearMask { get; set; }

        Texture CreateRenderTarget(int width, int height, PixelInternalFormat internalFormat, PixelFormat format, PixelType type)
        {
            var texture = new Texture();
            GL.BindTexture(TextureTarget.Texture2D, texture.Id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(
                TextureTarget.Texture2D, 0,
                internalFormat, width, height, 0,
                format,
                type,
                IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            return texture;
        }

        int CreateFramebuffer(Texture colorTarget, Texture depthTarget)
        {
            int fbo;
            GL.GenFramebuffers(1, out fbo);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTarget.Id, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthTarget.Id, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            return fbo;
        }

        void UpdateViewport(RectangleF viewport, float width, float height)
        {
            GL.Viewport(
                (int)(viewport.X * width),
                (int)(viewport.Y * height),
                (int)(viewport.Width * width),
                (int)(viewport.Height * height));
        }

        public override IObservable<Texture> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                var fbo = 0;
                var width = 0;
                var height = 0;
                var colorTarget = default(Texture);
                var depthTarget = default(Texture);
                return source.CombineEither(
                    ShaderManager.WindowSource.Do(window =>
                    {
                        window.Update(() =>
                        {
                            var targetWidth = 0u;
                            var targetHeight = 0u;
                            var system = OpenVR.System;
                            system.GetRecommendedRenderTargetSize(ref targetWidth, ref targetHeight);
                            width = (int)targetWidth;
                            height = (int)targetHeight;
                            colorTarget = CreateRenderTarget(width, height, PixelInternalFormat.Rgba, PixelFormat.Rgba, PixelType.UnsignedByte);
                            depthTarget = CreateRenderTarget(width, height, PixelInternalFormat.DepthComponent24, PixelFormat.DepthComponent, PixelType.Int);
                            fbo = CreateFramebuffer(colorTarget, depthTarget);
                        });
                    }),
                    (input, window) =>
                    {
                        foreach (var state in renderState)
                        {
                            state.Execute(window);
                        }

                        var clearMask = ClearMask;
                        GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
                        UpdateViewport(window.Viewport, width, height);
                        if (clearMask != ClearBufferMask.None)
                        {
                            GL.ClearColor(ClearColor);
                            GL.Clear(clearMask);
                        }
                        
                        foreach (var shader in window.Shaders)
                        {
                            shader.Dispatch();
                        }

                        UpdateViewport(window.Viewport, window.Width, window.Height);
                        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                        return colorTarget;
                    }).Finally(() =>
                    {
                        if (fbo > 0)
                        {
                            GL.DeleteFramebuffers(1, ref fbo);
                            colorTarget.Dispose();
                            depthTarget.Dispose();
                        }
                    });
            });
        }
    }
}
