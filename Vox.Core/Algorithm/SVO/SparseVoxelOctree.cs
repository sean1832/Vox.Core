using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Vox.Core.Algorithm.BVH;
using Vox.Core.Algorithm.Collision;
using Vox.Core.DataModels;

namespace Vox.Core.Algorithm.SVO
{
    internal class SparseVoxelOctree
    {
        private readonly int _maxDepth;
        private readonly PVector3d _rootSize;
        private readonly bool _isSolid;
        private readonly Intersection _intersector;
        private readonly BoundingVolumeHierarchy? _bvh;

        public SparseVoxelOctree(int maxDepth, PVector3d rootSize, bool isSolid)
        {
            _maxDepth = maxDepth;
            _rootSize = rootSize;
            _isSolid = isSolid;
            _intersector = new Intersection();
        }

        public SparseVoxelOctree(int maxDepth, PVector3d rootSize, bool isSolid, BoundingVolumeHierarchy bvh)
        {
            _maxDepth = maxDepth;
            _rootSize = rootSize;
            _isSolid = isSolid;
            _bvh = bvh;
            _intersector = new Intersection(bvh);
        }

        private VoxelState GetState(PBoundingBox nodeBounds, PMesh mesh, bool isSolid)
        {

            if (_intersector.IsNodeIntersect(nodeBounds, mesh))
            {
                return VoxelState.Intersecting;
            }

            if (isSolid && _intersector.IsFullyInside(nodeBounds, mesh))
            {
                return VoxelState.Inside;
            }

            return VoxelState.Outside;
        }

        private VoxelState GetStateBVH(PBoundingBox nodeBounds, PMesh mesh, bool isSolid)
        {
            if (_intersector.IsNodeIntersect(nodeBounds))
            {
                return VoxelState.Intersecting;
            }

            if (isSolid && _intersector.IsFullyInside(nodeBounds, mesh))
            {
                return VoxelState.Inside;
            }

            return VoxelState.Outside;
        }

        public void Build(OctreeNode node, PMesh mesh, int depth = 0)
        {
            VoxelState state;
            if (_bvh != null)
            {
                state = GetStateBVH(node.Bounds, mesh, _isSolid);
            }
            else
            {
                state = GetState(node.Bounds, mesh, _isSolid);
            }
            

            switch (state)
            {
                case VoxelState.Outside:
                    // Node does not intersect the mesh
                    node.IsLeaf = true;
                    node.IsOccupied = false;
                    node.Depth = depth;
                    node.State = state;
                    return;
                case VoxelState.Inside:
                    // Node is entirely inside the mesh, mark it as fully occupied and stop subdividing
                    node.IsLeaf = true;
                    node.IsOccupied = true;
                    node.Depth = depth;
                    node.State = state;
                    return;
                case VoxelState.Intersecting:
                    // Node intersects the mesh, subdivide if not at max depth
                    if (depth >= _maxDepth)
                    {
                        node.IsLeaf = true;
                        node.IsOccupied = true;
                        node.Depth = depth;
                        node.State = state;
                        return;
                    }

                    // Subdivide the node
                    node.Subdivide();

                    if (node.Children == null) return;

                    // Recursively build the children in parallel
                    Parallel.ForEach(node.Children, child =>
                    {
                        Build(child, mesh, depth + 1);
                    });
                    break;
            }
        }

        public void Collect(OctreeNode node, ConcurrentBag<Voxel> voxels)
        {
            //if (node.IsLeaf && node.IsOccupied)
            //{
            //    voxels.Add(new Voxel(node.Bounds.Center, GetVoxelSizeAtDepth(node.Depth), node.State));
            //    return;
            //}

            if (node.IsLeaf)
            {
                voxels.Add(new Voxel(node.Bounds.Center, GetVoxelSizeAtDepth(node.Depth), node.State));
                return;
            }

            if (node.Children == null) return;

            // Collect child nodes in parallel
            Parallel.ForEach(node.Children, child =>
            {
                Collect(child, voxels);
            });
        }

        public PVector3d GetVoxelSizeAtDepth(int depth)
        {
            // Voxel size = root size / (2^depth) for each dimension
            return _rootSize / (float)Math.Pow(2, depth);
        }
    }
}