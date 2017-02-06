using Infinite.Mathematics;
using Infinite.Terrain;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using System;
using System.Threading.Tasks;

namespace Infinite
{
    public class InfiniteGame : Game
    {
        public static TerrainDrawer TerrainDrawer { get; private set; }
        public static Action<Chunk> WriteChunk;
        public static Func<GenericVector3<int>, Chunk> ReadChunk;

        protected override async Task LoadContent()
        {
            await base.LoadContent();
            Window.AllowUserResizing = true;
            ProfilerSystem.EnableProfiling(false, GameProfilingKeys.GameDrawFPS);

            TerrainDrawer = new TerrainDrawer(GraphicsContext);
        }
    }
}
