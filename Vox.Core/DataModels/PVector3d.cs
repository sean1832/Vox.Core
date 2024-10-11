using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vox.Core.DataModels
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct PVector3d
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;
        public const float Tolerance = 1e-6f;

        public PVector3d()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public PVector3d(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static readonly PVector3d Zero = new PVector3d(0, 0, 0);
        public static readonly PVector3d One = new PVector3d(1, 1, 1);
        public static readonly PVector3d UnitX = new PVector3d(1, 0, 0);
        public static readonly PVector3d UnitY = new PVector3d(0, 1, 0);
        public static readonly PVector3d UnitZ = new PVector3d(0, 0, 1);

        public static PVector3d Min(PVector3d a, PVector3d b)
        {
            return new PVector3d(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
        }

        public static PVector3d Max(PVector3d a, PVector3d b)
        {
            return new PVector3d(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
        }

        public float Min()
        {
            return Math.Min(X, Math.Min(Y, Z));
        }

        public float Max()
        {
            return Math.Max(X, Math.Max(Y, Z));
        }

        public PVector3d Abs()
        {
            return new PVector3d(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));
        }

        public float[] ToArray()
        {
            return new float[] { X, Y, Z };
        }

        public static PVector3d FromArray(float[] array)
        {
            return new PVector3d(array[0], array[1], array[2]);
        }

        public PVector3d Normalize()
        {
            float magnitude = Magnitude();
            if (magnitude > Tolerance)
                return new PVector3d(X / magnitude, Y / magnitude, Z / magnitude);
            return Zero; // Handle degenerate cases
        }

        public bool IsNormalized(float tolerance = 0.0001f)
        {
            return Math.Abs(Math.Sqrt(X * X + Y * Y + Z * Z) - 1) < tolerance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PVector3d CrossProduct(PVector3d a, PVector3d b)
        {
            return new PVector3d(a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DotProduct(PVector3d a, PVector3d b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DotProduct(float[] a, float[] b)
        {
            return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
        }

        public static float AngleBetween(PVector3d a, PVector3d b)
        {
            return (float)Math.Acos(DotProduct(a, b) / (a.Magnitude() * b.Magnitude()));
        }

        public float MagnitudeSquared()
        {
            return X * X + Y * Y + Z * Z;
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
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + X.GetHashCode(); // Using float.GetHashCode() to avoid division
                hash = hash * 23 + Y.GetHashCode();
                hash = hash * 23 + Z.GetHashCode();
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

    public readonly struct PVector3dEqualityComparer : IEqualityComparer<PVector3d>
    {
        private readonly float _tolerance;

        public PVector3dEqualityComparer(float tolerance)
        {
            _tolerance = tolerance;
        }

        public bool Equals(PVector3d a, PVector3d b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a.Equals(default(PVector3d)) || b.Equals(default(PVector3d)))
                return false;

            return Math.Abs(a.X - b.X) < _tolerance &&
                   Math.Abs(a.Y - b.Y) < _tolerance &&
                   Math.Abs(a.Z - b.Z) < _tolerance;
        }

        public int GetHashCode(PVector3d obj)
        {
            int xInt = (int)Math.Round(obj.X / _tolerance);
            int yInt = (int)Math.Round(obj.Y / _tolerance);
            int zInt = (int)Math.Round(obj.Z / _tolerance);

            unchecked
            {
                int hash = 17;
                hash = hash * 31 + xInt;
                hash = hash * 31 + yInt;
                hash = hash * 31 + zInt;
                return hash;
            }
        }
    }
}
