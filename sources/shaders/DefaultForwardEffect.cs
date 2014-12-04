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
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.DataModel;
namespace DefaultForward
{
    internal static partial class ShaderMixins
    {
        internal partial class ParadoxLightingTypeShader  : IShaderMixinBuilder
        {
            public void Generate(ShaderMixinSourceTree mixin, ShaderMixinContext context)
            {
                if (context.GetParam(MaterialParameters.LightingType) == MaterialLightingType.DiffusePixel)
                {
                    context.Mixin(mixin, "ShadingDiffusePerPixel");
                }
                else if (context.GetParam(MaterialParameters.LightingType) == MaterialLightingType.DiffuseVertex)
                {
                    context.Mixin(mixin, "ShadingDiffusePerVertex");
                }
                else if (context.GetParam(MaterialParameters.LightingType) == MaterialLightingType.DiffuseSpecularPixel)
                {
                    context.Mixin(mixin, "ShadingDiffuseSpecularPerPixel");
                }
                else if (context.GetParam(MaterialParameters.LightingType) == MaterialLightingType.DiffuseVertexSpecularPixel)
                {
                    context.Mixin(mixin, "ShadingDiffusePerVertexSpecularPerPixel");
                }
            }

            [ModuleInitializer]
            internal static void __Initialize__()

            {
                ShaderMixinManager.Register("ParadoxLightingTypeShader", new ParadoxLightingTypeShader());
            }
        }
    }
    internal static partial class ShaderMixins
    {
        internal partial class ParadoxPointLightsShader  : IShaderMixinBuilder
        {
            public void Generate(ShaderMixinSourceTree mixin, ShaderMixinContext context)
            {
                mixin.Mixin.AddMacro("LIGHTING_MAX_LIGHT_COUNT", context.GetParam(LightingKeys.MaxPointLights));
                context.Mixin(mixin, "ParadoxLightingTypeShader");
                context.Mixin(mixin, "PointShading");
                context.Mixin(mixin, "ShadingEyeNormalVS");
            }

            [ModuleInitializer]
            internal static void __Initialize__()

            {
                ShaderMixinManager.Register("ParadoxPointLightsShader", new ParadoxPointLightsShader());
            }
        }
    }
    internal static partial class ShaderMixins
    {
        internal partial class ParadoxSpotLightsShader  : IShaderMixinBuilder
        {
            public void Generate(ShaderMixinSourceTree mixin, ShaderMixinContext context)
            {
                mixin.Mixin.AddMacro("LIGHTING_MAX_LIGHT_COUNT", context.GetParam(LightingKeys.MaxSpotLights));
                context.Mixin(mixin, "ParadoxLightingTypeShader");
                context.Mixin(mixin, "SpotShading");
                context.Mixin(mixin, "ShadingEyeNormalVS");
            }

            [ModuleInitializer]
            internal static void __Initialize__()

            {
                ShaderMixinManager.Register("ParadoxSpotLightsShader", new ParadoxSpotLightsShader());
            }
        }
    }
    internal static partial class ShaderMixins
    {
        internal partial class ParadoxDirectionalLightsShader  : IShaderMixinBuilder
        {
            public void Generate(ShaderMixinSourceTree mixin, ShaderMixinContext context)
            {
                mixin.Mixin.AddMacro("LIGHTING_MAX_LIGHT_COUNT", context.GetParam(LightingKeys.MaxDirectionalLights));
                context.Mixin(mixin, "ParadoxLightingTypeShader");
                context.Mixin(mixin, "DirectionalShading");
                context.Mixin(mixin, "ShadingEyeNormalVS");
            }

            [ModuleInitializer]
            internal static void __Initialize__()

