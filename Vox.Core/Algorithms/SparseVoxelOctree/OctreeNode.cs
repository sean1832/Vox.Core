using Vox.Core.DataModels;

namespace Vox.Core.Algorithms.SparseVoxelOctree
{
    internal class OctreeNode
    {
        public PBoundingBox Bounds;
        public bool IsLeaf;
        public bool IsOccupied;
        public OctreeNode[]? Children;
        public int Depth = 0;
        public VoxelState State = VoxelState.Outside;

        public OctreeNode(PBoundingBox bounds)
        {
            Bounds = bounds;
            IsLeaf = false;
            IsOccupied = false;
            Children = null;
        }

        public void Subdivide()
        {
            Children = new OctreeNode[8];
            int index = 0;

            PVector3d min = Bounds.Min;
            PVector3d max = Bounds.Max;
            PVector3d center = Bounds.Center;

            // create 8 child nodes
            for (int x = 0; x <= 1; x++)
            {
                for (int y = 0; y <= 1; y++)
                {
                    for (int z = 0; z <= 1; z++)
                    {
                        // if x, y, z are 0, then use min, otherwise use center
                        PVector3d childMin = new PVector3d(
                            x == 0 ? min.X : center.X,
                            y == 0 ? min.Y : center.Y,
                            z == 0 ? min.Z : center.Z);

                        // if x, y, z are 0, then use center, otherwise use max
                        PVector3d childMax = new PVector3d(
                            x == 0 ? center.X : max.X,
                            y == 0 ? center.Y : max.Y,
                            z == 0 ? center.Z : max.Z);

                        Children[index] = new OctreeNode(new PBoundingBox(childMin, childMax));
                        index++;
                    }
                }
            }
        }
    }
}
