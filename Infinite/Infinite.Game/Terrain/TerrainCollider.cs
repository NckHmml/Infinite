using Infinite.Mathematics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Physics;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Infinite.Terrain
{
    public static class TerrainCollider
    {
        private static readonly uint[] IndexOrder = new uint[] { 0, 1, 2, 2, 3, 0 };
        private static GenericVector3<int> LastPosition;

        public static bool Update(Entity terrain, Entity player)
        {
            Vector3 playerPosition = player.Transform.Position;
            var position = new GenericVector3<int>((int)playerPosition.X, (int)playerPosition.Y, (int)playerPosition.Z);

            if (LastPosition == position)
                return false;
            LastPosition = position;

            var compound = new CompoundColliderShape();
            foreach (var shape in GetColliderBoxes(playerPosition))
                compound.AddChildShape(shape);
            var collider = terrain.Get<StaticColliderComponent>();
            collider.ColliderShape = compound;

            return true;
        }

        private static IEnumerable<BoxColliderShape> GetColliderBoxes(Vector3 position)
        {
            const int max = 3;
            const int min = -3;
            long cX, cY, cZ;
            for (int x = min; x < max; x++)
            {
                for (int y = min; y < max; y++)
                {
                    for (int z = min; z < max; z++)
                    {
                        cX = (long)position.X + x;
                        cY = (long)position.Y + y;
                        cZ = (long)position.Z + z;
                        if (World.HasBlock(cX, cY, cZ))
                        {
                            yield return new BoxColliderShape(false, Vector3.One)
                            {
                                LocalOffset = new Vector3(cX + .5f, cY + .5f, cZ + .5f)
                            };
                        }
                    }
                }
            }
        }
    }
}
