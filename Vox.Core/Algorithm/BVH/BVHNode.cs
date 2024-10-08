using System;
using System.Collections.Generic;
using System.Text;
using Vox.Core.DataModels;

namespace Vox.Core.Algorithm.BVH
{
    internal class BVHNode
    {
        public PBoundingBox? Bounds;
        public BVHNode? Left;
        public BVHNode? Right;
        public List<int>? TriangleIndices; // Indices of triangles in this leaf node
        public BVHNode()
        {
            Left = null;
            Right = null;
            TriangleIndices = null;
        }

    }
}
