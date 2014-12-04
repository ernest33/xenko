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
namespace SiliconStudio.Paradox.Effects.Core
{
    internal static partial class ShaderMixins
    {
        internal partial class ParadoxTessellation  : IShaderMixinBuilder
        {
            public void Generate(ShaderMixinSourceTree mixin, ShaderMixinContext context)
            {
            }

            [ModuleInitializer]
            internal static void __Initialize__()

            {
                ShaderMixinManager.Register("ParadoxTessellation", new ParadoxTessellation());
            }
        }
    }
    internal static partial class ShaderMixins
    {
        internal partial class ParadoxSkinning  : IShaderMixinBuilder
        {
            public void Generate(ShaderMixinSourceTree mixin, ShaderMixinContext context)
            {
                if (context.GetParam(MaterialParameters.HasSkinningPosition))
                {
                    if (context.GetParam(MaterialParameters.SkinningBones) > context.GetParam(MaterialParameters.SkinningMaxBones))
                    {
                        context.SetParam(MaterialParameters.SkinningMaxBones, context.GetParam(MaterialParameters.SkinningBones));
                    }
                    mixin.Mixin.AddMacro("SkinningMaxBones", context.GetParam(MaterialParameters.SkinningMaxBones));
                    context.Mixin(mixin, "TransformationSkinning");
                    if (context.GetParam(MaterialParameters.HasSkinningNormal))
                    {
                        if (context.GetParam(MaterialParameters.NormalMap) != null)
                            context.Mixin(mixin, "TangentToViewSkinning");
                        else
                            context.Mixin(mixin, "NormalVSSkinning");
                        context.Mixin(mixin, "NormalSkinning");
                    }
                    if (context.GetParam(MaterialParameters.HasSkinningTangent))
                        context.Mixin(mixin, "TangentSkinning");
                }
            }

            [ModuleInitializer]
            internal static void __Initialize__()

            {
                ShaderMixinManager.Register("ParadoxSkinning", new ParadoxSkinning());
            }
        }
    }
    internal static partial class ShaderMixins
    {
        internal partial class ParadoxShadowCast  : IShaderMixinBuilder
        {
            public void Generate(ShaderMixinSourceTree mixin, ShaderMixinContext context)
            {
                if (context.GetParam(LightingKeys.CastShadows))

                    {
                        var __subMixin = new ShaderMixinSourceTree() { Name = "ShadowMapCaster" };
                        context.BeginChild(__subMixin);
                        context.Mixin(__subMixin, "ShadowMapCaster");
                        context.EndChild();
                    }
            }

            [ModuleInitializer]
            internal static void __Initialize__()

            {
                ShaderMixinManager.Register("ParadoxShadowCast", new ParadoxShadowCast());
            }
        }
    }
    internal static partial class ShaderMixins
    {
        internal partial class ParadoxBaseShader  : IShaderMixinBuilder
        {
            public void Generate(ShaderMixinSourceTree mixin, ShaderMixinContext context)
            {
                context.Mixin(mixin, "ShaderBase");
                context.Mixin(mixin, "ShadingBase");
                context.Mixin(mixin, "TransformationWAndVP");
                context.Mixin(mixin, "PositionVSStream");
                if (context.GetParam(MaterialParameters.NormalMap) != null)
                {
                    context.Mixin(mixin, "NormalMapTexture");

                    {
                        var __subMixin = new ShaderMixinSourceTree() { Parent = mixin };
                        context.PushComposition(mixin, "normalMap", __subMixin);
                        context.Mixin(__subMixin, context.GetParam(MaterialParameters.NormalMap));
                        context.PopComposition();
                    }
                }
                else
                {
                    context.Mixin(mixin, "NormalVSStream");
                }
            }

            [ModuleInitializer]
            internal static void __Initialize__()

            {
                ShaderMixinManager.Register("ParadoxBaseShader", new ParadoxBaseShader());
            }
        }
    }
}