            {
                ShaderMixinManager.Register("ParadoxDirectionalLightsShader", new ParadoxDirectionalLightsShader());
            }
        }
    }
    internal static partial class ShaderMixins
    {
        internal partial class ParadoxDirectionalShadowLightsShader  : IShaderMixinBuilder
        {
            public void Generate(ShaderMixinSourceTree mixin, ShaderMixinContext context)
            {
                mixin.Mixin.AddMacro("LIGHTING_MAX_LIGHT_COUNT", context.GetParam(ShadowMapParameters.ShadowMapCount));
                context.Mixin(mixin, "ParadoxLightingTypeShader");
                context.Mixin(mixin, "ShadingPerPixelShadow");
                context.Mixin(mixin, "DirectionalShading");
                context.Mixin(mixin, "ShadingEyeNormalVS");
                context.Mixin(mixin, "ForwardShadowMapBase");
                context.Mixin(mixin, "ShadowMapCascadeBase");
                mixin.Mixin.AddMacro("SHADOWMAP_COUNT", context.GetParam(ShadowMapParameters.ShadowMapCount));
                mixin.Mixin.AddMacro("SHADOWMAP_CASCADE_COUNT", context.GetParam(ShadowMapParameters.ShadowMapCascadeCount));
                mixin.Mixin.AddMacro("SHADOWMAP_TOTAL_COUNT", context.GetParam(ShadowMapParameters.ShadowMapCount) * context.GetParam(ShadowMapParameters.ShadowMapCascadeCount));
                if (context.GetParam(ShadowMapParameters.FilterType) == ShadowMapFilterType.Nearest)
                    context.Mixin(mixin, "ShadowMapFilterDefault");
                else if (context.GetParam(ShadowMapParameters.FilterType) == ShadowMapFilterType.PercentageCloserFiltering)
                    context.Mixin(mixin, "ShadowMapFilterPcf");
                else if (context.GetParam(ShadowMapParameters.FilterType) == ShadowMapFilterType.Variance)
                    context.Mixin(mixin, "ShadowMapFilterVsm");
            }

            [ModuleInitializer]
            internal static void __Initialize__()

            {
                ShaderMixinManager.Register("ParadoxDirectionalShadowLightsShader", new ParadoxDirectionalShadowLightsShader());
            }
        }
    }
    internal static partial class ShaderMixins
    {
        internal partial class ParadoxSpotShadowLightsShader  : IShaderMixinBuilder
        {
            public void Generate(ShaderMixinSourceTree mixin, ShaderMixinContext context)
            {
                mixin.Mixin.AddMacro("LIGHTING_MAX_LIGHT_COUNT", context.GetParam(ShadowMapParameters.ShadowMapCount));
                context.Mixin(mixin, "ParadoxLightingTypeShader");
                context.Mixin(mixin, "ShadingPerPixelShadow");
                context.Mixin(mixin, "SpotShading");
                context.Mixin(mixin, "ShadingEyeNormalVS");
                context.Mixin(mixin, "ForwardShadowMapBase");
                context.Mixin(mixin, "ShadowMapCascadeBase");
                mixin.Mixin.AddMacro("SHADOWMAP_COUNT", context.GetParam(ShadowMapParameters.ShadowMapCount));
                mixin.Mixin.AddMacro("SHADOWMAP_CASCADE_COUNT", context.GetParam(ShadowMapParameters.ShadowMapCascadeCount));
                mixin.Mixin.AddMacro("SHADOWMAP_TOTAL_COUNT", context.GetParam(ShadowMapParameters.ShadowMapCount) * context.GetParam(ShadowMapParameters.ShadowMapCascadeCount));
                if (context.GetParam(ShadowMapParameters.FilterType) == ShadowMapFilterType.Nearest)
                    context.Mixin(mixin, "ShadowMapFilterDefault");
                else if (context.GetParam(ShadowMapParameters.FilterType) == ShadowMapFilterType.PercentageCloserFiltering)
                    context.Mixin(mixin, "ShadowMapFilterPcf");
                else if (context.GetParam(ShadowMapParameters.FilterType) == ShadowMapFilterType.Variance)
                    context.Mixin(mixin, "ShadowMapFilterVsm");
            }

            [ModuleInitializer]
            internal static void __Initialize__()

