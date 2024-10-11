using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Vox.Core.Algorithms.BVH;
using Vox.Core.Algorithms.SVO;
using Vox.Core.DataModels;

namespace Vox.Core
{
    public class Voxelizer
    {
        public Voxelizer()
        {
        }

        /// <summary>
        /// Voxelize the mesh using the Sparse Voxel Octree algorithm
        /// </summary>
        /// <param name="mesh">Mesh to voxelize</param>
        /// <param name="maxDepth">Maximum subdivision level (resolution)</param>
        /// <param name="isSolid">Infill interior of a mesh</param>
        /// <param name="voxelScale">Scale factor for each voxel</param>
        /// <returns>Voxels</returns>
        public List<Voxel> VoxelizeSVO(PMesh mesh, int maxDepth, bool isSolid, PVector3d? voxelScale = null)
        {
            voxelScale ??= new PVector3d(1, 1, 1);
            mesh.ComputeTriangleBounds(); // Precompute triangle bounds
            PBoundingBox bBox = mesh.GetBoundingBox().ToScale(voxelScale);
            OctreeNode rootNode = new OctreeNode(bBox);
            SparseVoxelOctree svo = new SparseVoxelOctree(maxDepth, rootNode.Bounds.Size, isSolid);
            svo.Build(rootNode, mesh);

            ConcurrentBag<Voxel> voxels = new ConcurrentBag<Voxel>();
            svo.Collect(rootNode, voxels);
            return voxels.ToList();
        }

        /// <summary>
        /// Voxelize the mesh using the Sparse Voxel Octree algorithm with precomputed BVH data
        /// </summary>
        /// <param name="bvh">Precomputed BVH data</param>
        /// <param name="maxDepth">Maximum subdivision level (resolution)</param>
        /// <param name="isSolid">Infill interior of a mesh</param>
        /// <param name="voxelScale">Scale factor for each voxel</param>
        /// <returns>Voxels</returns>
        public List<Voxel> VoxelizeSVO(BoundingVolumeHierarchy bvh, int maxDepth, bool isSolid, PVector3d? voxelScale = null)
        {
            voxelScale ??= new PVector3d(1, 1, 1);

            // Calculate the mesh's bounding box, use cubic bounding box as the root node
            PBoundingBox bBox = bvh.Mesh.GetBoundingBox().ToScale(voxelScale);

            OctreeNode rootNode = new OctreeNode(bBox);
            SparseVoxelOctree svo = new SparseVoxelOctree(maxDepth, rootNode.Bounds.Size, isSolid, bvh);
            svo.Build(rootNode, bvh.Mesh);

            ConcurrentBag<Voxel> voxels = new ConcurrentBag<Voxel>();
            svo.Collect(rootNode, voxels);

            return voxels.ToList();
        }


        /// <summary>
        /// Voxelize the mesh using the Signed Distance Field algorithm
        /// </summary>
        /// <param name="mesh">Mesh to voxelize</param>
        /// <param name="gridSize">Voxel field size</param>
        /// <returns>Voxels</returns>
        public List<Voxel> VoxelizeSDF(PMesh mesh, PVector3d gridSize)
        {
            PBoundingBox bBox = mesh.GetBoundingBox().ToCubic();
            mesh.ComputeTriangleBounds(); // precompute triangle bounds

            OctreeNode rootNode = new OctreeNode(bBox);
            SparseVoxelOctree svo = new SparseVoxelOctree(3, rootNode.Bounds.Size, false);
            svo.Build(rootNode, mesh);

            ConcurrentBag<Voxel> voxels = new ConcurrentBag<Voxel>();
            svo.Collect(rootNode, voxels); // mesh proximity volume
            throw new NotImplementedException();
            return voxels.ToList();
        }
    }
}
