using SiliconStudio.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinite.Terrain
{
    public class EntitySpawn
    {
        public EntityType Type { get; set; }
        public float Rotation { get; set; }

        public enum EntityType : byte
        {
            Tree
        }
    }
}