            {
                ShaderMixinManager.Register("ParadoxSpotShadowLightsShader", new ParadoxSpotShadowLightsShader());
            }
        }
    }
    internal static partial class ShaderMixins
    {
        internal partial class ParadoxDiffuseForward  : IShaderMixinBuilder
        {
            public void Generate(ShaderMixinSourceTree mixin, ShaderMixinContext context)
            {
                context.Mixin(mixin, "BRDFDiffuseBase");
                context.Mixin(mixin, "BRDFSpecularBase");
                if (context.GetParam(MaterialParameters.AlbedoDiffuse) != null)
                {
                    context.Mixin(mixin, "AlbedoDiffuseBase");

                    {
                        var __subMixin = new ShaderMixinSourceTree() { Parent = mixin };
                        context.PushComposition(mixin, "albedoDiffuse", __subMixin);
                        context.Mixin(__subMixin, context.GetParam(MaterialParameters.AlbedoDiffuse));
                        context.PopComposition();
                    }
                    if (context.GetParam(LightingKeys.MaxDirectionalLights) > 0 || context.GetParam(LightingKeys.MaxSpotLights) > 0 || context.GetParam(LightingKeys.MaxPointLights) > 0 || (context.GetParam(LightingKeys.ReceiveShadows) && context.GetParam(ShadowMapParameters.ShadowMaps) != null && context.GetParam(ShadowMapParameters.ShadowMaps).Length > 0))
                    {
                        if (context.GetParam(LightingKeys.MaxDirectionalLights) > 0 || context.GetParam(LightingKeys.MaxSpotLights) > 0 || context.GetParam(LightingKeys.MaxPointLights) > 0)
                        {
                            context.Mixin(mixin, "GroupShadingBase");
                            if (context.GetParam(LightingKeys.MaxDirectionalLights) > 0)

                                {
                                    var __subMixin = new ShaderMixinSourceTree() { Parent = mixin };
                                    context.PushCompositionArray(mixin, "ShadingGroups", __subMixin);
                                    context.Mixin(__subMixin, "ParadoxDirectionalLightsShader");
                                    context.PopComposition();
                                }
                            if (context.GetParam(LightingKeys.MaxSpotLights) > 0)

                                {
                                    var __subMixin = new ShaderMixinSourceTree() { Parent = mixin };
                                    context.PushCompositionArray(mixin, "ShadingGroups", __subMixin);
                                    context.Mixin(__subMixin, "ParadoxSpotLightsShader");
                                    context.PopComposition();
                                }
                            if (context.GetParam(LightingKeys.MaxPointLights) > 0)

                                {
                                    var __subMixin = new ShaderMixinSourceTree() { Parent = mixin };
                                    context.PushCompositionArray(mixin, "ShadingGroups", __subMixin);
                                    context.Mixin(__subMixin, "ParadoxPointLightsShader");
                                    context.PopComposition();
                                }
                        }
                        if (context.GetParam(LightingKeys.ReceiveShadows) && context.GetParam(ShadowMapParameters.ShadowMaps) != null && context.GetParam(ShadowMapParameters.ShadowMaps).Length > 0)
                        {
                            context.Mixin(mixin, "ShadowMapReceiver");
                            foreach(var ____1 in context.GetParam(ShadowMapParameters.ShadowMaps))

                            {
                                context.PushParameters(____1);
                                if (context.GetParam(ShadowMapParameters.LightType) == LightType.Directional)

                                    {
                                        var __subMixin = new ShaderMixinSourceTree() { Parent = mixin };
                                        context.PushCompositionArray(mixin, "shadows", __subMixin);
                                        context.Mixin(__subMixin, "ParadoxDirectionalShadowLightsShader");
                                        context.PopComposition();
                                    }
                                else if (context.GetParam(ShadowMapParameters.LightType) == LightType.Spot)

                                    {
                                        var __subMixin = new ShaderMixinSourceTree() { Parent = mixin };
                                        context.PushCompositionArray(mixin, "shadows", __subMixin);
                                        context.Mixin(__subMixin, "ParadoxSpotShadowLightsShader");
                                        context.PopComposition();
                                    }
                                context.PopParameters();
                            }
                        }
                        if (context.GetParam(MaterialParameters.DiffuseModel) == MaterialDiffuseModel.None || context.GetParam(MaterialParameters.DiffuseModel) == MaterialDiffuseModel.Lambert)
                        {

                            {
                                var __subMixin = new ShaderMixinSourceTree() { Parent = mixin };
                                context.PushComposition(mixin, "DiffuseLighting", __subMixin);
                                context.Mixin(__subMixin, "ComputeBRDFDiffuseLambert");
                                context.PopComposition();
                            }
                        }
                        else if (context.GetParam(MaterialParameters.DiffuseModel) == MaterialDiffuseModel.OrenNayar)
                        {

                            {
                                var __subMixin = new ShaderMixinSourceTree() { Parent = mixin };
                                context.PushComposition(mixin, "DiffuseLighting", __subMixin);
                                context.Mixin(__subMixin, "ComputeBRDFDiffuseOrenNayar");
                                context.PopComposition();
                            }
                        }
                    }
                    else
                    {
                        context.Mixin(mixin, "AlbedoFlatShading");
                    }
                }
            }

            [ModuleInitializer]
            internal static void __Initialize__()

