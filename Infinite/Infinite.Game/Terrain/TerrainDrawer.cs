using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;
using System;
using System.Collections.Generic;
using System.Linq;
using static SiliconStudio.Xenko.Graphics.Buffer;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace Infinite.Terrain
{
    public class TerrainDrawer
    {
        private HashSet<int> PlaneMap { get; } = new HashSet<int>();
        private Dictionary<int, TerrainPlane> Planes { get; } = new Dictionary<int, TerrainPlane>();
        private VertexBufferBinding? VertexBuffer { get; set; }

        private const int PlaneStep = 1000;
        private int PlaneCount = 0;
        private int VertexSize = TerrainVertex.Layout.CalculateSize();
        private Object PlaneLock = new Object();

        private Buffer Indices;
        private GraphicsContext Context;

        public TerrainDrawer(GraphicsContext context)
        {
            Context = context;
        }

        public void Initialize(IEnumerable<TerrainPlane> planes = null)
        {
            GraphicsDevice device = Context.CommandList.GraphicsDevice;

            int count = planes?.Count() ?? 0;
            count = count - count % PlaneStep;
            count += PlaneStep;
            CreateIndices(device, count);
            InitialAddPlanes(planes);
        }

        public void AddPlane(TerrainPlane plane)
        {
            lock (PlaneLock)
            {
                EnsureCapacity();
                SetVertices(plane, PlaneMap.Count);

                PlaneMap.Add(PlaneCount);
                Planes.Add(PlaneCount, plane);

                PlaneCount++;
            }
        }

        public Entity CreateEntity()
        {
            var model = new Model();
            var material = Material.New(Context.CommandList.GraphicsDevice, new MaterialDescriptor
            {
                Attributes =
                {
                    DiffuseModel = new MaterialDiffuseLambertModelFeature(),
                    Diffuse = new MaterialDiffuseMapFeature(new ComputeShaderClassColor
                        {
                            MixinReference = "ComputeColorStream"
                        })
                },
            });
            model.Materials.Add(material);

            model.Add(new Mesh
            {
                Draw = new MeshDraw
                {
                    PrimitiveType = PrimitiveType.TriangleList,
                    VertexBuffers = new[] { VertexBuffer.Value },
                    IndexBuffer = new IndexBufferBinding(Indices, true, Indices.ElementCount),
                    DrawCount = PlaneMap.Count * 6,
                },
            });

            var entity = new Entity();
            entity.Add(new ModelComponent(model));
            return entity;
        }

        private void InitialAddPlanes(IEnumerable<TerrainPlane> planes)
        {
            lock (PlaneLock)
            {
                foreach (TerrainPlane plane in planes)
                {
                    PlaneMap.Add(PlaneCount);
                    Planes.Add(PlaneCount, plane);
                    PlaneCount++;
                }

                int index = 0;
                int count = PlaneMap.Count;
                count = count - count % PlaneStep;
                count += PlaneStep;

                EnsureCapacity(count);

                foreach (int planeKey in PlaneMap)
                {
                    TerrainPlane plane = Planes[planeKey];
                    SetVertices(plane, index++);
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
            Block.MaterialType material = plane.Material;
            Buffer vertices = VertexBuffer?.Buffer;

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

                    pointer[0] = new TerrainVertex(cornerTopRight, normal, material);
                    pointer[1] = new TerrainVertex(cornerTopLeft, normal, material);
                    pointer[2] = new TerrainVertex(cornerBtmLeft, normal, material);
                    pointer[3] = new TerrainVertex(cornerBtmRight, normal, material);
                    break;
                case TerrainPlane.Sides.Bottom:
                    cornerTopLeft = plane.Position + corner5;
                    cornerBtmLeft = plane.Position + corner6;
                    cornerTopRight = plane.Position + corner7;
                    cornerBtmRight = plane.Position + corner8;
                    normal = -Vector3.UnitY;

                    pointer[0] = new TerrainVertex(cornerBtmLeft, normal, material);
                    pointer[1] = new TerrainVertex(cornerTopLeft, normal, material);
                    pointer[2] = new TerrainVertex(cornerTopRight, normal, material);
                    pointer[3] = new TerrainVertex(cornerBtmRight, normal, material);
                    break;
                case TerrainPlane.Sides.Front:
                    cornerTopLeft = plane.Position + corner3;
                    cornerBtmLeft = plane.Position + corner7;
                    cornerTopRight = plane.Position + corner1;
                    cornerBtmRight = plane.Position + corner5;
                    normal = Vector3.UnitZ;

                    pointer[0] = new TerrainVertex(cornerTopRight, normal, material);
                    pointer[1] = new TerrainVertex(cornerTopLeft, normal, material);
                    pointer[2] = new TerrainVertex(cornerBtmLeft, normal, material);
                    pointer[3] = new TerrainVertex(cornerBtmRight, normal, material);
                    break;
                case TerrainPlane.Sides.Back:
                    cornerTopLeft = plane.Position + corner4;
                    cornerBtmLeft = plane.Position + corner8;
                    cornerTopRight = plane.Position + corner2;
                    cornerBtmRight = plane.Position + corner6;
                    normal = -Vector3.UnitZ;

                    pointer[0] = new TerrainVertex(cornerBtmLeft, normal, material);
                    pointer[1] = new TerrainVertex(cornerTopLeft, normal, material);
                    pointer[2] = new TerrainVertex(cornerTopRight, normal, material);
                    pointer[3] = new TerrainVertex(cornerBtmRight, normal, material);
                    break;
                case TerrainPlane.Sides.Left:
                    cornerTopLeft = plane.Position + corner1;
                    cornerBtmLeft = plane.Position + corner5;
                    cornerTopRight = plane.Position + corner2;
                    cornerBtmRight = plane.Position + corner6;
                    normal = Vector3.UnitX;

                    pointer[0] = new TerrainVertex(cornerTopRight, normal, material);
                    pointer[1] = new TerrainVertex(cornerTopLeft, normal, material);
                    pointer[2] = new TerrainVertex(cornerBtmLeft, normal, material);
                    pointer[3] = new TerrainVertex(cornerBtmRight, normal, material);
                    break;
                case TerrainPlane.Sides.Right:
                    cornerTopLeft = plane.Position + corner3;
                    cornerBtmLeft = plane.Position + corner7;
                    cornerTopRight = plane.Position + corner4;
                    cornerBtmRight = plane.Position + corner8;
                    normal = -Vector3.UnitX;

                    pointer[0] = new TerrainVertex(cornerBtmLeft, normal, material);
                    pointer[1] = new TerrainVertex(cornerTopLeft, normal, material);
                    pointer[2] = new TerrainVertex(cornerTopRight, normal, material);
                    pointer[3] = new TerrainVertex(cornerBtmRight, normal, material);
                    break;
            }

            Context.CommandList.UnmapSubresource(map);
        }

        private void EnsureCapacity(int step = PlaneStep)
        {
            if (!VertexBuffer.HasValue)
            {
                int size = TerrainVertex.Layout.CalculateSize() * step * 4;
                var vertices = Vertex.New(Context.CommandList.GraphicsDevice, size, GraphicsResourceUsage.Dynamic);
                VertexBuffer = new VertexBufferBinding(vertices, TerrainVertex.Layout, vertices.ElementCount);
            }
            else
            {
                var buffer = VertexBuffer?.Buffer;
                int planeCount = PlaneMap.Count;
                int verticesCount = buffer.SizeInBytes / VertexSize;
                int indicesCount = Indices.SizeInBytes / 2;

                if (planeCount * 4 >= verticesCount)
                {
                    int size = TerrainVertex.Layout.CalculateSize() * step * 4;
                    var vertices = Vertex.New(Context.CommandList.GraphicsDevice, buffer.SizeInBytes + size, GraphicsResourceUsage.Dynamic);
                    CopyBuffer(Context.CommandList, vertices, buffer, verticesCount);

                    VertexBuffer = new VertexBufferBinding(vertices, TerrainVertex.Layout, vertices.ElementCount);
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
