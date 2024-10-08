using System;
using System.Collections.Generic;
using System.Text;
using Vox.Core.DataModels;

namespace Vox.Core.Algorithm.BVH
{
    internal class BoundingVolumeHierarchy
    {
        private readonly PMesh _mesh;
        public BoundingVolumeHierarchy(PMesh mesh)
        {
            _mesh = mesh;
        }

        public List<int> GetAllTriangleIndices()
        {
            var allTriangleIndices = new List<int>();
            for (int i = 0; i < _mesh.Faces.Count; i++)
            {
                allTriangleIndices.Add(i);
            }

            return allTriangleIndices;
        }

        public BVHNode BuildRecursive(List<int> triangleIndices)
        {
            var node = new BVHNode();
            
            // compute bounding box of this node
            node.Bounds = ComputeBounds(triangleIndices);

            if (triangleIndices.Count <= 4)
            {
                // leaf node
                node.TriangleIndices = triangleIndices;
                return node;
            }

            // determine axis to split
            int axis = node.Bounds.GetLongestAxis();

            // sort triangles along the axis
            triangleIndices.Sort((a, b) =>
            {
                double centerA = GetTriangleCenter(a, axis);
                double centerB = GetTriangleCenter(b, axis);
                return centerA.CompareTo(centerB);
            });

            // split the triangles into two groups
            int mid = triangleIndices.Count / 2;
            var leftTriangles = triangleIndices.GetRange(0, mid);
            var rightTriangles = triangleIndices.GetRange(mid, triangleIndices.Count - mid);

            // recursively build left and right children
            node.Left = BuildRecursive(leftTriangles);
            node.Right = BuildRecursive(rightTriangles);

            return node;
        }

        private PBoundingBox ComputeBounds(List<int> triangleIndices)
        {
            PBoundingBox bounds = new PBoundingBox();

            foreach (int idx in triangleIndices)
            {
                var triangleBounds = _mesh.TriangleBounds[idx];
                bounds.Expand(triangleBounds);
            }
            return bounds;
        }

        private double GetTriangleCenter(int triangleIndex, int axis)
        {
            PBoundingBox bounds = _mesh.TriangleBounds[triangleIndex];
            return (bounds.Min.ToArray()[axis] + bounds.Max.ToArray()[axis]) * 0.5;
        }

    }
}
