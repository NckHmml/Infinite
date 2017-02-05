using SiliconStudio.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinite.Terrain
{
    public class TerrainPlane
    {
        public Vector3 Position { get; private set; }
        public Sides Side { get; private set; }
        public TerrainTexture Texture { get; private set; }

        public TerrainPlane(Vector3 position, Sides side, TerrainTexture texture)
        {
            Position = position;
            Side = side;
            Texture = texture;
        }

        public enum Sides
        {
            Top,
            Bottom,
            Front,
            Back,
            Left,
            Right
        }
    }
}
