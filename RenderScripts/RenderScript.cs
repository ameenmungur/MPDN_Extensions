﻿using System;
using System.IO;

namespace Mpdn.RenderScript
{
    public abstract class RenderScript : IScriptRenderer, IDisposable
    {
        private InputFilter m_InputFilter;

        protected IRenderer Renderer { get; private set; }

        protected virtual string ShaderPath
        {
            get { return GetType().FullName; }
        }

        protected string ShaderDataFilePath
        {
            get
            {
                var asmPath = typeof (IScriptRenderer).Assembly.Location;
                return Path.Combine(Common.GetDirectoryName(asmPath), "RenderScripts", ShaderPath);
            }
        }

        protected IFilter InputFilter
        {
            get { return m_InputFilter ?? (m_InputFilter = new InputFilter(Renderer)); }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract ScriptDescriptor Descriptor { get; }

        public virtual ScriptInputDescriptor InputDescriptor
        {
            get
            {
                return new ScriptInputDescriptor();
            }
        }

        public virtual void Initialize(int instanceId)
        {
        }

        public virtual void Destroy()
        {
        }

        public virtual bool ShowConfigDialog()
        {
            throw new NotImplementedException("Config dialog has not been implemented");
        }

        public virtual void Setup(IRenderer renderer)
        {
            Renderer = renderer;
        }

        public virtual void OnInputSizeChanged()
        {
        }

        public virtual void OnOutputSizeChanged()
        {
        }

        public virtual void Render()
        {
            Scale(Renderer.OutputRenderTarget, GetFrame());
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        ~RenderScript()
        {
            Dispose(false);
        }

        protected abstract ITexture GetFrame();

        protected virtual ITexture GetFrame(IFilter filter)
        {
            filter.NewFrame();
            filter.Render();
            return filter.OutputTexture;
        }

        protected IShader CompileShader(string shaderFileName)
        {
            return Renderer.CompileShader(Path.Combine(ShaderDataFilePath, shaderFileName));
        }

        protected IFilter CreateFilter(IShader shader, IFilter inputFilter)
        {
            return CreateFilter(shader, false, inputFilter);
        }

        protected IFilter CreateFilter(IShader shader, bool linearSampling, IFilter inputFilter)
        {
            if (shader == null)
                throw new ArgumentNullException("shader");

            if (Renderer == null)
                throw new InvalidOperationException("CreateFilter is not available before Setup() is called");

            return new ShaderFilter(Renderer, shader, linearSampling, inputFilter);
        }

        protected IFilter CreateFilter(IShader shader, bool linearSampling, params IFilter[] inputFilters)
        {
            if (shader == null)
                throw new ArgumentNullException("shader");

            if (Renderer == null)
                throw new InvalidOperationException("CreateFilter is not available before Setup() is called");

            return new ShaderFilter(Renderer, shader, linearSampling, inputFilters);
        }

        protected void Scale(ITexture output, ITexture input)
        {
            Renderer.Scale(output, input, Renderer.LumaUpscaler, Renderer.LumaDownscaler);
        }
    }
}