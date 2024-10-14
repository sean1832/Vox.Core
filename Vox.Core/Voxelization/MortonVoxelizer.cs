using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vox.Core.Algorithms.Collision;
using Vox.Core.Algorithms.MortonCode;
using Vox.Core.DataModels;

namespace Vox.Core.Voxelization
{
    internal class MortonVoxelizer
    {
        private readonly PVector3d _voxelSize;
        private readonly ConcurrentDictionary<ulong, bool> _voxelGrid;
        private readonly int _parallelThreshold;
        private Morton3D _morton;
        private PMesh _mesh;

        public MortonVoxelizer(PVector3d voxelSize, PMesh mesh, int parallelThreshold = 2048)
        {
            _voxelSize = voxelSize;
            _voxelGrid = new ConcurrentDictionary<ulong, bool>();
            _parallelThreshold = parallelThreshold;
            _mesh = mesh;

            int minX = int.MaxValue, minY = int.MaxValue, minZ = int.MaxValue;

            foreach (var vertex in mesh.Vertices)
            {
                var (x, y, z) = WorldToVoxel(vertex);
                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (z < minZ) minZ = z;
            }

            _morton = new Morton3D(minX, minY, minZ);
        }

        public List<Voxel> Voxelize()
        {
            List<Voxel> voxels = new List<Voxel>();

            if (_mesh.Faces.Count > _parallelThreshold)
            {
                Parallel.ForEach(_mesh.Faces, (face) =>
                {
                    VoxelizeFace(_mesh, face);
                });
            }
            else
            {
                foreach (var face in _mesh.Faces)
                {
                    VoxelizeFace(_mesh, face);
                }
            }

            foreach (ulong voxelKey in _voxelGrid.Keys)
            {
                // Compute the voxel's world position
                (int x, int y, int z) = _morton.Decode(voxelKey);
                PVector3d voxelPosition = new PVector3d(
                    x * _voxelSize.X,
                    y * _voxelSize.Y,
                    z * _voxelSize.Z);
                voxels.Add(new Voxel(voxelPosition, _voxelSize, VoxelState.Intersecting));
            }

            return voxels;
        }

        private void VoxelizeFace(PMesh mesh, int[] face)
        {
            // get the triangle's vertices
            var v0 = mesh.Vertices[face[0]];
            var v1 = mesh.Vertices[face[1]];
            var v2 = mesh.Vertices[face[2]];

            // compute the AABB of the triangle
            PBoundingBox triangleBounds = new PBoundingBox();
            triangleBounds.Expand(v0);
            triangleBounds.Expand(v1);
            triangleBounds.Expand(v2);

            // Convert AABB to voxel space
            (int minX, int minY, int minZ) = WorldToVoxel(triangleBounds.Min);
            (int maxX, int maxY, int maxZ) = WorldToVoxel(triangleBounds.Max);

            // iterate over all voxels in the AABB
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        // Check if the voxel intersects the triangle
                        if (VoxelIntersectsTriangle(x, y, z, v0, v1, v2))
                        {
                            // Compute the Morton code
                            ulong mortonCode = _morton.Encode(x, y, z);
                            _voxelGrid.TryAdd(mortonCode, true);
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

        public static bool TryAdd<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
                return true;
            }

            return false;
        }
    }
}
