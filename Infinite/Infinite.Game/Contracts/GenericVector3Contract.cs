using ProtoBuf;

namespace Infinite.Contracts
{
    [ProtoContract]
    public class GenericVector3Contract<T>
    {
        [ProtoMember(1)]
        public T X { get; set; }

        [ProtoMember(2)]
        public T Y { get; set; }

        [ProtoMember(3)]
        public T Z { get; set; }
    }
}
