using System;
using System.Collections.Generic;
using System.Text;
using Vox.Core.DataModels;

namespace Vox.Core.Algorithm.BVH
{
    public class BVHNode
    {
        public PBoundingBox Bounds;
        public BVHNode Left;
        public BVHNode Right;
        public int[] TriangleIndices; // For leaf nodes
        public bool IsLeaf => TriangleIndices != null;
    }

}
