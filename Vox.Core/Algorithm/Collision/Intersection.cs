using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Vox.Core.Algorithm.BVH;
using Vox.Core.Algorithm.SVO;
using Vox.Core.DataModels;

namespace Vox.Core.Algorithm.Collision
{
    internal class Intersection
    {
        private readonly BoundingVolumeHierarchy? _bvh;

        public Intersection()
        {
        }

        public Intersection(BoundingVolumeHierarchy bvh)
        {
            _bvh = bvh;
        }

        public bool IsNodeIntersect(PBoundingBox nodeBounds)
        {
            if (_bvh == null)
            {
                throw new InvalidOperationException($"{nameof(BoundingVolumeHierarchy)} not initialized!");
            }

            return IntersectBVHNode(_bvh.Root, nodeBounds);
        }

        private bool IntersectBVHNode(BVHNode bvhNode, PBoundingBox nodeBounds)
        {
            // Check if the BVH node's bounding box intersects with the nodeBounds
            if (!bvhNode.Bounds.Intersects(nodeBounds))
                return false;

            if (bvhNode.IsLeaf)
            {
                // Check for triangle intersections
                foreach (int idx in bvhNode.TriangleIndices)
                {
                    var face = _bvh.Mesh.Faces[idx];
                    var v0 = _bvh.Mesh.Vertices[face[0]];
                    var v1 = _bvh.Mesh.Vertices[face[1]];
                    var v2 = _bvh.Mesh.Vertices[face[2]];

                    if (TriangleIntersectsAABB(v0, v1, v2, nodeBounds))
                    {
                        return true; // Intersection found
                    }
                }
                return false;
            }
            else
            {
                // Recursively check child nodes
                return IntersectBVHNode(bvhNode.Left, nodeBounds) || IntersectBVHNode(bvhNode.Right, nodeBounds);
            }
        }

        public bool IsNodeIntersect(PBoundingBox nodeBounds, PMesh mesh)
        {
            int triangleBoundsCount = mesh.TriangleBounds.Count;
            if (triangleBoundsCount == 0)
                throw new InvalidOperationException("Mesh triangle bounds is not pre-calculated.");

            // filter mesh face that could intersect with the node's bounding box
            var nearTrianglesIdxBag = new ConcurrentBag<int>();

            Parallel.For(0, triangleBoundsCount, (i) =>
            {
                if (nodeBounds.Intersects(mesh.TriangleBounds[i]))
                {
                    nearTrianglesIdxBag.Add(i);
                }
            });

            // perform a more precise triangle-AABB intersection test
            foreach (int idx in nearTrianglesIdxBag)
            {
                var face = mesh.Faces[idx];
                var v0 = mesh.Vertices[face[0]];
                var v1 = mesh.Vertices[face[1]];
                var v2 = mesh.Vertices[face[2]];

                if (TriangleIntersectsAABB(v0, v1, v2, nodeBounds))
                {
                    return true; // Node intersects the mesh
                }
            }
            return false;
        }

        public bool IsFullyInside(PBoundingBox nodeBounds, PMesh mesh)
        {
            var corners = nodeBounds.Corners;

            foreach (var corner in corners)
            {
                if (!IsPointInsideMesh(corner))
                {
                    // If any corner is outside, the node is not fully inside
                    return false;
                }
            }

            // Additionally, ensure the node does not intersect the mesh surface
            if (IsNodeIntersect(nodeBounds, mesh))
            {
                return false;
            }

            return true;
        }

        private bool IsPointInsideMesh(PVector3d point)
        {
            // Use a ray casting method optimized with BVH
            PVector3d rayDirection = new PVector3d(1, 0.5, 0.25); // Arbitrary direction
            int intersections = CountRayIntersections(point, rayDirection, _bvh.Root);

            // Point is inside if intersections are odd
            return (intersections % 2) == 1;
        }

