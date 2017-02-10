using SiliconStudio.Core.Mathematics;

namespace Infinite.Terrain
{
    public class TerrainPlane
    {
        public Vector3 Position { get; private set; }
        public Sides Side { get; private set; }
        public Block.MaterialType Material { get; private set; }

        public TerrainPlane(Vector3 position, Sides side, Block.MaterialType material)
        {
            Position = position;
            Side = side;
            Material = material;
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
