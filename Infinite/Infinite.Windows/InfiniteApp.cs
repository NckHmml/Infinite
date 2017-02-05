namespace Infinite
{
    class InfiniteApp
    {
        static void Main(string[] args)
        {
            InfiniteGame.WriteChunk = ProtoStream.WriteChunk;
            InfiniteGame.ReadChunk = ProtoStream.ReadChunk;
            using (var game = new InfiniteGame())
            {
                game.Run();
            }
        }
    }
}
