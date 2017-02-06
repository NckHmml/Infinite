using Infinite;
using Infinite.Terrain;
using SiliconStudio.Core.Threading;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Scripts
{
    public class TerrainController : SyncScript
    {
        public override void Start()
        {
            var chunks = new List<Chunk>();

            const int width = 10;
            Dispatcher.For(-2, 3, (y) =>
            {
                for (int i = 0; i < width * width; i++)
                {
                    var x = i % width;
                    var z = (i - x) / width;
                    x -= width / 2;
                    z -= width / 2;

                    chunks.Add(Chunk.Load(x, y, z));
                }
            });

            IEnumerable<TerrainPlane> planes = chunks.SelectMany(x => x.GetPlanes());

            InfiniteGame.TerrainDrawer.Initialize(planes);
            foreach (var entity in InfiniteGame.TerrainDrawer.CreateEntities())
                Entity.AddChild(entity);
        }


        public override void Update()
        {
            
        }
    }
}
