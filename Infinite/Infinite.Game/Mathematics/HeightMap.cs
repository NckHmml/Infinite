using System;

namespace Infinite.Mathematics
{
    public class HeightMap<T>
    {
        private T[,] Buffer;
        private int Size;

        public HeightMap(int size)
        {
            Buffer = new T[size + 2, size + 2];
            Size = size;
        }

        public void Fill(Func<int, int, T> fill)
        {
            for (int x = -1; x <= Size; x++)
            {
                for (int z = -1; z <= Size; z++)
                {
                    Buffer[x + 1, z + 1] = fill(x, z);
                }
            }
        }

        public T this[int x, int z]
        {
            get
            {
                return Buffer[x + 1, z + 1];
            }
            set
            {
                Buffer[x + 1, z + 1] = value;
            }
        }
    }
}
