using System;
using System.Collections.Generic;
using Vox.Core.DataModels;

namespace Vox.Core.Algorithms.BoundingVolumeHierarchy
{
    /// <summary>
    /// Bounding Volume Hierarchy (BVH) Acceleration Structure
    /// </summary>
    public class BVH
    {
        public readonly PMesh Mesh;
        private readonly int _maxTrianglesPerLeaf;
        private readonly int _numBucket;
        public BVHNode Root { get; private set; }

        public BVH(PMesh mesh, int maxTrianglesPerLeaf = 8, int numBucket = 4)
        {
            Mesh = mesh;
            _numBucket = numBucket;
            _maxTrianglesPerLeaf = maxTrianglesPerLeaf;
            var allTriangleIndices = GetAllTriangleIndices();
            Root = BuildRecursive(allTriangleIndices);
        }

        private List<int> GetAllTriangleIndices()
        {
            int triangleCount = Mesh.Faces.Count;
            var indices = new List<int>(triangleCount);
            for (int i = 0; i < triangleCount; i++)
            {
                indices.Add(i);
            }
            return indices;
        }

        private BVHNode BuildRecursive(List<int> triangleIndices)
        {
            // Create a new node and compute its bounding box
            var node = new BVHNode();
            node.Bounds = ComputeBounds(triangleIndices);

            // If the number of triangles is below the threshold, make a leaf node
            if (triangleIndices.Count <= _maxTrianglesPerLeaf)
            {
                node.TriangleIndices = triangleIndices.ToArray();
                return node;
            }

            // Initialize variables for SAH
            int bestAxis = -1;
            double bestCost = double.PositiveInfinity;
            int bestSplitIndex = -1;

            // Initialize buckets
            var buckets = new BucketInfo[_numBucket, 3]; // 3 axes

            // Compute centroid bounds
            var centroidBounds = ComputeCentroidBounds(triangleIndices);

            if (centroidBounds.IsDegenerate())
            {
                // All centroids are the same, create a leaf node
                node.TriangleIndices = triangleIndices.ToArray();
                return node;
            }

            // For each axis
            for (int axis = 0; axis < 3; axis++)
            {
                // Initialize buckets
                for (int i = 0; i < _numBucket; i++)
                {
                    buckets[i, axis] = new BucketInfo();
                }

                // Place triangles into buckets
                foreach (var idx in triangleIndices)
                {
                    double centroid = GetTriangleCentroid(idx, axis);
                    double minCentroid = centroidBounds.Min.ToArray()[axis];
                    double maxCentroid = centroidBounds.Max.ToArray()[axis];
                    double range = maxCentroid - minCentroid;

                    // Avoid division by zero
                    if (range == 0) range = 1e-6;

                    int bucketIndex = (int)(_numBucket * ((centroid - minCentroid) / range));
                    bucketIndex = Math.Max(0, Math.Min(bucketIndex, _numBucket - 1));

                    buckets[bucketIndex, axis].Triangles.Add(idx);
                    buckets[bucketIndex, axis].Bounds.Expand(Mesh.TriangleBounds[idx]);
                }

                // Compute costs for splitting after each bucket
                for (int i = 1; i < _numBucket; i++)
                {
                    // Left side
                    var leftBounds = new PBoundingBox();
                    int leftCount = 0;
                    for (int j = 0; j < i; j++)
                    {
                        leftBounds.Expand(buckets[j, axis].Bounds);
                        leftCount += buckets[j, axis].Triangles.Count;
                    }

                    // Right side
                    var rightBounds = new PBoundingBox();
                    int rightCount = 0;
                    for (int j = i; j < _numBucket; j++)
                    {
                        rightBounds.Expand(buckets[j, axis].Bounds);
                        rightCount += buckets[j, axis].Triangles.Count;
                    }

                    // Compute cost
                    double cost = 1 + (leftCount * leftBounds.SurfaceArea() + rightCount * rightBounds.SurfaceArea()) / node.Bounds.SurfaceArea();

                    // Update best split
                    if (cost < bestCost)
                    {
                        bestCost = cost;
                        bestAxis = axis;
                        bestSplitIndex = i;
                    }
                }
            }

            // Now that bestCost is computed, check if splitting is beneficial
            const double minSplitImprovement = 0.05;

            // Calculate the cost of not splitting (i.e., making a leaf node)
            double leafCost = triangleIndices.Count;

            // Calculate improvement as the ratio of improvement from not splitting to splitting
            double improvement = (leafCost - bestCost) / leafCost;

            // If the improvement in cost is less than the threshold, or if no valid split was found, return a leaf node
            if (improvement < minSplitImprovement || bestAxis == -1)
            {
                node.TriangleIndices = triangleIndices.ToArray();
                return node;
            }

            // Partition triangles into left and right sets
            var leftIndices = new List<int>();
            var rightIndices = new List<int>();
            for (int i = 0; i < _numBucket; i++)
            {
                var bucket = buckets[i, bestAxis];
                if (i < bestSplitIndex)
                {
                    leftIndices.AddRange(bucket.Triangles);
                }
                else
                {
                    rightIndices.AddRange(bucket.Triangles);
                }
            }

            // Recursively build child nodes
            node.Left = BuildRecursive(leftIndices);
            node.Right = BuildRecursive(rightIndices);

            return node;
        }

        private PBoundingBox ComputeBounds(List<int> triangleIndices)
        {
            var bounds = new PBoundingBox();
            foreach (int idx in triangleIndices)
            {
                bounds.Expand(Mesh.TriangleBounds[idx]);
            }
            return bounds;
        }

        private PBoundingBox ComputeCentroidBounds(List<int> triangleIndices)
        {
            var bounds = new PBoundingBox();
            foreach (int idx in triangleIndices)
            {
                var centroid = GetTriangleCentroidPoint(idx);
                bounds.Expand(centroid);
            }
            return bounds;
        }

        private PVector3d GetTriangleCentroidPoint(int triangleIndex)
        {
            var face = Mesh.Faces[triangleIndex];
            var v0 = Mesh.Vertices[face[0]];
            var v1 = Mesh.Vertices[face[1]];
            var v2 = Mesh.Vertices[face[2]];
            return (v0 + v1 + v2) / 3.0f;
        }

        private double GetTriangleCentroid(int triangleIndex, int axis)
        {
            var centroid = GetTriangleCentroidPoint(triangleIndex);
            return centroid.ToArray()[axis];
        }

        private class BucketInfo
        {
            public List<int> Triangles = new List<int>();
            public PBoundingBox Bounds = new PBoundingBox();
        }
    }

}
