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

            const int width = 10;
            for (int y = -1; y < 2; y++)
            {
                for (int i = 0; i < width * width; i++)
                {
                    var x = i % width;
                    var z = (i - x) / width;
                    x -= width / 2;
                    z -= width / 2;

                    chunks.Add(Chunk.Load(x, y, z));
                }
            }

            IEnumerable<TerrainPlane> planes = chunks.SelectMany(x => x.GetPlanes());

            InfiniteGame.TerrainDrawer.Initialize(Content, planes);
        }


        public override void Update()
        {
            
        }
    }
}
