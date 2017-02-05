using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SiliconStudio.Xenko.Graphics.Buffer;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace Infinite.Terrain
{
    public class TerrainDrawer
    {
        private Dictionary<TerrainTexture, HashSet<int>> PlaneMap { get; } = new Dictionary<TerrainTexture, HashSet<int>>();
        private Dictionary<int, TerrainPlane> Planes { get; } = new Dictionary<int, TerrainPlane>();
        private Dictionary<TerrainTexture, VertexBufferBinding> VertexBuffers { get; } = new Dictionary<TerrainTexture, VertexBufferBinding>();
        private Dictionary<TerrainTexture, EffectInstance> Effects { get; } = new Dictionary<TerrainTexture, EffectInstance>();

        private const int PlaneStep = 1000;
        private int PlaneCount = 0;
        private int VertexSize = VertexPositionNormalTexture.Layout.CalculateSize();
        private Object PlaneLock = new Object();

        private Buffer Indices;
        private MutablePipelineState PipelineState;
        private GraphicsContext Context;

        private bool Initialized = false;

        public TerrainDrawer(GraphicsContext context)
        {
            Context = context;
        }

        public void Initialize(ContentManager content, IEnumerable<TerrainPlane> planes = null)
        {
            GraphicsDevice device = Context.CommandList.GraphicsDevice;
            LoadTextures(device, content);

            PipelineState = new MutablePipelineState(device);
            PipelineState.State.InputElements = VertexPositionNormalTexture.Layout.CreateInputElements();
            PipelineState.State.SetDefaults();
            PipelineState.State.PrimitiveType = PrimitiveType.TriangleList;

            int count = planes?.Count() ?? 0;
            count = count - count % PlaneStep;
            count += PlaneStep;
            InitialAddPlanes(planes);
            CreateIndices(device, count);

            Initialized = true;
        }

        public void AddPlane(TerrainPlane plane)
        {
            lock (PlaneLock)
            {
                TerrainTexture key = plane.Texture;
                EnsureCapacity(key);
                SetVertices(key, plane, PlaneMap[key].Count);

                if (!PlaneMap.ContainsKey(key))
                    PlaneMap.Add(key, new HashSet<int>());
                PlaneMap[key].Add(PlaneCount);

                Planes.Add(PlaneCount, plane);
                PlaneCount++;
            }
        }

        public void Draw()
        {
            // We cannot draw without the ViewProjection
            if (BasicCameraController.ViewProjection == null || !Initialized)
                return;

            CommandList commandList = Context.CommandList;

            // Update pipeline state
            PipelineState.State.RootSignature = Effects.FirstOrDefault().Value.RootSignature;
            PipelineState.State.EffectBytecode = Effects.FirstOrDefault().Value.Effect.Bytecode;
            PipelineState.State.RasterizerState = RasterizerStates.Wireframe;
            PipelineState.State.Output.CaptureState(commandList);
            PipelineState.Update();
            commandList.SetPipelineState(PipelineState.CurrentState);

            // Set the index buffer
            commandList.SetIndexBuffer(Indices, 0, true);

            foreach (TerrainTexture textureKey in PlaneMap.Keys)
            {
                // Get/Set vertex buffer
                VertexBufferBinding binding = VertexBuffers[textureKey];
                commandList.SetVertexBuffer(0, binding.Buffer, 0, binding.Stride);

                // Transform and apply effect
                Effects[textureKey].Parameters.Set(SpriteBaseKeys.MatrixTransform, BasicCameraController.ViewProjection);
                Effects[textureKey].Apply(Context);

                // Draw
                commandList.DrawIndexed(PlaneMap[textureKey].Count * 6);
            }
        }

        private void LoadTextures(GraphicsDevice device, ContentManager content)
        {
            foreach(TerrainTexture key in Enum.GetValues(typeof(TerrainTexture)))
            {
                string name = Enum.GetName(typeof(TerrainTexture), key);
                var texture = content.Load<Texture>($"Terrain/{name}");
                var effect = new EffectInstance(new Effect(device, SpriteEffect.Bytecode));
                effect.Parameters.Set(TexturingKeys.Texture0, texture);
                effect.UpdateEffect(device);
                Effects.Add(key, effect);
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
                        SetVertices(key, plane, index++);
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

        private unsafe void SetVertices(TerrainTexture key, TerrainPlane plane, int offset)
        {
            Buffer vertices = VertexBuffers[key].Buffer;

            MappedResource map = Context.CommandList.MapSubresource(vertices, 0, MapMode.WriteNoOverwrite);
            var pointer = (VertexPositionNormalTexture*)map.DataBox.DataPointer;
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

                    pointer[0] = new VertexPositionNormalTexture(cornerTopRight, normal, textureBtmLeft);
                    pointer[1] = new VertexPositionNormalTexture(cornerTopLeft, normal, textureBtmRight);
                    pointer[2] = new VertexPositionNormalTexture(cornerBtmLeft, normal, textureTopRight);
                    pointer[3] = new VertexPositionNormalTexture(cornerBtmRight, normal, textureTopLeft);
                    break;
                case TerrainPlane.Sides.Bottom:
                    cornerTopLeft = plane.Position + corner5;
                    cornerBtmLeft = plane.Position + corner6;
                    cornerTopRight = plane.Position + corner7;
                    cornerBtmRight = plane.Position + corner8;
                    normal = -Vector3.UnitY;

                    pointer[0] = new VertexPositionNormalTexture(cornerBtmLeft, normal, textureTopRight);
                    pointer[1] = new VertexPositionNormalTexture(cornerTopLeft, normal, textureBtmRight);
                    pointer[2] = new VertexPositionNormalTexture(cornerTopRight, normal, textureBtmLeft);
                    pointer[3] = new VertexPositionNormalTexture(cornerBtmRight, normal, textureTopLeft);
                    break;
                case TerrainPlane.Sides.Front:
                    cornerTopLeft = plane.Position + corner3;
                    cornerBtmLeft = plane.Position + corner7;
                    cornerTopRight = plane.Position + corner1;
                    cornerBtmRight = plane.Position + corner5;
                    normal = Vector3.UnitZ;

                    pointer[0] = new VertexPositionNormalTexture(cornerTopRight, normal, textureBtmLeft);
                    pointer[1] = new VertexPositionNormalTexture(cornerTopLeft, normal, textureBtmRight);
                    pointer[2] = new VertexPositionNormalTexture(cornerBtmLeft, normal, textureTopRight);
                    pointer[3] = new VertexPositionNormalTexture(cornerBtmRight, normal, textureTopLeft);
                    break;
                case TerrainPlane.Sides.Back:
                    cornerTopLeft = plane.Position + corner4;
                    cornerBtmLeft = plane.Position + corner8;
                    cornerTopRight = plane.Position + corner2;
                    cornerBtmRight = plane.Position + corner6;
                    normal = -Vector3.UnitZ;

                    pointer[0] = new VertexPositionNormalTexture(cornerBtmLeft, normal, textureTopRight);
                    pointer[1] = new VertexPositionNormalTexture(cornerTopLeft, normal, textureBtmRight);
                    pointer[2] = new VertexPositionNormalTexture(cornerTopRight, normal, textureBtmLeft);
                    pointer[3] = new VertexPositionNormalTexture(cornerBtmRight, normal, textureTopLeft);
                    break;
                case TerrainPlane.Sides.Left:
                    cornerTopLeft = plane.Position + corner1;
                    cornerBtmLeft = plane.Position + corner5;
                    cornerTopRight = plane.Position + corner2;
                    cornerBtmRight = plane.Position + corner6;
                    normal = Vector3.UnitX;

                    pointer[0] = new VertexPositionNormalTexture(cornerTopRight, normal, textureBtmLeft);
                    pointer[1] = new VertexPositionNormalTexture(cornerTopLeft, normal, textureBtmRight);
                    pointer[2] = new VertexPositionNormalTexture(cornerBtmLeft, normal, textureTopRight);
                    pointer[3] = new VertexPositionNormalTexture(cornerBtmRight, normal, textureTopLeft);
                    break;
                case TerrainPlane.Sides.Right:
                    cornerTopLeft = plane.Position + corner3;
                    cornerBtmLeft = plane.Position + corner7;
                    cornerTopRight = plane.Position + corner4;
                    cornerBtmRight = plane.Position + corner8;
                    normal = -Vector3.UnitX;

                    pointer[0] = new VertexPositionNormalTexture(cornerBtmLeft, normal, textureTopRight);
                    pointer[1] = new VertexPositionNormalTexture(cornerTopLeft, normal, textureBtmRight);
                    pointer[2] = new VertexPositionNormalTexture(cornerTopRight, normal, textureBtmLeft);
                    pointer[3] = new VertexPositionNormalTexture(cornerBtmRight, normal, textureTopLeft);
                    break;
            }

            Context.CommandList.UnmapSubresource(map);
        }

        private void EnsureCapacity(TerrainTexture key, int step = PlaneStep)
        {
            if (!VertexBuffers.ContainsKey(key))
            {
                int size = VertexPositionNormalTexture.Layout.CalculateSize() * step * 4;
                var vertices = Vertex.New(Context.CommandList.GraphicsDevice, size, GraphicsResourceUsage.Dynamic);
                var binding = new VertexBufferBinding(vertices, VertexPositionNormalTexture.Layout, vertices.ElementCount);
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
                    int size = VertexPositionNormalTexture.Layout.CalculateSize() * step * 4;
                    var vertices =  Vertex.New(Context.CommandList.GraphicsDevice, binding.Buffer.SizeInBytes + size, GraphicsResourceUsage.Dynamic);
                    CopyBuffer(Context.CommandList, vertices, binding.Buffer, verticesCount);

                    binding = new VertexBufferBinding(vertices, VertexPositionNormalTexture.Layout, vertices.ElementCount);
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
            var oldPointer = (VertexPositionNormalTexture*)oldMap.DataBox.DataPointer;
            var newPointer = (VertexPositionNormalTexture*)newMap.DataBox.DataPointer;

            for (int i = 0; i < count; i++)
                newPointer[i] = oldPointer[i];

            commandList.UnmapSubresource(oldMap);
            commandList.UnmapSubresource(newMap);
            oldBuffer.Dispose();
        }
    }
}
