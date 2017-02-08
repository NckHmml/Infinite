using System;

namespace Infinite.Mathematics
{
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
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
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
}
