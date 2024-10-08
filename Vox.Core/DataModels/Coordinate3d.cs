namespace Vox.Core.DataModels
{
    public abstract class Coordinate3d<T>
    {
        public T X { get; set; }
        public T Y { get; set; }
        public T Z { get; set; }

        protected Coordinate3d(T x, T y, T z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
