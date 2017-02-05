using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
