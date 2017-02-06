using Infinite.Mathematics;
using SiliconStudio.Core.Mathematics;
using System;
using System.Collections.Generic;

namespace Infinite.Terrain
{
    public class Chunk
    {
        public const byte Size = 32;
        public GenericVector3<int> Position { get; private set; }

        public Dictionary<GenericVector3<byte>, Block> Blocks { get; } = new Dictionary<GenericVector3<byte>, Block>();

        public Chunk(GenericVector3<int> position, Dictionary<GenericVector3<byte>, Block> blocks)
        {
            Position = position;
            if (blocks != null)
            {
                foreach (var pair in blocks)
                    Blocks.Add(pair.Key, pair.Value);
            }
        }

        public void Save()
        {
            InfiniteGame.WriteChunk(this);
        }

        public IEnumerable<TerrainPlane> GetPlanes()
        {
            foreach (var pair in Blocks)
                foreach (TerrainPlane plane in CheckSides(pair))
                    yield return plane;
        }

        private IEnumerable<TerrainPlane> CheckSides(KeyValuePair<GenericVector3<byte>, Block> pair)
        {
            var block = pair.Value;
            var position = new Vector3()
            {
                X = Position.X * Size + pair.Key.X,
                Y = Position.Y * Size + pair.Key.Y,
                Z = Position.Z * Size + pair.Key.Z,
            };

            if ((block.Sides & Block.Adjecent.Top) == Block.Adjecent.Top)
                yield return new TerrainPlane(position, TerrainPlane.Sides.Top, TextureForMaterial(block.Material));
            if ((block.Sides & Block.Adjecent.Bottom) == Block.Adjecent.Bottom)
                yield return new TerrainPlane(position, TerrainPlane.Sides.Bottom, TextureForMaterial(block.Material));
            if ((block.Sides & Block.Adjecent.Left) == Block.Adjecent.Left)
                yield return new TerrainPlane(position, TerrainPlane.Sides.Left, TextureForMaterial(block.Material));
            if ((block.Sides & Block.Adjecent.Right) == Block.Adjecent.Right)
                yield return new TerrainPlane(position, TerrainPlane.Sides.Right, TextureForMaterial(block.Material));
            if ((block.Sides & Block.Adjecent.Front) == Block.Adjecent.Front)
                yield return new TerrainPlane(position, TerrainPlane.Sides.Front, TextureForMaterial(block.Material));
            if ((block.Sides & Block.Adjecent.Back) == Block.Adjecent.Back)
                yield return new TerrainPlane(position, TerrainPlane.Sides.Back, TextureForMaterial(block.Material));
        }

        private TerrainTexture TextureForMaterial(Block.MaterialType material)
        {
            switch (material)
            {
                case Block.MaterialType.Soil:
                    return TerrainTexture.Soil;
                case Block.MaterialType.Grass:
                    return TerrainTexture.Grass;
                case Block.MaterialType.Stone:
                    return TerrainTexture.Stone;
            }
            return TerrainTexture.Soil;
        }

        public static Chunk Load(GenericVector3<int> position)
        {
            Chunk chunk = InfiniteGame.ReadChunk(position);
            if (chunk == null)
                chunk = New(position);
            return chunk;
        }

        public static Chunk Load(int x, int y, int z)
        {
            return Load(new GenericVector3<int>(x, y, z));
        }

        public static Chunk New(GenericVector3<int> position)
        {
            var chunk = TerrainGenerator.GenerateChunk(position);
            //chunk.Save();
            return chunk;
        }

        public static Chunk New(int x, int y, int z)
        {
            return New(new GenericVector3<int>(x, y, z));
        }
    }
}
