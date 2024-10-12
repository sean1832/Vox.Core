using System;
using Vox.Core.DataModels;

namespace Vox.Core.Algorithms.Collision
{
    internal static class RayCollision
    {
        public static bool RayIntersectsBounds(PVector3d rayOrigin, PVector3d rayDirection, PBoundingBox aabb)
        {
            // Implement the slab method for ray-AABB intersection
            double tXMin = (aabb.Min.X - rayOrigin.X) / rayDirection.X;
            double tMax = (aabb.Max.X - rayOrigin.X) / rayDirection.X;
            if (tXMin > tMax) Swap(ref tXMin, ref tMax); // Swap tXMin and tMax if tMin is greater than tXMax

            double tYMin = (aabb.Min.Y - rayOrigin.Y) / rayDirection.Y;
            double tYMax = (aabb.Max.Y - rayOrigin.Y) / rayDirection.Y;
            if (tYMin > tYMax) Swap(ref tYMin, ref tYMax); // Swap tYMin and tYMax if tYMin is greater than tYMax

            // Check if the ray misses the AABB
            if ((tXMin > tYMax) || (tYMin > tMax))
                return false;

            // Update tMin and tMax
            if (tYMin > tXMin)
                tXMin = tYMin;
            if (tYMax < tMax)
                tMax = tYMax;

            // Check the Z-axis
            double tZMin = (aabb.Min.Z - rayOrigin.Z) / rayDirection.Z;
            double tZNax = (aabb.Max.Z - rayOrigin.Z) / rayDirection.Z;
            if (tZMin > tZNax) Swap(ref tZMin, ref tZNax);

            // Check if the ray misses the AABB
            if ((tXMin > tZNax) || (tZMin > tMax))
                return false;

            return true; // Ray intersects the AABB
        }

        public static bool RayIntersectsTriangle(
            PVector3d rayOrigin,
            PVector3d rayDirection,
            PVector3d v0,
            PVector3d v1,
            PVector3d v2)
        {
            const double EPSILON = 1e-8;

            PVector3d edge1 = v1 - v0;
            PVector3d edge2 = v2 - v0;

            PVector3d h = PVector3d.CrossProduct(rayDirection, edge2);
            double a = PVector3d.DotProduct(edge1, h);

            if (a > -EPSILON && a < EPSILON)
                return false; // Ray is parallel to triangle

            double f = 1.0 / a;
            PVector3d s = rayOrigin - v0;
            double u = f * PVector3d.DotProduct(s, h);

            if (u < 0.0 || u > 1.0)
                return false;

            PVector3d q = PVector3d.CrossProduct(s, edge1);
            double v = f * PVector3d.DotProduct(rayDirection, q);

            if (v < 0.0 || u + v > 1.0)
                return false;

            // At this stage, we can compute t to find out where the intersection point is on the line
            double t = f * PVector3d.DotProduct(edge2, q);

            if (t > EPSILON) // Ray intersection
                return true;
            else // Line intersection but not a ray intersection
                return false;
        }
        private static void Swap(ref double a, ref double b)
        {
            (a, b) = (b, a);
        }
    }
}
