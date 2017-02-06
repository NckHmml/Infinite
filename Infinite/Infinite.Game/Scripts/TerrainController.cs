using Infinite;
using Infinite.Terrain;
using SiliconStudio.Core.Threading;
using SiliconStudio.Xenko.Engine;
using System.Collections.Generic;
using System.Linq;

namespace Scripts
{
    public class TerrainController : SyncScript
    {
        public override void Start()
        {
            var chunks = new List<Chunk>();

            const int width = 8;
            const int end = width / 2;
            const int start = -end;
            Dispatcher.For(start, end, (x) =>
            {
                for (int y = -6; y < 2; y++)
                    for (int z = start; z < end; z++)
                        chunks.Add(Chunk.Load(x, y, z));
            });

            IEnumerable<TerrainPlane> planes = chunks.SelectMany(x => x.GetPlanes());

            InfiniteGame.TerrainDrawer.Initialize(planes);
            Entity entity = InfiniteGame.TerrainDrawer.CreateEntity();
            Entity.AddChild(entity);
        }


        public override void Update()
        {
            
        }
    }
}
