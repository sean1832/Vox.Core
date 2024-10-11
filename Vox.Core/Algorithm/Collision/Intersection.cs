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

                if (RayCollision.TriangleIntersectsAABB(v0, v1, v2, nodeBounds))
                {
                    return true; // Node intersects the mesh
                }
            }
            return false;
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

                    if (RayCollision.TriangleIntersectsAABB(v0, v1, v2, nodeBounds))
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
            PVector3d rayDirection = new PVector3d(1, 0.5f, 0.25f); // Arbitrary direction
            int intersections = CountRayIntersections(point, rayDirection, _bvh.Root);

            // Point is inside if intersections are odd
            return (intersections % 2) == 1;
        }

        private int CountRayIntersections(PVector3d rayOrigin, PVector3d rayDirection, BVHNode bvhNode)
        {
            if (!RayCollision.RayIntersectsAABB(rayOrigin, rayDirection, bvhNode.Bounds))
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

                    if (RayCollision.RayIntersectsTriangle(rayOrigin, rayDirection, v0, v1, v2))
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
    }
}
