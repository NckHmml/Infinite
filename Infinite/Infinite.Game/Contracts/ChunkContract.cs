using Infinite.Mathematics;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinite.Contracts
{
    [ProtoContract]
    public class ChunkContract
    {
        [ProtoMember(1)]
        public Dictionary<GenericVector3Contract<byte>, BlockContract> Blocks { get; set; }
    }
}
