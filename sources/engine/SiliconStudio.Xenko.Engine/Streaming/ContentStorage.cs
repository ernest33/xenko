﻿// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Streaming;

namespace SiliconStudio.Xenko.Streaming
{
    /// <summary>
    /// Streamable resources content storage containter.
    /// </summary>
    public class ContentStorage
    {
        private readonly ContentChunk[] chunks;
        private long locks;

        /// <summary>
        /// The content streaming service which manages this storage container.
        /// </summary>
        public ContentStreamingService Service { get; }

        /// <summary>
        /// Gets the storage URL path.
        /// </summary>
        public string Url { get; }
        
        /// <summary>
        /// Gets the time when container has been created.
        /// </summary>
        public DateTime PackageTime { get; }

        /// <summary>
        /// Gets the last access time.
        /// </summary>
        public DateTime LastAccessTime
        {
            get
            {
                var result = chunks[0].LastAccessTime;
                for (int i = 1; i < chunks.Length; i++)
                {
                    if (result < chunks[i].LastAccessTime)
                        result = chunks[i].LastAccessTime;
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the amount of chunks located inside the storage container.
        /// </summary>
        public int ChunksCount => chunks.Length;

        internal ContentStorage(ContentStreamingService service, ContentStorageHeader header)
        {
            // Init
            Service = service;
            Url = header.DataUrl;
            chunks = new ContentChunk[header.ChunksCount];
            for (int i = 0; i < chunks.Length; i++)
            {
                var e = header.Chunks[i];
                chunks[i] = new ContentChunk(this, e.Location, e.Size);
            }
            PackageTime = header.PackageTime;

            // Validate hash code
            if(GetHashCode() != header.HashCode)
                throw new DataMisalignedException();
        }

        /// <summary>
        /// Gets the chunk.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Chunk</returns>
        public ContentChunk GetChunk(int index)
        {
            Debug.Assert(index >= 0 && chunks.Length > index);

            var chunk = chunks[index];
            chunk.RegisterUsage();
            return chunk;
        }

        internal void ReleaseUnusedChunks()
        {
            if (Interlocked.Read(ref locks) != 0)
                return;

            var now = DateTime.UtcNow;
            foreach (var chunk in chunks)
            {
                if (chunk.IsLoaded && now - chunk.LastAccessTime >= Service.UnusedDataChunksLifetime)
                    chunk.Unload();
            }
        }

        internal void LockChunks()
        {
            Interlocked.Increment(ref locks);
        }

        internal void UnlockChunks()
        {
            Interlocked.Decrement(ref locks);
        }

        /// <summary>
        /// Creates the new storage container at the specified location and generates header for that.
        /// </summary>
        /// <param name="dataUrl">The file url.</param>
        /// <param name="chunksData">The chunks data.</param>
        /// <param name="header">The header data.</param>
        public static void Create(string dataUrl, List<byte[]> chunksData, out ContentStorageHeader header)
        {
            if (chunksData == null || chunksData.Count == 0 || chunksData.Any(x => x == null || x.Length == 0))
                throw new ArgumentException(nameof(chunksData));

            var packageTime = DateTime.UtcNow;

            // Sort chunks (smaller ones go first)
            int chunksCount = chunksData.Count;
            List<int> chunksOrder = new List<int>(chunksCount);
            for (int i = 0; i < chunksCount; i++)
                chunksOrder.Add(i);
            chunksOrder.Sort((a, b) => chunksData[a].Length - chunksData[b].Length);

            // Calculate first chunk location (in file)
            /*int offset =
                // Version
                sizeof(int)
                // Package Time (ticks)
                + sizeof(long)
                // Chunks count
                + sizeof(int)
                // Header hash code
                + sizeof(int)
                // Chunk locations and sizes
                + (sizeof(int) + sizeof(int)) * chunksCount;*/
            int offset = 0;

            // Calculate header hash code (used to provide simple data verification during loading)
            // Note: this must match ContentStorage.GetHashCode()
            int hashCode = (int)packageTime.Ticks;
            hashCode = (hashCode * 397) ^ chunksCount;
            for (int i = 0; i < chunksCount; i++)
                hashCode = (hashCode * 397) ^ chunksData[i].Length;

            // Create header
            header = new ContentStorageHeader
            {
                DataUrl = dataUrl,
                PackageTime = packageTime,
                HashCode = hashCode,
                Chunks = new ContentStorageHeader.ChunkEntry[chunksCount]
            };
            for (int i = 0; i < chunksCount; i++)
            {
                int chunkIndex = chunksOrder[i];
                int size = chunksData[chunkIndex].Length;
                header.Chunks[chunkIndex].Location = offset;
                header.Chunks[chunkIndex].Size = size;
                offset += size;
            }

            // Create file with a raw data
            using (var outputStream = ContentManager.FileProvider.OpenStream(dataUrl, VirtualFileMode.Create, VirtualFileAccess.Write, VirtualFileShare.Read, StreamFlags.Seekable))
            using (var stream = new BinaryWriter(outputStream))
            {
                // Write data (one after another)
                for (int i = 0; i < chunksCount; i++)
                    stream.Write(chunksData[chunksOrder[i]]);

                // Validate calculated offset
                if (offset != outputStream.Position)
                    throw new DataException("Invalid storage offset.");
            }
        }

        /// <inheritdoc/>
        public sealed override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int)PackageTime.Ticks;
                hashCode = (hashCode * 397) ^ chunks.Length;
                for (int i = 0; i < chunks.Length; i++)
                    hashCode = (hashCode * 397) ^ chunks[i].Size;
                return hashCode;
            }
        }
    }
}