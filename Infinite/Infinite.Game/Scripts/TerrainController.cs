using Infinite.Mathematics;
using Infinite.Terrain;
using SiliconStudio.Core.Threading;
using SiliconStudio.Xenko.Engine;
using System.Collections.Generic;
using System.Linq;

namespace Infinite.Scripts
{
    public class TerrainController : SyncScript
    {
        public Entity Player { get; set; }
        //private bool ColliderShapesRendering = false;

        public override void Start()
        {
            const int width = 8;
            const int end = width / 2;
            const int start = -end;
            Dispatcher.For(start, end, (x) =>
            {
                for (int y = -6; y < 2; y++)
                    for (int z = start; z < end; z++)
                        World.Chunks.Add(new GenericVector3<int>(x, y, z), Chunk.Load(x, y, z));
            });

            IEnumerable<TerrainPlane> planes = World.Chunks.Values.SelectMany(x => x.GetPlanes());

            InfiniteGame.TerrainDrawer.Initialize(planes);
            Entity entity = InfiniteGame.TerrainDrawer.CreateEntity();
            Entity.AddChild(entity);

            var spawns = World.Chunks.Values.SelectMany(x => x.GetSpawns(Content));
            foreach (var spawn in spawns)
                Entity.AddChild(spawn);
        }


        public override void Update()
        {
            //Simulation similation = this.GetSimulation();
            //if (!ColliderShapesRendering)
            //{
            //    ColliderShapesRendering = true;
            //    similation.ColliderShapesRendering = ColliderShapesRendering;
            //}

            //if (TerrainCollider.Update(Entity, Player))
            //{
            //    ColliderShapesRendering = false;
            //    similation.ColliderShapesRendering = ColliderShapesRendering;
            //}

            TerrainCollider.Update(Entity, Player);
        }
    }
}
