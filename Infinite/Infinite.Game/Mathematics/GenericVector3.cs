using System;

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

        public static bool operator ==(GenericVector3<T> left, GenericVector3<T> right)
        {
            return left?.Equals(right) ?? false;
        }

        public static bool operator !=(GenericVector3<T> left, GenericVector3<T> right)
        {
            return !(left == right);
        }
    }
}
