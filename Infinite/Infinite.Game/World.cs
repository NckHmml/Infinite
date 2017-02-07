using Infinite.Mathematics;
using Infinite.Terrain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinite
{
    public static class World
    {
        public static Dictionary<GenericVector3<int>, Chunk> Chunks { get; } = new Dictionary<GenericVector3<int>, Chunk>();

        public static Block? GetBlock(long x, long y, long z)
        {
            GenericVector3<int> position = ToChunkRelative(x, y, z);
            if (!Chunks.ContainsKey(position))
                return null;
            Chunk chunk = Chunks[position];

            x -= position.X * Chunk.Size;
            y -= position.Y * Chunk.Size;
            z -= position.Z * Chunk.Size;

            var blockPosition = new GenericVector3<byte>((byte)x, (byte)y, (byte)z);
            if (!chunk.Blocks.ContainsKey(blockPosition))
                return null;
            return chunk.Blocks[blockPosition];
        }

        public static bool HasBlock(long x, long y, long z)
        {
            GenericVector3<int> position = ToChunkRelative(x, y, z);
            if (!Chunks.ContainsKey(position))
                return false;
            Chunk chunk = Chunks[position];

            x -= position.X * Chunk.Size;
            y -= position.Y * Chunk.Size;
            z -= position.Z * Chunk.Size;

            var blockPosition = new GenericVector3<byte>((byte)x, (byte)y, (byte)z);
            if (!chunk.Blocks.ContainsKey(blockPosition))
                return false;
            return true;
        }

        private static GenericVector3<int> ToChunkRelative(long x, long y, long z)
        {
            int cX, cY, cZ;
            if (x >= 0)
                cX = (int)(x / Chunk.Size);
            else
                cX = (int)((x + 1) / Chunk.Size) - 1;
            if (y >= 0)
                cY = (int)(y / Chunk.Size);
            else
                cY = (int)((y + 1) / Chunk.Size) - 1;
            if (z >= 0)
                cZ = (int)(z / Chunk.Size);
            else
                cZ = (int)((z + 1) / Chunk.Size) - 1;
            return new GenericVector3<int>(cX, cY, cZ);
        }
    }
}
