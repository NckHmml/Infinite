using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using System;
using System.Runtime.InteropServices;
using Colors = SiliconStudio.Core.Mathematics.Color;

namespace Infinite.Terrain
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct TerrainVertex : IEquatable<TerrainVertex>, IVertex
    {
        public TerrainVertex(Vector3 position, Vector3 normal, Block.MaterialType texture) : this()
        {
            Position = position;
            Normal = normal;
            Color = GetColorFor(texture, position);
        }

        /// <summary>
        /// XYZ position.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The vertex normal.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// The color.
        /// </summary>
        public Vector4 Color;

        /// <summary>
        /// Defines structure byte size.
        /// </summary>
        public static readonly int Size = 28;

        /// <summary>
        /// The vertex layout of this structure.
        /// </summary>
        public static readonly VertexDeclaration Layout = new VertexDeclaration(
            VertexElement.Position<Vector3>(),
            VertexElement.Normal<Vector3>(),
            VertexElement.Color<Vector4>()
        );

        private Vector4 GetColorFor(Block.MaterialType material, Vector3 position)
        {
            float blend;
            switch (material)
            {
                case Block.MaterialType.Grass:
                    blend = Math.Min(1f, Math.Max(0.5f, position.Y / 50f) - 0.5f);
                    return Vector4.Lerp(Colors.Green.ToVector4(), Vector4.One, blend);
                case Block.MaterialType.Soil:
                    return Colors.SandyBrown.ToVector4();
                case Block.MaterialType.Water:
                    return new Vector4(0, 0, 1f, .5f);
                case Block.MaterialType.Stone:
                default:
                    blend = -128 - position.Y;
                    blend = 1f - Math.Min(1f, Math.Max(0f, blend / -128f));
                    blend /= 30f;
                    return Vector4.Lerp(new Vector4(0.04f, 0.04f, 0.04f, 1f), Colors.Red.ToVector4(), blend);
            }
        }

        public bool Equals(TerrainVertex other)
        {
            return Position.Equals(other.Position) && Normal.Equals(other.Normal) && Color.Equals(other.Color);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TerrainVertex && Equals((TerrainVertex)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Normal.GetHashCode();
                hashCode = (hashCode * 397) ^ Color.GetHashCode();
                return hashCode;
            }
        }

        public VertexDeclaration GetLayout()
        {
            return Layout;
        }

        public void FlipWinding()
        {
        }

        public static bool operator ==(TerrainVertex left, TerrainVertex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TerrainVertex left, TerrainVertex right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format("Position: {0}, Normal: {1}, Color: {2}", Position, Normal, Color);
        }
    }
}