        private int CountRayIntersections(PVector3d rayOrigin, PVector3d rayDirection, BVHNode bvhNode)
        {
            if (!RayIntersectsAABB(rayOrigin, rayDirection, bvhNode.Bounds))
                return 0;

            if (bvhNode.IsLeaf)
            {
                int count = 0;
                foreach (int idx in bvhNode.TriangleIndices)
                {
                    var face = _bvh.Mesh.Faces[idx];
                    var v0 = _bvh.Mesh.Vertices[face[0]];
                    var v1 = _bvh.Mesh.Vertices[face[1]];
                    var v2 = _bvh.Mesh.Vertices[face[2]];

                    if (RayIntersectsTriangle(rayOrigin, rayDirection, v0, v1, v2))
                    {
                        count++;
                    }
                }
                return count;
            }
            else
            {
                return CountRayIntersections(rayOrigin, rayDirection, bvhNode.Left) +
                       CountRayIntersections(rayOrigin, rayDirection, bvhNode.Right);
            }
        }

        private bool RayIntersectsAABB(PVector3d rayOrigin, PVector3d rayDirection, PBoundingBox aabb)
        {
            // Implement the slab method for ray-AABB intersection
            double tmin = (aabb.Min.X - rayOrigin.X) / rayDirection.X;
            double tmax = (aabb.Max.X - rayOrigin.X) / rayDirection.X;
            if (tmin > tmax) Swap(ref tmin, ref tmax);

            double tymin = (aabb.Min.Y - rayOrigin.Y) / rayDirection.Y;
            double tymax = (aabb.Max.Y - rayOrigin.Y) / rayDirection.Y;
            if (tymin > tymax) Swap(ref tymin, ref tymax);

            if ((tmin > tymax) || (tymin > tmax))
                return false;

            if (tymin > tmin)
                tmin = tymin;
            if (tymax < tmax)
                tmax = tymax;

            double tzmin = (aabb.Min.Z - rayOrigin.Z) / rayDirection.Z;
            double tzmax = (aabb.Max.Z - rayOrigin.Z) / rayDirection.Z;
            if (tzmin > tzmax) Swap(ref tzmin, ref tzmax);

            if ((tmin > tzmax) || (tzmin > tmax))
                return false;

            return true;
        }

        private void Swap(ref double a, ref double b)
        {
            (a, b) = (b, a);
        }


        private bool RayIntersectsTriangle(
            PVector3d rayOrigin,
            PVector3d rayDirection,
            PVector3d vertex0,
            PVector3d vertex1,
            PVector3d vertex2)
        {
            const double EPSILON = 1e-8;

            PVector3d edge1 = vertex1 - vertex0;
            PVector3d edge2 = vertex2 - vertex0;

            PVector3d h = PVector3d.CrossProduct(rayDirection, edge2);
            double a = PVector3d.DotProduct(edge1, h);

            if (a > -EPSILON && a < EPSILON)
                return false; // Ray is parallel to triangle

            double f = 1.0 / a;
            PVector3d s = rayOrigin - vertex0;
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
        private bool TriangleIntersectsAABB(PVector3d v0, PVector3d v1, PVector3d v2, PBoundingBox box)
        {
            // Implementing the Separating Axis Theorem (SAT) for triangle-AABB intersection

            // Move triangle into box's local coordinate frame
            PVector3d boxCenter = (box.Min + box.Max) * 0.5;
            PVector3d boxHalfSize = (box.Max - box.Min) * 0.5;

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

        private bool PlaneBoxOverlap(PVector3d normal, PVector3d vert, PVector3d maxBox)
        {
            double[] vMin = new PVector3d().ToArray();
            double[] vMax = new PVector3d().ToArray();



            double[] vertArray = vert.ToArray();
            double[] normalArray = normal.ToArray();
            double[] maxBoxArray = maxBox.ToArray();

            for (int q = 0; q < 3; q++)
            {
                double v = vertArray[q];
                double n = normalArray[q];
                double max = maxBoxArray[q];

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

        private bool AxisOverlapTest(PVector3d v0, PVector3d v1, PVector3d v2, PVector3d boxHalfSize)
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
    }
}
