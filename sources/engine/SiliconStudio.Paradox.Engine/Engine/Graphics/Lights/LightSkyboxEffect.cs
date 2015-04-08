﻿// <auto-generated>
// Do not edit this file yourself!
//
// This code was generated by Paradox Shader Mixin Code Generator.
// To generate it yourself, please install SiliconStudio.Paradox.VisualStudio.Package .vsix
// and re-save the associated .pdxfx.
// </auto-generated>

using System;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;
using SiliconStudio.Core.Mathematics;
using Buffer = SiliconStudio.Paradox.Graphics.Buffer;

using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Materials;
namespace SiliconStudio.Paradox.Effects.Lights
{
    internal static partial class ShaderMixins
    {
        internal partial class LightSkyboxEffect  : IShaderMixinBuilder
        {
            public void Generate(ShaderMixinSource mixin, ShaderMixinContext context)
            {
                context.Mixin(mixin, "LightSkyboxShader");
                if (context.GetParam(LightSkyboxShaderKeys.LightDiffuseColor) != null)
                {

                    {
                        var __subMixin = new ShaderMixinSource();
                        context.PushComposition(mixin, "lightDiffuseColor", __subMixin);
                        context.Mixin(__subMixin, context.GetParam(LightSkyboxShaderKeys.LightDiffuseColor));
                        context.PopComposition();
                    }
                }
                if (context.GetParam(LightSkyboxShaderKeys.LightSpecularColor) != null)
                {

                    {
                        var __subMixin = new ShaderMixinSource();
                        context.PushComposition(mixin, "lightSpecularColor", __subMixin);
                        context.Mixin(__subMixin, context.GetParam(LightSkyboxShaderKeys.LightSpecularColor));
                        context.PopComposition();
                    }
                }
            }

            [ModuleInitializer]
            internal static void __Initialize__()

            {
                ShaderMixinManager.Register("LightSkyboxEffect", new LightSkyboxEffect());
            }
        }
    }
}
