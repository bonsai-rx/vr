using Bonsai.Shaders;
using Bonsai.Shaders.Configuration;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.VR
{
    public class BindFramebuffer : Combinator
    {
        readonly FramebufferConfiguration framebuffer = new FramebufferConfiguration();

        [Editor("Bonsai.Shaders.Configuration.Design.ShaderConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string BindShader { get; set; }

        [Editor("Bonsai.Shaders.Configuration.Design.ShaderConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string UnbindShader { get; set; }

        [Description("Specifies any framebuffer attachments that are required to run the shader.")]
        [Editor("Bonsai.Shaders.Configuration.Design.FramebufferAttachmentConfigurationCollectionEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public Collection<FramebufferAttachmentConfiguration> FramebufferAttachments
        {
            get { return framebuffer.FramebufferAttachments; }
        }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return ShaderManager.WindowSource.SelectMany(window => Observable.FromEventPattern<EventArgs>(
                handler => window.Load += handler,
                handler => window.Unload += handler)
                .SelectMany(evt =>
                   ShaderManager.ReserveShader(BindShader).SelectMany(bindShader =>
                   ShaderManager.ReserveShader(UnbindShader).SelectMany(unbindShader =>
            {
                return Observable.Defer(() =>
                {
                    bindShader.Update(() => framebuffer.Load(bindShader));
                    return source.Do(input =>
                    {
                        bindShader.Update(() => framebuffer.Bind(bindShader));
                        unbindShader.Update(() => framebuffer.Unbind(unbindShader));
                    }).Finally(() =>
                    {
                        framebuffer.Unload(bindShader);
                    });
                });
            }))));
        }
    }
}