            {
                ShaderMixinManager.Register("ParadoxDiffuseForward", new ParadoxDiffuseForward());
            }
        }
    }
    internal static partial class ShaderMixins
    {
        internal partial class ParadoxSpecularLighting  : IShaderMixinBuilder
        {
            public void Generate(ShaderMixinSourceTree mixin, ShaderMixinContext context)
            {
                if (context.GetParam(MaterialParameters.SpecularModel) == MaterialSpecularModel.None || context.GetParam(MaterialParameters.SpecularModel) == MaterialSpecularModel.Phong)
                {
                    context.Mixin(mixin, "ComputeBRDFColorSpecularPhong");
                }
                else if (context.GetParam(MaterialParameters.SpecularModel) == MaterialSpecularModel.BlinnPhong)
                {
                    context.Mixin(mixin, "ComputeBRDFColorSpecularBlinnPhong");
                }
                else if (context.GetParam(MaterialParameters.SpecularModel) == MaterialSpecularModel.CookTorrance)
                {
                    context.Mixin(mixin, "ComputeBRDFColorSpecularCookTorrance");
                }
                if (context.GetParam(MaterialParameters.SpecularPowerMap) != null)
                {
                    context.Mixin(mixin, "SpecularPower");

                    {
                        var __subMixin = new ShaderMixinSourceTree() { Parent = mixin };
                        context.PushComposition(mixin, "SpecularPowerMap", __subMixin);
                        context.Mixin(__subMixin, context.GetParam(MaterialParameters.SpecularPowerMap));
                        context.PopComposition();
                    }
                }
                if (context.GetParam(MaterialParameters.SpecularIntensityMap) != null)
                {

                    {
                        var __subMixin = new ShaderMixinSourceTree() { Parent = mixin };
                        context.PushComposition(mixin, "SpecularIntensityMap", __subMixin);
                        context.Mixin(__subMixin, context.GetParam(MaterialParameters.SpecularIntensityMap));
                        context.PopComposition();
                    }
                }
            }

            [ModuleInitializer]
            internal static void __Initialize__()

            {
                ShaderMixinManager.Register("ParadoxSpecularLighting", new ParadoxSpecularLighting());
            }
        }
    }
    internal static partial class ShaderMixins
    {
        internal partial class ParadoxSpecularForward  : IShaderMixinBuilder
        {
            public void Generate(ShaderMixinSourceTree mixin, ShaderMixinContext context)
            {
                context.Mixin(mixin, "BRDFDiffuseBase");
                context.Mixin(mixin, "BRDFSpecularBase");
                if (context.GetParam(MaterialParameters.AlbedoSpecular) != null)
                {
                    context.Mixin(mixin, "AlbedoSpecularBase");

                    {
                        var __subMixin = new ShaderMixinSourceTree() { Parent = mixin };
                        context.PushComposition(mixin, "albedoSpecular", __subMixin);
                        context.Mixin(__subMixin, context.GetParam(MaterialParameters.AlbedoSpecular));
                        context.PopComposition();
                    }

                    {
                        var __subMixin = new ShaderMixinSourceTree() { Parent = mixin };
                        context.PushComposition(mixin, "SpecularLighting", __subMixin);
                        context.Mixin(__subMixin, "ParadoxSpecularLighting");
                        context.PopComposition();
                    }
                }
            }

            [ModuleInitializer]
            internal static void __Initialize__()

            {
                ShaderMixinManager.Register("ParadoxSpecularForward", new ParadoxSpecularForward());
            }
        }
    }
    internal static partial class ShaderMixins
    {
        internal partial class ParadoxDefaultForwardShader  : IShaderMixinBuilder
        {
            public void Generate(ShaderMixinSourceTree mixin, ShaderMixinContext context)
            {
                context.Mixin(mixin, "ParadoxBaseShader");
                context.Mixin(mixin, "ParadoxSkinning");
                context.Mixin(mixin, "ParadoxShadowCast");
                context.Mixin(mixin, "ParadoxDiffuseForward");
                context.Mixin(mixin, "ParadoxSpecularForward");
                if (context.GetParam(MaterialParameters.AmbientMap) != null)
                {
                    context.Mixin(mixin, "AmbientMapShading");

                    {
                        var __subMixin = new ShaderMixinSourceTree() { Parent = mixin };
                        context.PushComposition(mixin, "AmbientMap", __subMixin);
                        context.Mixin(__subMixin, context.GetParam(MaterialParameters.AmbientMap));
                        context.PopComposition();
                    }
                }
                if (context.GetParam(MaterialParameters.UseTransparentMask))
                {
                    context.Mixin(mixin, "TransparentShading");
                    context.Mixin(mixin, "DiscardTransparentThreshold", context.GetParam(MaterialParameters.AlphaDiscardThreshold));
                }
                else if (context.GetParam(MaterialParameters.UseTransparent))
                {
                    context.Mixin(mixin, "TransparentShading");
                    context.Mixin(mixin, "DiscardTransparent");
                }
            }

            [ModuleInitializer]
            internal static void __Initialize__()

            {
                ShaderMixinManager.Register("ParadoxDefaultForwardShader", new ParadoxDefaultForwardShader());
            }
        }
    }
}
