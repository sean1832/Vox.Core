using System;
using System.Collections.Generic;
using System.Text;
using Vox.Core.DataModels;

namespace Vox.Core.Algorithm.Collision
{
    internal static class RayCollision
    {
        public static bool RayIntersectsAABB(PVector3d rayOrigin, PVector3d rayDirection, PBoundingBox aabb)
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

        // Triangle-AABB intersection test
        public static bool TriangleIntersectsAABB(PVector3d v0, PVector3d v1, PVector3d v2, PBoundingBox box)
        {
            // Implementing the Separating Axis Theorem (SAT) for triangle-AABB intersection

            // Move triangle into box's local coordinate frame
            PVector3d boxCenter = (box.Min + box.Max) * 0.5f;
            PVector3d boxHalfSize = (box.Max - box.Min) * 0.5f;

            PVector3d v0b = v0 - boxCenter;
            PVector3d v1b = v1 - boxCenter;
            PVector3d v2b = v2 - boxCenter;

            // Compute triangle edges
            PVector3d e0 = v1b - v0b;
            PVector3d e1 = v2b - v1b;
            PVector3d e2 = v0b - v2b;

            // Test the triangle normal
            PVector3d normal = PVector3d.CrossProduct(e0, e1);
            if (!PlaneBoxOverlap(normal, v0b, boxHalfSize))
                return false;

            // Define the axes to test (cross products of edges and coordinate axes)
            PVector3d[] axes = {
                new PVector3d(0, -e0.Z, e0.Y),
                new PVector3d(e0.Z, 0, -e0.X),
                new PVector3d(-e0.Y, e0.X, 0),
                new PVector3d(0, -e1.Z, e1.Y),
                new PVector3d(e1.Z, 0, -e1.X),
                new PVector3d(-e1.Y, e1.X, 0),
                new PVector3d(0, -e2.Z, e2.Y),
                new PVector3d(e2.Z, 0, -e2.X),
                new PVector3d(-e2.Y, e2.X, 0)
            };


            // Test the 9 axes
            foreach (var axis in axes)
            {
                double p0 = PVector3d.DotProduct(v0b, axis);
                double p1 = PVector3d.DotProduct(v1b, axis);
                double p2 = PVector3d.DotProduct(v2b, axis);

                double r = box.Size.X / 2 * Math.Abs(axis.X) +
                           box.Size.Y / 2 * Math.Abs(axis.Y) +
                           box.Size.Z / 2 * Math.Abs(axis.Z);

                if (Math.Max(-Math.Max(p0, Math.Max(p1, p2)), Math.Min(p0, Math.Min(p1, p2))) > r)
                    return false; // No intersection
            }

            // Test the box face normals (AABB axes)
            if (!AxisOverlapTest(v0b, v1b, v2b, boxHalfSize))
                return false;

            return true; // Intersection occurs
        }

        private static bool PlaneBoxOverlap(PVector3d normal, PVector3d vert, PVector3d maxBox)
        {
            float[] vMin = new PVector3d().ToArray();
            float[] vMax = new PVector3d().ToArray();



            float[] vertArray = vert.ToArray();
            float[] normalArray = normal.ToArray();
            float[] maxBoxArray = maxBox.ToArray();

            for (int q = 0; q < 3; q++)
            {
                float v = vertArray[q];
                float n = normalArray[q];
                float max = maxBoxArray[q];

                if (n > 0.0f)
                {
                    vMin[q] = -max - v;
                    vMax[q] = max - v;
                }
                else
                {
                    vMin[q] = max - v;
                    vMax[q] = -max - v;
                }
            }

            if (PVector3d.DotProduct(normalArray, vMin) > 0.0f)
                return false;
            if (PVector3d.DotProduct(normalArray, vMax) >= 0.0f)
                return true;

            return false;
        }
        private static bool AxisOverlapTest(PVector3d v0, PVector3d v1, PVector3d v2, PVector3d boxHalfSize)
        {
            // Test overlap along X-axis
            double min = Math.Min(v0.X, Math.Min(v1.X, v2.X));
            double max = Math.Max(v0.X, Math.Max(v1.X, v2.X));
            if (min > boxHalfSize.X || max < -boxHalfSize.X)
                return false;

            // Test overlap along Y-axis
            min = Math.Min(v0.Y, Math.Min(v1.Y, v2.Y));
            max = Math.Max(v0.Y, Math.Max(v1.Y, v2.Y));
            if (min > boxHalfSize.Y || max < -boxHalfSize.Y)
                return false;

            // Test overlap along Z-axis
            min = Math.Min(v0.Z, Math.Min(v1.Z, v2.Z));
            max = Math.Max(v0.Z, Math.Max(v1.Z, v2.Z));
            if (min > boxHalfSize.Z || max < -boxHalfSize.Z)
                return false;

            return true;
        }

        private static void Swap(ref double a, ref double b)
        {
            (a, b) = (b, a);
        }
    }
}
