using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinite.Mathematics
{
    public class GenericVector3<T> : Tuple<T, T, T>
    {
        public GenericVector3(T x, T y, T z): base(x, y, z)
        { }

        public T X
        {
            get
            {
                return Item1;
            }
        }

        public T Y
        {
            get
            {
                return Item2;
            }
        }

        public T Z
        {
            get
            {
                return Item3;
            }
        }
    }
}
