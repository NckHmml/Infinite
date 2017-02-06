﻿using Infinite.Shaders;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using static SiliconStudio.Xenko.Graphics.Buffer;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace Infinite.Terrain
{
    public class TerrainDrawer
    {
        private Dictionary<TerrainTexture, HashSet<int>> PlaneMap { get; } = new Dictionary<TerrainTexture, HashSet<int>>();
        private Dictionary<int, TerrainPlane> Planes { get; } = new Dictionary<int, TerrainPlane>();
        private Dictionary<TerrainTexture, VertexBufferBinding> VertexBuffers { get; } = new Dictionary<TerrainTexture, VertexBufferBinding>();

        private const int PlaneStep = 1000;
        private int PlaneCount = 0;
        private int VertexSize = TerrainVertex.Layout.CalculateSize();
        private Object PlaneLock = new Object();

        private Buffer Indices;
        private MutablePipelineState PipelineState;
        private GraphicsContext Context;

        public TerrainDrawer(GraphicsContext context)
        {
            Context = context;
        }

        public void Initialize(IEnumerable<TerrainPlane> planes = null)
        {
            GraphicsDevice device = Context.CommandList.GraphicsDevice;

            PipelineState = new MutablePipelineState(device);
            PipelineState.State.InputElements = TerrainVertex.Layout.CreateInputElements();
            PipelineState.State.SetDefaults();
            PipelineState.State.PrimitiveType = PrimitiveType.TriangleList;

            int count = planes?.Count() ?? 0;
            count = count - count % PlaneStep;
            count += PlaneStep;
            InitialAddPlanes(planes);
            CreateIndices(device, count);
        }

        public void AddPlane(TerrainPlane plane)
        {
            lock (PlaneLock)
            {
                TerrainTexture key = plane.Texture;
                EnsureCapacity(key);
                SetVertices(plane, PlaneMap[key].Count);

                if (!PlaneMap.ContainsKey(key))
                    PlaneMap.Add(key, new HashSet<int>());
                PlaneMap[key].Add(PlaneCount);

                Planes.Add(PlaneCount, plane);
                PlaneCount++;
            }
        }

        public IEnumerable<Entity> CreateEntities()
        {
            foreach (TerrainTexture textureKey in PlaneMap.Keys)
            {
                var model = new Model();
                model.Add(new Mesh
                {
                    Draw = new MeshDraw
                    {
                        PrimitiveType = PrimitiveType.TriangleList,
                        VertexBuffers = new[] { VertexBuffers[textureKey] },
                        IndexBuffer = new IndexBufferBinding(Indices, true, Indices.ElementCount),
                        DrawCount = PlaneMap[textureKey].Count * 6
                    },
                });
                model.Meshes[0].Parameters.Set(GameParameters.EnableColorEffect, true);

                var entity = new Entity();
                entity.Add(new ModelComponent(model));
                yield return entity;
            }
        }

        private void InitialAddPlanes(IEnumerable<TerrainPlane> planes)
        {
            lock (PlaneLock)
            {
                foreach (TerrainPlane plane in planes)
                {
                    TerrainTexture key = plane.Texture;
                    if (!PlaneMap.ContainsKey(key))
                        PlaneMap.Add(key, new HashSet<int>());
                    PlaneMap[key].Add(PlaneCount);

                    Planes.Add(PlaneCount, plane);
                    PlaneCount++;
                }

                int count;
                foreach (TerrainTexture key in PlaneMap.Keys)
                {
                    count = PlaneMap[key].Count;
                    count = count - count % PlaneStep;
                    count += PlaneStep;

                    EnsureCapacity(key, count);

                    int index = 0;
                    foreach (int planeKey in PlaneMap[key])
                    {
                        TerrainPlane plane = Planes[planeKey];
                        SetVertices(plane, index++);
                    }
                }
            }
        }

        private void CreateIndices(GraphicsDevice device, int amount)
        {
            var index = new int[] { 0, 1, 2, 2, 3, 0 };
            var indices = new int[amount * 6];
            for (int i = 0; i < amount; i++)
            {
                indices[i * 6 + 0] = i * 4 + index[0];
                indices[i * 6 + 1] = i * 4 + index[1];
                indices[i * 6 + 2] = i * 4 + index[2];
                indices[i * 6 + 3] = i * 4 + index[3];
                indices[i * 6 + 4] = i * 4 + index[4];
                indices[i * 6 + 5] = i * 4 + index[5];
            }

            Indices?.Dispose();
            Indices = Index.New(device, indices);
        }

        private unsafe void SetVertices(TerrainPlane plane, int offset)
        {
            TerrainTexture key = plane.Texture;
            Buffer vertices = VertexBuffers[key].Buffer;

            MappedResource map = Context.CommandList.MapSubresource(vertices, 0, MapMode.WriteNoOverwrite);
            var pointer = (TerrainVertex*)map.DataBox.DataPointer;
            pointer += offset * 4;

            // Texture UV coordinates
            Vector2
                textureTopLeft = new Vector2(0, 1),
                textureTopRight = new Vector2(1, 1),
                textureBtmLeft = new Vector2(0, 0),
                textureBtmRight = new Vector2(1, 0);

            // Size vector and corner positions
            Vector3
                corner1 = new Vector3(0, 1, 1),
                corner2 = new Vector3(0, 1, 0),
                corner3 = new Vector3(1, 1, 1),
                corner4 = new Vector3(1, 1, 0),
                corner5 = new Vector3(0, 0, 1),
                corner6 = new Vector3(0, 0, 0),
                corner7 = new Vector3(1, 0, 1),
                corner8 = new Vector3(1, 0, 0);

            Vector3 normal, cornerTopLeft, cornerBtmLeft, cornerTopRight, cornerBtmRight;

            switch (plane.Side)
            {
                case TerrainPlane.Sides.Top:
                    cornerTopLeft = plane.Position + corner1;
                    cornerBtmLeft = plane.Position + corner2;
                    cornerTopRight = plane.Position + corner3;
                    cornerBtmRight = plane.Position + corner4;
                    normal = Vector3.UnitY;

                    pointer[0] = new TerrainVertex(cornerTopRight, normal, key);
                    pointer[1] = new TerrainVertex(cornerTopLeft, normal, key);
                    pointer[2] = new TerrainVertex(cornerBtmLeft, normal, key);
                    pointer[3] = new TerrainVertex(cornerBtmRight, normal, key);
                    break;
                case TerrainPlane.Sides.Bottom:
                    cornerTopLeft = plane.Position + corner5;
                    cornerBtmLeft = plane.Position + corner6;
                    cornerTopRight = plane.Position + corner7;
                    cornerBtmRight = plane.Position + corner8;
                    normal = -Vector3.UnitY;

                    pointer[0] = new TerrainVertex(cornerBtmLeft, normal, key);
                    pointer[1] = new TerrainVertex(cornerTopLeft, normal, key);
                    pointer[2] = new TerrainVertex(cornerTopRight, normal, key);
                    pointer[3] = new TerrainVertex(cornerBtmRight, normal, key);
                    break;
                case TerrainPlane.Sides.Front:
                    cornerTopLeft = plane.Position + corner3;
                    cornerBtmLeft = plane.Position + corner7;
                    cornerTopRight = plane.Position + corner1;
                    cornerBtmRight = plane.Position + corner5;
                    normal = Vector3.UnitZ;

                    pointer[0] = new TerrainVertex(cornerTopRight, normal, key);
                    pointer[1] = new TerrainVertex(cornerTopLeft, normal, key);
                    pointer[2] = new TerrainVertex(cornerBtmLeft, normal, key);
                    pointer[3] = new TerrainVertex(cornerBtmRight, normal, key);
                    break;
                case TerrainPlane.Sides.Back:
                    cornerTopLeft = plane.Position + corner4;
                    cornerBtmLeft = plane.Position + corner8;
                    cornerTopRight = plane.Position + corner2;
                    cornerBtmRight = plane.Position + corner6;
                    normal = -Vector3.UnitZ;

                    pointer[0] = new TerrainVertex(cornerBtmLeft, normal, key);
                    pointer[1] = new TerrainVertex(cornerTopLeft, normal, key);
                    pointer[2] = new TerrainVertex(cornerTopRight, normal, key);
                    pointer[3] = new TerrainVertex(cornerBtmRight, normal, key);
                    break;
                case TerrainPlane.Sides.Left:
                    cornerTopLeft = plane.Position + corner1;
                    cornerBtmLeft = plane.Position + corner5;
                    cornerTopRight = plane.Position + corner2;
                    cornerBtmRight = plane.Position + corner6;
                    normal = Vector3.UnitX;

                    pointer[0] = new TerrainVertex(cornerTopRight, normal, key);
                    pointer[1] = new TerrainVertex(cornerTopLeft, normal, key);
                    pointer[2] = new TerrainVertex(cornerBtmLeft, normal, key);
                    pointer[3] = new TerrainVertex(cornerBtmRight, normal, key);
                    break;
                case TerrainPlane.Sides.Right:
                    cornerTopLeft = plane.Position + corner3;
                    cornerBtmLeft = plane.Position + corner7;
                    cornerTopRight = plane.Position + corner4;
                    cornerBtmRight = plane.Position + corner8;
                    normal = -Vector3.UnitX;

                    pointer[0] = new TerrainVertex(cornerBtmLeft, normal, key);
                    pointer[1] = new TerrainVertex(cornerTopLeft, normal, key);
                    pointer[2] = new TerrainVertex(cornerTopRight, normal, key);
                    pointer[3] = new TerrainVertex(cornerBtmRight, normal, key);
                    break;
            }

            Context.CommandList.UnmapSubresource(map);
        }

        private void EnsureCapacity(TerrainTexture key, int step = PlaneStep)
        {
            if (!VertexBuffers.ContainsKey(key))
            {
                int size = TerrainVertex.Layout.CalculateSize() * step * 4;
                var vertices = Vertex.New(Context.CommandList.GraphicsDevice, size, GraphicsResourceUsage.Dynamic);
                var binding = new VertexBufferBinding(vertices, TerrainVertex.Layout, vertices.ElementCount);
                VertexBuffers.Add(key, binding);
            }
            else
            {
                var binding = VertexBuffers[key];
                int planeCount = PlaneMap[key].Count;
                int verticesCount = binding.Buffer.SizeInBytes / VertexSize;
                int indicesCount = Indices.SizeInBytes / 2;

                if (planeCount * 4 >= verticesCount)
                {
                    int size = TerrainVertex.Layout.CalculateSize() * step * 4;
                    var vertices = Vertex.New(Context.CommandList.GraphicsDevice, binding.Buffer.SizeInBytes + size, GraphicsResourceUsage.Dynamic);
                    CopyBuffer(Context.CommandList, vertices, binding.Buffer, verticesCount);

                    binding = new VertexBufferBinding(vertices, TerrainVertex.Layout, vertices.ElementCount);
                    VertexBuffers[key] = binding;
                }

                if (planeCount * 6 >= indicesCount)
                    CreateIndices(Context.CommandList.GraphicsDevice, indicesCount + PlaneStep);
            }
        }

        private unsafe void CopyBuffer(CommandList commandList, Buffer newBuffer, Buffer oldBuffer, int count)
        {
            MappedResource oldMap = commandList.MapSubresource(oldBuffer, 0, MapMode.WriteNoOverwrite);
            MappedResource newMap = commandList.MapSubresource(newBuffer, 0, MapMode.WriteNoOverwrite);
            var oldPointer = (TerrainVertex*)oldMap.DataBox.DataPointer;
            var newPointer = (TerrainVertex*)newMap.DataBox.DataPointer;

            for (int i = 0; i < count; i++)
                newPointer[i] = oldPointer[i];

            commandList.UnmapSubresource(oldMap);
            commandList.UnmapSubresource(newMap);
            oldBuffer.Dispose();
        }
    }
}
