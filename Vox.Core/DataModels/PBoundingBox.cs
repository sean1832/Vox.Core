using System;
using System.Collections.Generic;
using System.Text;

namespace Vox.Core.DataModels
{
    public class PBoundingBox
    {
        public PVector3d Min;
        public PVector3d Max;

        public PBoundingBox(PVector3d min, PVector3d max)
        {
            Min = min;
            Max = max;
        }

        public PBoundingBox()
        {
            Min = new PVector3d(double.MaxValue, double.MaxValue, double.MaxValue);
            Max = new PVector3d(double.MinValue, double.MinValue, double.MinValue);
        }

        public PVector3d Center => (Min + Max) / 2;
        public PVector3d Size => Max - Min;

        public PVector3d[] Corners => new PVector3d[2] { Min, Max };

        /// <summary>
        /// Check if the bounding box is degenerate. A bounding box is degenerate if the min values are greater than the max values.
        /// </summary>
        /// <returns></returns>
        public bool IsDegenerate()
        {
            return Min.X > Max.X || Min.Y > Max.Y || Min.Z > Max.Z;
        }

        public double SurfaceArea()
        {
            var size = Size;
            return 2 * (size.X * size.Y + size.X * size.Z + size.Y * size.Z);
        }

        public bool Intersects(PBoundingBox other)
        {
            return (Min.X <= other.Max.X && Max.X >= other.Min.X) &&
                   (Min.Y <= other.Max.Y && Max.Y >= other.Min.Y) &&
                   (Min.Z <= other.Max.Z && Max.Z >= other.Min.Z);
        }

        public PBoundingBox ToCubic()
        {
            double maxSize = Math.Max(Size.X, Math.Max(Size.Y, Size.Z));
            PVector3d newMin = Center - new PVector3d(maxSize / 2.0, maxSize / 2.0, maxSize / 2.0);
            PVector3d newMax = Center + new PVector3d(maxSize / 2.0, maxSize / 2.0, maxSize / 2.0);

            return new PBoundingBox(newMin, newMax);
        }

        public void Expand(PBoundingBox bounds)
        {
            Min = PVector3d.Min(Min, bounds.Min);
            Max = PVector3d.Max(Max, bounds.Max);
        }

        public void Expand(PVector3d point)
        {
            Min = PVector3d.Min(Min, point);
            Max = PVector3d.Max(Max, point);
        }

        /// <summary>
        /// Get the longest axis of the bounding box
        /// </summary>
        /// <returns>Longest axis. (X=0, Y=1, Z=2)</returns>
        public int GetLongestAxis()
        {
            var size = Size;
            if (size.X > size.Y && size.X > size.Z)
                return 0; // X
            if (size.Y > size.Z)
                return 1; // Y
            return 2; // Z
        }
    }
}
