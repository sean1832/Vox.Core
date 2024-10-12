using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Vox.Core.Algorithms.Collision;
using Vox.Core.Algorithms.SpatialHashing;
using Vox.Core.DataModels;

namespace Vox.Core.Voxelization
{
    internal class SHVoxelizer
    {
        private readonly PVector3d _voxelSize;
        private readonly ConcurrentDictionary<(int, int, int), bool> _voxelGrid;
        private readonly int _parallelThreshold;

        public SHVoxelizer(PVector3d voxelSize, int parallelThreshold = 2048)
        {
            _voxelSize = voxelSize;
            _voxelGrid = new ConcurrentDictionary<(int, int, int), bool>();
            _parallelThreshold = parallelThreshold;
        }

        public List<Voxel> Voxelize(PMesh mesh)
        {
            List<Voxel> voxels = new List<Voxel>();

            if (mesh.Faces.Count > _parallelThreshold)
            {
                Parallel.ForEach(mesh.Faces, (face) =>
                {
                    VoxelizeFace(mesh, face);
                });
            }
            else
            {
                foreach (var face in mesh.Faces)
                {
                    VoxelizeFace(mesh, face);
                }
            }

            foreach ((int, int, int) voxelKey in _voxelGrid.Keys)
            {
                // Compute the voxel's world position
                PVector3d voxelPosition = new PVector3d(
                    voxelKey.Item1 * _voxelSize.X, 
                    voxelKey.Item2 * _voxelSize.Y, 
                    voxelKey.Item3 * _voxelSize.Z);
                voxels.Add(new Voxel(voxelPosition, _voxelSize, VoxelState.Intersecting));
            }

            return voxels;
        }

        private void VoxelizeFace(PMesh mesh, int[] face)
        {
            // Get the triangle's vertices
            var v0 = mesh.Vertices[face[0]];
            var v1 = mesh.Vertices[face[1]];
            var v2 = mesh.Vertices[face[2]];

            // Compute the AABB of the triangle
            PBoundingBox triangleBounds = new PBoundingBox();
            triangleBounds.Expand(v0);
            triangleBounds.Expand(v1);
            triangleBounds.Expand(v2);

            // Convert AABB to voxel space
            (int minX, int minY, int minZ) = WorldToVoxel(triangleBounds.Min);
            (int maxX, int maxY, int maxZ) = WorldToVoxel(triangleBounds.Max);

            // Iterate over all voxels in the AABB
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        // Check if the voxel intersects the triangle
                        if (VoxelIntersectsTriangle(x, y, z, v0, v1, v2))
                        {
                            var voxelKey = (x, y, z);
                            _voxelGrid.TryAdd(voxelKey, true);  // Thread-safe add
                        }
                    }
                }
            }
        }

        // Convert world coordinates to voxel grid coordinates
        private (int, int, int) WorldToVoxel(PVector3d point)
        {
            // Add small tolerance to avoid floating point issues
            const double tolerance = 1e-6;
            int x = (int)Math.Floor((point.X + tolerance) / _voxelSize.X);
            int y = (int)Math.Floor((point.Y + tolerance) / _voxelSize.Y);
            int z = (int)Math.Floor((point.Z + tolerance) / _voxelSize.Z);
            return (x, y, z);
        }

        private bool VoxelIntersectsTriangle(int x, int y, int z, PVector3d v0, PVector3d v1, PVector3d v2)
        {
            // Compute the voxel's AABB in world space
            PBoundingBox voxelBounds = new PBoundingBox(
                new PVector3d(x * _voxelSize.X, y * _voxelSize.Y, z * _voxelSize.Z),
                new PVector3d((x + 1) * _voxelSize.X, (y + 1) * _voxelSize.Y, (z + 1) * _voxelSize.Z)
            );

            // Use your existing Triangle-AABB intersection test
            return AABB.TriangleIntersectsAABB(v0, v1, v2, voxelBounds);
        }
    }
}
