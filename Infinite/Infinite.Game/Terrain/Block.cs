using System;

namespace Infinite.Terrain
{
    public struct Block
    {
        public MaterialType Material;
        public Adjecent Sides;

        public Block(MaterialType material, Adjecent sides)
        {
            Material = material;
            Sides = sides;
        }

        public enum MaterialType : byte
        {
            Soil,
            Grass,
            Stone,
            None = Byte.MaxValue
        }

        [Flags]
        public enum Adjecent : byte
        {
            None = 0,
            Top = 1 << 0,
            Bottom = 1 << 1,
            Left = 1 << 2,
            Right = 1 << 3,
            Front = 1 << 4,
            Back = 1 << 5,
            All = 0xFF
        }
    }
}
