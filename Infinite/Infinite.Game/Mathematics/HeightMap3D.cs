using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinite.Mathematics
{
    public class HeightMap3D<T>
    {
        private T[,,] Buffer;
        private int Size;

        public HeightMap3D(int size)
        {
            Buffer = new T[size + 2, size + 2, size + 2];
            Size = size;
        }

        public void Fill(Func<int, int, int, T> fill)
        {
            for (int x = -1; x <= Size; x++)
            {
                for (int y = -1; y <= Size; y++)
                {
                    for (int z = -1; z <= Size; z++)
                    {
                        Buffer[x + 1, y + 1, z + 1] = fill(x, y, z);
                    }
                }
            }
        }

        public T this[int x, int y, int z]
        {
            get
            {
                return Buffer[x + 1, y + 1, z + 1];
            }
            set
            {
                Buffer[x + 1, y + 1, z + 1] = value;
            }
        }
    }
}
