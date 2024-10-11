using System;

namespace Vox.Core.DataModels
{
    public class PVector3d : Coordinate3d<float>
    {
        public float Tolerance = 1e-6f;

        public PVector3d(float x, float y, float z) : base(x, y, z)
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

        public float[] ToArray()
        {
            return new float[] { X, Y, Z };
        }

        public static PVector3d FromArray(float[] array)
        {
            return new PVector3d(array[0], array[1], array[2]);
        }

        public PVector3d Abs()
        {
            return new PVector3d(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));
        }

        public float MinComponent()
        {
            return Math.Min(X, Math.Min(Y, Z));
        }

        public float MaxComponent()
        {
            return Math.Max(X, Math.Max(Y, Z));
        }

        public PVector3d Normalize()
        {
            float magnitude = (float)Math.Sqrt(X * X + Y * Y + Z * Z);
            return new PVector3d(X / magnitude, Y / magnitude, Z / magnitude);
        }

        public bool IsNormalized(float tolerance = 0.0001f)
        {
            return Math.Abs(Math.Sqrt(X * X + Y * Y + Z * Z) - 1) < tolerance;
        }

        public static PVector3d CrossProduct(PVector3d a, PVector3d b)
        {
            return new PVector3d(a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X);
        }

        public static float DotProduct(PVector3d a, PVector3d b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static float DotProduct(float[] a, float[] b)
        {
            return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
        }

        public static float AngleBetween(PVector3d a, PVector3d b)
        {
            return (float)Math.Acos(DotProduct(a, b) / (a.Magnitude() * b.Magnitude()));
        }

        public float Magnitude()
        {
            // Pythagorean theorem
            // a^2 + b^2 = c^2
            return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public override bool Equals(object obj)
        {
            if (obj is PVector3d other)
            {
                return Math.Abs(X - other.X) < Tolerance &&
                       Math.Abs(Y - other.Y) < Tolerance &&
                       Math.Abs(Z - other.Z) < Tolerance;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int xInt = (int)Math.Round(X / Tolerance);
            int yInt = (int)Math.Round(Y / Tolerance);
            int zInt = (int)Math.Round(Z / Tolerance);

            unchecked
            {
                int hash = 17;
                hash = hash * 23 + xInt;
                hash = hash * 23 + yInt;
                hash = hash * 23 + zInt;
                return hash;
            }
        }


        // operators for vector math
        public static PVector3d operator +(PVector3d a, PVector3d b) => new PVector3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static PVector3d operator -(PVector3d a, PVector3d b) => new PVector3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static PVector3d operator *(PVector3d a, PVector3d b) => new PVector3d(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        public static PVector3d operator *(PVector3d a, float b) => new PVector3d(a.X * b, a.Y * b, a.Z * b);
        public static PVector3d operator /(PVector3d a, float b) => new PVector3d(a.X / b, a.Y / b, a.Z / b);
        public static PVector3d operator /(PVector3d a, PVector3d b) => new PVector3d(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
    }
}
