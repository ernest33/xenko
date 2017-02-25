﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Assets.Scripts;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Compositing;

namespace SiliconStudio.Xenko.Assets.Rendering
{
    // TODO: this list is here just to easily identify object references. It can be removed once we have access to the path from IsObjectReference
    [DataContract]
    public class RenderStageCollection : List<RenderStage>
    {
    }

    // TODO: this list is here just to easily identify object references. It can be removed once we have access to the path from IsObjectReference
    [DataContract]
    public class SharedRendererCollection : TrackingCollection<ISharedRenderer>
    {
    }

    [DataContract("GraphicsCompositorAsset")]
    [Display(82, "Graphics Compositor")]
    [AssetContentType(typeof(GraphicsCompositor))]
    [AssetDescription(FileExtension)]
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion)]
    [AssetCompiler(typeof(GraphicsCompositorAssetCompiler))]
    [AssetUpgrader(XenkoConfig.PackageName, "0.0.0", "1.10.0-beta01", typeof(FixPartReferenceUpgrader))]
    public class GraphicsCompositorAsset : AssetComposite
    {
        private const string CurrentVersion = "1.10.0-beta01";

        /// <summary>
        /// The default file extension used by the <see cref="GraphicsCompositorAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkgfxcomp";

        /// <summary>
        /// Gets the cameras used by this composition.
        /// </summary>
        /// <value>The cameras.</value>
        /// <userdoc>The list of cameras used in the graphic pipeline</userdoc>
        [Category("Camera Slots")]
        [MemberCollection(CanReorderItems = true, NotNullItems = true)]
        public SceneCameraSlotCollection Cameras { get; } = new SceneCameraSlotCollection();

        /// <summary>
        /// The list of render stages.
        /// </summary>
        [Category]
        [MemberCollection(CanReorderItems = true, NotNullItems = true)]
        //[AssetPartContained(typeof(RenderStage))]
        public RenderStageCollection RenderStages { get; } = new RenderStageCollection();

        /// <summary>
        /// The list of render features.
        /// </summary>
        [Category]
        [MemberCollection(CanReorderItems = true, NotNullItems = true)]
        //[AssetPartContained(typeof(RootRenderFeature))]
        public List<RootRenderFeature> RenderFeatures { get; } = new List<RootRenderFeature>();

        /// <summary>
        /// The list of graphics compositors.
        /// </summary>
        [Category]
        [MemberCollection(CanReorderItems = true, NotNullItems = true)]
        //[AssetPartContained(typeof(ISharedRenderer))]
        public SharedRendererCollection SharedRenderers { get; } = new SharedRendererCollection();

        /// <summary>
        /// The entry point for the game compositor.
        /// </summary>
        public ISceneRenderer Game { get; set; }

        /// <summary>
        /// The entry point for a compositor that can render a single view.
        /// </summary>
        public ISceneRenderer SingleView { get; set; }

        /// <summary>
        /// The entry point for a compositor used by the scene editor.
        /// </summary>
        public ISceneRenderer Editor { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<AssetPart> CollectParts()
        {
            foreach (var renderStage in RenderStages)
                yield return new AssetPart(renderStage.Id, null, newBase => {});
            foreach (var sharedRenderer in SharedRenderers)
                yield return new AssetPart(sharedRenderer.Id, null, newBase => { });
        }

        /// <inheritdoc/>
        public override IIdentifiable FindPart(Guid partId)
        {
            foreach (var renderStage in RenderStages)
            {
                if (renderStage.Id == partId)
                    return renderStage;
            }

            foreach (var sharedRenderer in SharedRenderers)
            {
                if (sharedRenderer.Id == partId)
                    return sharedRenderer;
            }

            return null;
        }

        /// <inheritdoc/>
        public override bool ContainsPart(Guid partId)
        {
            foreach (var renderStage in RenderStages)
            {
                if (renderStage.Id == partId)
                    return true;
            }
            foreach (var sharedRenderer in SharedRenderers)
            {
                if (sharedRenderer.Id == partId)
                    return true;
            }

            return false;
        }

        public GraphicsCompositor Compile(bool copyRenderers)
        {
            var graphicsCompositor = new GraphicsCompositor();

            foreach (var cameraSlot in Cameras)
                graphicsCompositor.Cameras.Add(cameraSlot);
            foreach (var renderStage in RenderStages)
                graphicsCompositor.RenderStages.Add(renderStage);
            foreach (var renderFeature in RenderFeatures)
                graphicsCompositor.RenderFeatures.Add(renderFeature);

            if (copyRenderers)
            {
                graphicsCompositor.Game = Game;
                graphicsCompositor.SingleView = SingleView;
            }

            return graphicsCompositor;
        }
    }
}
