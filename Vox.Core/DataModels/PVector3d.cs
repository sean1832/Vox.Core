using System;

namespace Vox.Core.DataModels
{
    public class PVector3d : Coordinate3d<double>
    {
        public PVector3d(double x, double y, double z) : base(x, y, z)
        {
        }

        public PVector3d(): base(0,0,0)
        {
        }

        public static PVector3d Min(PVector3d a, PVector3d b)
        {
            return new PVector3d(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
        }

        public static PVector3d Max(PVector3d a, PVector3d b)
        {
            return new PVector3d(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
        }

        public double[] ToArray()
        {
            return new double[] { X, Y, Z };
        }

        public static PVector3d FromArray(double[] array)
        {
            return new PVector3d(array[0], array[1], array[2]);
        }

        public PVector3d Abs()
        {
            return new PVector3d(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));
        }

        public double MinComponent()
        {
            return Math.Min(X, Math.Min(Y, Z));
        }

        public double MaxComponent()
        {
            return Math.Max(X, Math.Max(Y, Z));
        }

        public PVector3d Normalize()
        {
            var magnitude = Math.Sqrt(X * X + Y * Y + Z * Z);
            return new PVector3d(X / magnitude, Y / magnitude, Z / magnitude);
        }

        public bool IsNormalized(double tolerance = 0.0001)
        {
            return Math.Abs(Math.Sqrt(X * X + Y * Y + Z * Z) - 1) < tolerance;
        }

        public PVector3df ToFloat()
        {
            return new PVector3df((float)X, (float)Y, (float)Z);
        }

        public static PVector3d CrossProduct(PVector3d a, PVector3d b)
        {
            return new PVector3d(a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X);
        }

        public static double DotProduct(PVector3d a, PVector3d b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static double DotProduct(double[] a, double[] b)
        {
            return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
        }

        public static double AngleBetween(PVector3d a, PVector3d b)
        {
            return Math.Acos(DotProduct(a, b) / (a.Magnitude() * b.Magnitude()));
        }

        public double Magnitude()
        {
            // Pythagorean theorem
            // a^2 + b^2 = c^2
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        // operators for vector math
        public static PVector3d operator +(PVector3d a, PVector3d b) => new PVector3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static PVector3d operator -(PVector3d a, PVector3d b) => new PVector3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static PVector3d operator *(PVector3d a, double b) => new PVector3d(a.X * b, a.Y * b, a.Z * b);
        public static PVector3d operator /(PVector3d a, double b) => new PVector3d(a.X / b, a.Y / b, a.Z / b);
    }

    public class PVector3df : Coordinate3d<float>
    {
        public PVector3df(float x, float y, float z) : base(x, y, z)
        {
        }

        public PVector3df Normalize()
        {
            var magnitude = (float)Math.Sqrt(X * X + Y * Y + Z * Z);
            return new PVector3df(X / magnitude, Y / magnitude, Z / magnitude);
        }

        public bool IsNormalized(float tolerance = 0.0001f)
        {
            return Math.Abs(Math.Sqrt(X * X + Y * Y + Z * Z) - 1) < tolerance;
        }

        public static PVector3df CrossProduct(PVector3df a, PVector3df b)
        {
            return new PVector3df(a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X);
        }

        public static double DotProduct(PVector3df a, PVector3df b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static double AngleBetween(PVector3df a, PVector3df b)
        {
            return Math.Acos(DotProduct(a, b) / (a.Magnitude() * b.Magnitude()));
        }

        public double Magnitude()
        {
            // Pythagorean theorem
            // a^2 + b^2 = c^2
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        // operators for vector math
        public static PVector3df operator +(PVector3df a, PVector3df b) => new PVector3df(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static PVector3df operator -(PVector3df a, PVector3df b) => new PVector3df(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static PVector3df operator *(PVector3df a, double b) => new PVector3df(a.X * (float)b, a.Y * (float)b, a.Z * (float)b);
        public static PVector3df operator /(PVector3df a, double b) => new PVector3df(a.X / (float)b, a.Y / (float)b, a.Z / (float)b);
    }
}
