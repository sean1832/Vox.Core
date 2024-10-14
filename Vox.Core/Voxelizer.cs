using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
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
        /// Voxelize the mesh using the Sparse Voxel Octree algorithm. Use this for static mesh.
        /// </summary>
        /// <param name="mesh">Mesh to voxelize</param>
        /// <param name="maxDepth">Maximum subdivision level (resolution)</param>
        /// <param name="isSolid">Infill interior of a mesh</param>
        /// <param name="voxelScale">Scale factor for each voxel</param>
        /// <returns>Voxels</returns>
        public static List<Voxel> VoxelizeSVO(PMesh mesh, int maxDepth, bool isSolid, PVector3d voxelScale)
        {
            return SVOVoxelizer.Voxelize(mesh, maxDepth, isSolid, voxelScale);
        }

        /// <summary>
        /// Voxelize the mesh using the Sparse Voxel Octree algorithm with precomputed BVH data. Use this for static mesh.
        /// </summary>
        /// <param name="bvh">Precomputed BVH data</param>
        /// <param name="maxDepth">Maximum subdivision level (resolution)</param>
        /// <param name="isSolid">Infill interior of a mesh</param>
        /// <param name="voxelScale">Scale factor for each voxel</param>
        /// <returns>Voxels</returns>
        public static List<Voxel> VoxelizeSVO(BVH bvh, int maxDepth, bool isSolid, PVector3d voxelScale)
        {
            return SVOVoxelizer.Voxelize(bvh, maxDepth, isSolid, voxelScale);
        }

        /// <summary>
        /// Voxelize the mesh using Spatial Hashing algorithm. Use this for mesh that update frequently.
        /// </summary>
        /// <param name="mesh">Mesh to voxelize</param>
        /// <param name="voxelSize">Size of each voxel</param>
        /// <returns>Voxels</returns>
        public static List<Voxel> VoxelizeSH(PMesh mesh, PVector3d voxelSize)
        {
            SHVoxelizer voxelizer = new SHVoxelizer(voxelSize);
            return voxelizer.Voxelize(mesh);
        }

        /// <summary>
        /// Voxelize the mesh using Morton Code algorithm. Use this for mesh that update frequently.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="voxelSize"></param>
        /// <returns></returns>
        public static List<Voxel> VoxelizeMorton(PMesh mesh, PVector3d voxelSize)
        {
            MortonVoxelizer voxelizer = new MortonVoxelizer(voxelSize, mesh);
            return voxelizer.Voxelize();
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
