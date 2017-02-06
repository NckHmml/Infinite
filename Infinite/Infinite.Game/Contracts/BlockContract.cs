using ProtoBuf;
using static Infinite.Terrain.Block;

namespace Infinite.Contracts
{
    [ProtoContract]
    public class BlockContract
    {
        [ProtoMember(1)]
        public MaterialType Material { get; set; }
        [ProtoMember(2)]
        public Adjecent Sides { get; set; }
    }
}
