using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Vox.Core.Algorithms.BoundingVolumeHierarchy;
using Vox.Core.Algorithms.SparseVoxelOctree;
using Vox.Core.DataModels;
using Vox.Core.Voxelization;

namespace Vox.Core
{
    public static class Voxelizer
    {

        /// <summary>
        /// Voxelize the mesh using the Sparse Voxel Octree algorithm
        /// </summary>
        /// <param name="mesh">Mesh to voxelize</param>
        /// <param name="maxDepth">Maximum subdivision level (resolution)</param>
        /// <param name="isSolid">Infill interior of a mesh</param>
        /// <param name="voxelScale">Scale factor for each voxel</param>
        /// <returns>Voxels</returns>
        public static List<Voxel> VoxelizeSVO(PMesh mesh, int maxDepth, bool isSolid, PVector3d voxelScale)
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

        /// <summary>
        /// Voxelize the mesh using the Sparse Voxel Octree algorithm with precomputed BVH data
        /// </summary>
        /// <param name="bvh">Precomputed BVH data</param>
        /// <param name="maxDepth">Maximum subdivision level (resolution)</param>
        /// <param name="isSolid">Infill interior of a mesh</param>
        /// <param name="voxelScale">Scale factor for each voxel</param>
        /// <returns>Voxels</returns>
        public static List<Voxel> VoxelizeSVO(BVH bvh, int maxDepth, bool isSolid, PVector3d voxelScale)
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

        public static List<Voxel> VoxelizeSH(PMesh mesh, PVector3d voxelSize)
        {
            SHVoxelizer voxelizer = new SHVoxelizer(voxelSize);
            return voxelizer.Voxelize(mesh);
        }


        /// <summary>
        /// Voxelize the mesh using the Signed Distance Field algorithm
        /// </summary>
        /// <param name="mesh">Mesh to voxelize</param>
        /// <param name="gridSize">Voxel field size</param>
        /// <returns>Voxels</returns>
        public static List<Voxel> VoxelizeSDF(PMesh mesh, PVector3d gridSize)
        {
            PBoundingBox bBox = mesh.GetBoundingBox().ToCubic();
            mesh.ComputeTriangleBounds(); // precompute triangle bounds

            OctreeNode rootNode = new OctreeNode(bBox);
            SVO svo = new SVO(3, rootNode.Bounds.Size, false);
            svo.Build(rootNode, mesh);

            ConcurrentBag<Voxel> voxels = new ConcurrentBag<Voxel>();
            svo.Collect(rootNode, voxels); // mesh proximity volume
            throw new NotImplementedException();
            return voxels.ToList();
        }
    }
}
