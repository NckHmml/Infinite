﻿using Infinite.Mathematics;
using Infinite.Terrain;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Threading;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Physics;
using SiliconStudio.Xenko.Rendering;
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

            var tree = new Entity(new Vector3(20.5f, 59f, 20.5f));
            var model = Content.Load<Model>("Models/Tree");
            var modelComponent = new ModelComponent(model);
            tree.Add(modelComponent);
            tree.Transform.Scale = new Vector3(0.6f);

            Entity.AddChild(tree);
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
