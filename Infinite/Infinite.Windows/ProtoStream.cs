using Infinite.Contracts;
using Infinite.Mathematics;
using Infinite.Terrain;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinite
{
    public static class ProtoStream
    {
        public static object Lock = new object();

        public static void WriteChunk(Chunk chunk)
        {
            string path = $"chunks/{chunk.Position.X:X8}{chunk.Position.Y:X8}{chunk.Position.Z:X8}.dat";
            var contract = new ChunkContract();
            contract.Blocks = new Dictionary<GenericVector3Contract<byte>, BlockContract>();

            BlockContract block;
            Block temp;
            GenericVector3Contract<byte> position;
            foreach (var pair in chunk.Blocks)
            {
                temp = pair.Value;
                position = new GenericVector3Contract<byte>
                {
                    X = pair.Key.X,
                    Y = pair.Key.Y,
                    Z = pair.Key.Z
                };
                block = new BlockContract();
                block.Material = pair.Value.Material;
                block.Sides = pair.Value.Sides;
                contract.Blocks.Add(position, block);
            }

            lock (Lock)
                using (var file = new FileStream(path, FileMode.OpenOrCreate))
                    Serializer.Serialize(file, contract);
        }

        public static Chunk ReadChunk(GenericVector3<int> position)
        {
            string path = $"chunks/{position.X:X8}{position.Y:X8}{position.Z:X8}.dat";
            if (!File.Exists(path))
                return null;

            ChunkContract contract;
            using (var file = new FileStream(path, FileMode.Open))
                contract = Serializer.Deserialize<ChunkContract>(file);

            var blocks = new Dictionary<GenericVector3<byte>, Block>();
            if (contract.Blocks != null)
                foreach (var pair in contract.Blocks)
                    blocks.Add(new GenericVector3<byte>(pair.Key.X, pair.Key.Y, pair.Key.Z), new Block(pair.Value.Material, pair.Value.Sides));

            return new Chunk(position, blocks, null);
        }
    }
}
