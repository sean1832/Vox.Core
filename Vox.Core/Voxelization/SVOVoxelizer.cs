using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vox.Core.Algorithms.BoundingVolumeHierarchy;
using Vox.Core.Algorithms.SparseVoxelOctree;
using Vox.Core.DataModels;

namespace Vox.Core.Voxelization
{
    internal class SVOVoxelizer
    {
        public static List<Voxel> Voxelize(PMesh mesh, int maxDepth, bool isSolid, PVector3d voxelScale)
        {
            mesh.ComputeTriangleBounds(); // Precompute triangle bounds
            PBoundingBox bBox = mesh.GetBoundingBox().ToScale(voxelScale);
            OctreeNode rootNode = new OctreeNode(bBox);
            SVO svo = new SVO(maxDepth, rootNode.Bounds.Size, isSolid);
            svo.Build(rootNode, mesh);

            ConcurrentBag<Voxel> voxels = new ConcurrentBag<Voxel>();
            svo.Collect(rootNode, voxels);
            return voxels.ToList();
        }

        public static List<Voxel> Voxelize(BVH bvh, int maxDepth, bool isSolid, PVector3d voxelScale)
        {
            // Calculate the mesh's bounding box, use cubic bounding box as the root node
            PBoundingBox bBox = bvh.Mesh.GetBoundingBox().ToScale(voxelScale);

            OctreeNode rootNode = new OctreeNode(bBox);
            SVO svo = new SVO(maxDepth, rootNode.Bounds.Size, isSolid, bvh);
            svo.Build(rootNode, bvh.Mesh);

            ConcurrentBag<Voxel> voxels = new ConcurrentBag<Voxel>();
            svo.Collect(rootNode, voxels);

            return voxels.ToList();
        }
    }
}
