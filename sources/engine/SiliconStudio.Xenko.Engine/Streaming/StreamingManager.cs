// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Streaming;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Data;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Streaming
{
    /// <summary>
    /// Performs content streaming.
    /// </summary>
    /// <seealso cref="SiliconStudio.Xenko.Games.GameSystemBase" />
    /// <seealso cref="SiliconStudio.Xenko.Graphics.Data.ITexturesStreamingProvider" />
    public class StreamingManager : GameSystemBase, IStreamingManager, ITexturesStreamingProvider
    {
        private readonly List<StreamableResource> resources = new List<StreamableResource>(512);
        private readonly Dictionary<object, StreamableResource> resourcesLookup = new Dictionary<object, StreamableResource>(512);
        private readonly List<StreamableResource> priorityUpdateQueue = new List<StreamableResource>(64); // Could be Queue<T> but it doesn't support .Remove(T)
        private int lastUpdateResourcesIndex;
        private bool isDisposing;
        private int frameIndex;

        // Configuration
        public TimeSpan ManagerUpdatesInterval = TimeSpan.FromMilliseconds(33);
        public TimeSpan ResourceUpdatesInterval = TimeSpan.FromMilliseconds(200);
        public const int MaxResourcesPerUpdate = 30;

        /// <summary>
        /// Gets the content streaming service.
        /// </summary>
        public ContentStreamingService ContentStreaming { get; }

        /// <summary>
        /// List with all registered streamable resources.
        /// </summary>
        public ICollection<StreamableResource> Resources => resources;

        /// <summary>
        /// Gets or sets a value indicating whether resources streaming should be disabled.
        /// </summary>
        public bool DisableStreaming { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingManager"/> class.
        /// </summary>
        /// <param name="services">The servicies registry.</param>
        /// <remarks>
        /// The GameSystem is expecting the following services to be registered: <see cref="T:SiliconStudio.Xenko.Games.IGame" /> and <see cref="T:SiliconStudio.Core.Serialization.Contents.IContentManager" />.
        /// </remarks>
        public StreamingManager(IServiceRegistry services) : base(services)
        {
            services.AddService(typeof(StreamingManager), this);
            services.AddService(typeof(IStreamingManager), this);
            services.AddService(typeof(ITexturesStreamingProvider), this);

            ContentStreaming = new ContentStreamingService();

            (Game as Game)?.Script.AddTask(Update);
        }

        /// <inheritdoc />
        protected override void Destroy()
        {
            isDisposing = true;

            if (Services.GetService(typeof(StreamingManager)) == this)
            {
                Services.RemoveService(typeof(StreamingManager));
            }
            if (Services.GetService(typeof(IStreamingManager)) == this)
            {
                Services.RemoveService(typeof(IStreamingManager));
            }
            if (Services.GetService(typeof(ITexturesStreamingProvider)) == this)
            {
                Services.RemoveService(typeof(ITexturesStreamingProvider));
            }

            lock (resources)
            {
                resources.ForEach(x => x.Release());
                resources.Clear();
                resourcesLookup.Clear();
                priorityUpdateQueue.Clear();
            }

            ContentStreaming.Dispose();

            base.Destroy();
        }

        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T Get<T>(object obj) where T : StreamableResource
        {
            StreamableResource result;
            resourcesLookup.TryGetValue(obj, out result);
            return result as T;
        }

        internal void RegisterResource(StreamableResource resource)
        {
            Debug.Assert(resource != null && isDisposing == false);

            lock (resources)
            {
                Debug.Assert(!resources.Contains(resource));

                resources.Add(resource);
                resourcesLookup.Add(resource.Resource, resource);
            }
        }

        internal void UnregisterResource(StreamableResource resource)
        {
            if (isDisposing)
                return;

            Debug.Assert(resource != null);

            lock (resources)
            {
                Debug.Assert(resources.Contains(resource));

                resources.Remove(resource);
                resourcesLookup.Remove(resource.Resource);
                priorityUpdateQueue.RemoveAll(x => x == resource);
            }
        }

        private StreamingTexture CreateStreamingTexture(Texture obj, ref ImageDescription imageDescription, ref ContentStorageHeader storageHeader)
        {
            // Get content storage container
            var storage = ContentStreaming.GetStorage(ref storageHeader);
            if (storage == null)
                throw new ContentStreamingException("Missing content storage.");

            // Find resource or create new
            var resource = Get<StreamingTexture>(obj);
            if (resource == null)
            {
                resource = new StreamingTexture(this, obj);
                RegisterResource(resource);
            }

            // Update resource storage/description information (may be modified on asset rebuilding)
            resource.Init(storage, ref imageDescription);
            
            // Check if cannot use streaming
            if (DisableStreaming)
            {
                FullyLoadResource(resource);
            }

            return resource;
        }

        /// <summary>
        /// Requests the streamable resource update.
        /// </summary>
        /// <param name="resource">The resource to update.</param>
        public void RequestUpdate(StreamableResource resource)
        {
            lock (resources)
            {
                priorityUpdateQueue.Add(resource);
            }
        }

        /// <inheritdoc />
        public void FullyLoadTexture(Texture obj, ref ImageDescription imageDescription, ref ContentStorageHeader storageHeader)
        {
            lock (resources)
            {
                // Get streaming object
                var resource = CreateStreamingTexture(obj, ref imageDescription, ref storageHeader);

                // Stream resource to the maximum level
                FullyLoadResource(resource);

                // Release streaming object
                resource.Release();
            }
        }

        /// <inheritdoc />
        public void RegisterTexture(Texture obj, ref ImageDescription imageDescription, ref ContentStorageHeader storageHeader)
        {
            lock (resources)
            {
                // Get streaming object
                var resource = CreateStreamingTexture(obj, ref imageDescription, ref storageHeader);

                // Register quicker update for that resource
                RequestUpdate(resource);
            }
        }

        /// <inheritdoc />
        public void UnregisterTexture(Texture obj)
        {
            Debug.Assert(obj != null);

            lock (resources)
            {
                var resource = Get<StreamingTexture>(obj);
                resource?.Dispose();
            }
        }

        /// <inheritdoc />
        public void FullyLoadResource(object obj)
        {
            StreamableResource resource;
            lock (resources)
            {
                resource = Get<StreamableResource>(obj);
            }

            if (resource != null)
                FullyLoadResource(resource);
        }

        public void FullyLoadResource(StreamableResource resource)
        {
            // Disable dynamic streaming for the esource
            resource.ForceFullyLoaded = true;

            // Stream resource to the maximum level
            var task = resource.StreamAsync(resource.MaxResidency);
            task.Start();
            task.Wait();
        }

        private async Task Update()
        {
            while (!IsDisposed)
            {
                // temp for testing...
                /*if (!((Game)Game).Input.IsKeyDown(Keys.P))
                {
                    ContentStreaming.Update();
                    await ((Game)Game).Script.NextFrame();
                    continue;
                }*/

                // temp code for testing quality changing using K/L keys
                /*if (((Game)Game).Input.IsKeyPressed(Keys.K))
                {
                    testQuality = Math.Min(testQuality + 5, 100);
                }
                if (((Game)Game).Input.IsKeyPressed(Keys.L))
                {
                    testQuality = Math.Max(testQuality - 5, 0);
                }*/

                // Update resources
                lock (resources)
                {
                    int resourcesCount = Resources.Count;
                    if (resourcesCount > 0)
                    {
                        var now = DateTime.UtcNow;
                        int resourcesUpdates = Math.Min(MaxResourcesPerUpdate, resourcesCount);

                        // Update high priority queue and then rest of the resources
                        // Note: resources in the update queue are updated always, while others only between specified intervals
                        int resourcesChecks = resourcesCount - priorityUpdateQueue.Count;
                        while (priorityUpdateQueue.Count > 0 && resourcesUpdates-- > 0)
                        {
                            var resource = priorityUpdateQueue[0];
                            priorityUpdateQueue.RemoveAt(0);
                            if (resource.CanBeUpdated)
                                Update(resource, ref now);
                        }
                        while (resourcesUpdates > 0 && resourcesChecks-- > 0)
                        {
                            // Move forward
                            // Note: we update resources like in a ring buffer
                            lastUpdateResourcesIndex++;
                            if (lastUpdateResourcesIndex >= resourcesCount)
                                lastUpdateResourcesIndex = 0;

                            // Peek resource
                            var resource = resources[lastUpdateResourcesIndex];

                            // Try to update it
                            if (now - resource.LastUpdate >= ResourceUpdatesInterval && resource.CanBeUpdated)
                            {
                                Update(resource, ref now);
                                resourcesUpdates--;
                            }
                        }

                        // TODO: add StreamingManager stats, update time per frame, updates per frame, etc.
                    }
                }

                ContentStreaming.Update();

                frameIndex++;
                await Task.Delay(ManagerUpdatesInterval);
            }
        }

        private void Update(StreamableResource resource, ref DateTime now)
        {
            Debug.Assert(resource != null && resource.CanBeUpdated);

            // Pick group and handler dedicated for that resource
            // TODO: finish resource groups and streaming handlers implementation
            //var group = resource.Group;
            //var handler = group.Handler;

            // Calculate target quality for that asset
            StreamingQuality targetQuality = StreamingQuality.Mininum;
            if (resource.ForceFullyLoaded)
            {
                targetQuality = StreamingQuality.Maximum;
            }
            else if (resource.LastTimeUsed > 0)
            {
                var lastUsageTimespan = new TimeSpan((frameIndex - resource.LastTimeUsed) * ManagerUpdatesInterval.Ticks);
                if (lastUsageTimespan < TimeSpan.FromSeconds(5))
                {
                    //targetQuality = handler.CalculateTargetQuality(resource, now);
                    //targetQuality = (testQuality / 100.0f); // apply quality scale for testing
                    targetQuality = StreamingQuality.Maximum;
                    // TODO: here we should apply resources group master scale (based on game settings quality level and memory level)
                }
            }
            targetQuality.Normalize();

            // Calculate target residency level (discrete value)
            var currentResidency = resource.CurrentResidency;
            var allocatedResidency = resource.AllocatedResidency;
            var targetResidency = resource.CalculateResidency(targetQuality);
            Debug.Assert(allocatedResidency >= currentResidency && allocatedResidency >= 0);

            // Update target residency smoothing
            // TODO: use move quality samples and use max or avg value - make that input it smooth - or use PID
            //resource.QualitySamples.Add(targetResidency);
            //targetResidency = resource.QualitySamples.Maximum();

            // Assign last update time
            resource.LastUpdate = now;

            // Check if a target residency level has been changed
            if (targetResidency != resource.TargetResidency)
            {
                // Register change
                resource.TargetResidency = targetResidency;
                resource.TargetResidencyChange = now;
            }

            // Check if need to change resource current residency
            if (targetResidency != currentResidency)
            {
                // Check if need to increase it's residency
                if (targetResidency > currentResidency)
                {
                    // Calculate residency level to stream in (resources may want to incease/decrease it's quality in steps rather than at once)
                    //var requestedResidency = handler.CalculateRequestedResidency(resource, targetResidency);// TODO: use resource groups and handlers
                    var requestedResidency = Math.Min(targetResidency, Math.Max(currentResidency + 1, 4)); // Stream target quality in steps but lower mips at once
                    //var requestedResidency = currentResidency + 1; // Stream target quality in steps
                    //var requestedResidency = targetResidency; // Stream target quality at once

                    // Create streaming task (resource type specific)
                    resource.StreamAsync(requestedResidency).Start();
                }
                else
                {
                    // Calculate residency level to stream in (resources may want to incease/decrease it's quality in steps rather than at once)
                    //var requestedResidency = handler.CalculateRequestedResidency(resource, targetResidency);// TODO: use resource groups and handlers
                    var requestedResidency = targetResidency; // Stream target quality at once

                    // Spawn streaming task (resource type specific)
                    resource.StreamAsync(requestedResidency).Start();
                }
            }
        }

        /// <summary>
        /// Called when render mesh is submited to rendering. Registers referenced resources to stream them.
        /// </summary>
        /// <param name="renderMesh">The render mesh.</param>
        public void StreamResources(RenderMesh renderMesh)
        {
            if (renderMesh.MaterialPass != null)
            {
                StreamResources(renderMesh.MaterialPass.Parameters);
            }

            StreamResources(renderMesh.RenderModel.Model);
        }

        /// <summary>
        /// Called when material parameters are submited to rendering. Registers referenced resources to stream them.
        /// </summary>
        /// <param name="parameters">The material parameters.</param>
        public void StreamResources(ParameterCollection parameters)
        {
            if (parameters.ObjectValues == null)
                return;

            // Register all binded textures
            foreach (var e in parameters.ObjectValues)
            {
                if (e is Texture t)
                {
                    var resource = Get<StreamingTexture>(t);
                    if (resource != null)
                    {
                        resource.LastTimeUsed = frameIndex;
                    }
                }
            }
        }

        /// <summary>
        /// Called when texture is submited to be used during rendering. Registers referenced resources to stream them.
        /// </summary>
        /// <param name="texture">The texture.</param>
        public void StreamResources(Texture texture)
        {
            if (texture == null)
                return;

            var resource = Get<StreamingTexture>(texture);
            if (resource != null)
            {
                resource.LastTimeUsed = frameIndex;
            }
        }

        /// <summary>
        /// Called when model is submited to be used during rendering. Registers referenced resources to stream them.
        /// </summary>
        /// <param name="model">The model.</param>
        public void StreamResources(Model model)
        {
            // TODO: register model streaming
        }

        /// <summary>
        /// Called when render mesh is submited to rendering. Registers referenced resources to stream them up to the maximum quality level.
        /// </summary>
        /// <param name="renderMesh">The render mesh.</param>
        public void StreamResourcesFully(RenderMesh renderMesh)
        {
            if (renderMesh.MaterialPass != null)
            {
                StreamResourcesFully(renderMesh.MaterialPass.Parameters);
            }

            StreamResourcesFully(renderMesh.RenderModel.Model);
        }

        /// <summary>
        /// Called when material parameters are submited to rendering. Registers referenced resources to stream them up to the maximum quality level.
        /// </summary>
        /// <param name="parameters">The material parameters.</param>
        public void StreamResourcesFully(ParameterCollection parameters)
        {
            if (parameters.ObjectValues == null)
                return;

            // Register all binded textures
            foreach (var e in parameters.ObjectValues)
            {
                if (e is Texture t)
                {
                    var resource = Get<StreamingTexture>(t);
                    if (resource != null)
                    {
                        resource.ForceFullyLoaded = true;
                        resource.LastTimeUsed = frameIndex;
                    }
                }
            }
        }

        /// <summary>
        /// Called when texture is submited to be used during rendering. Registers referenced resources to stream them up to the maximum quality level.
        /// </summary>
        /// <param name="texture">The texture.</param>
        public void StreamResourcesFully(Texture texture)
        {
            if (texture == null)
                return;

            var resource = Get<StreamingTexture>(texture);
            if (resource != null)
            {
                resource.ForceFullyLoaded = true;
                resource.LastTimeUsed = frameIndex;
            }
        }

        /// <summary>
        /// Called when model is submited to be used during rendering. Registers referenced resources to stream them up to the maximum quality level.
        /// </summary>
        /// <param name="model">The model.</param>
        public void StreamResourcesFully(Model model)
        {
            // TODO: register model streaming
        }
    }
}
