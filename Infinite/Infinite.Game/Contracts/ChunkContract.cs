using ProtoBuf;
using System.Collections.Generic;

namespace Infinite.Contracts
{
    [ProtoContract]
    public class ChunkContract
    {
        [ProtoMember(1)]
        public Dictionary<GenericVector3Contract<byte>, BlockContract> Blocks { get; set; }
    }
}
