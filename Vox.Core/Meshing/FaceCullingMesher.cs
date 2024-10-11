using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vox.Core.Algorithms.SpatialHashing;
using Vox.Core.DataModels;

namespace Vox.Core.Meshing
{
    internal class FaceCullingMesher: BaseMesher
    {
        // Directions: Front, Back, Left, Right, Top, Bottom
        private static readonly PVector3d[] Directions = new PVector3d[]
        {
            new PVector3d(1, 0, 0),  // Front (X+)
            new PVector3d(-1, 0, 0), // Back (X-)
            new PVector3d(0, -1, 0), // Left (Y-)
            new PVector3d(0, 1, 0),  // Right (Y+)
            new PVector3d(0, 0, 1),  // Top (Z+)
            new PVector3d(0, 0, -1)  // Bottom (Z-)
        };

        public PMesh Generate(List<Voxel> voxels)
        {
            float cellSize = voxels.First().Size.Min(); // Get the minimum size of the voxels
            // Initialize the spatial hasher
            var hasher = new SpatialHasher(cellSize);
            var spatialHash = new Dictionary<int, List<Voxel>>();

            // Populate the spatial hash with voxels
            foreach (var voxel in voxels)
            {
                int hash = hasher.Hash(voxel.Position);
                if (!spatialHash.ContainsKey(hash))
                {
                    spatialHash[hash] = new List<Voxel>();
                }
                spatialHash[hash].Add(voxel);
            }

            List<PVector3d> vertices = new List<PVector3d>();
            List<int[]> faces = new List<int[]>();

            // iterate over each voxel, generate faces if no neighbor exists
            foreach (var voxel in voxels)
            {
                foreach (var direction in Directions)
                {
                    PVector3d neighborOffset = direction * voxel.Size;
                    PVector3d neighborPosition = voxel.Position + neighborOffset;

                    int neighborHash = hasher.Hash(neighborPosition);

                    // check if there is a voxel in neighbor cell
                    bool neighborExist = false;
                    if (spatialHash.ContainsKey(neighborHash))
                    {
                        foreach (var neighbor in spatialHash[neighborHash])
                        {
                            // Check for exact adjacency based on voxel size and direction
                            if (IsAdjacent(voxel.Position, voxel.Size, neighbor.Position, neighbor.Size, direction))
                            {
                                neighborExist = true;
                                break;
                            }
                        }
                    }

                    if (!neighborExist)
                    {
                        GenerateFace(voxel.Position, direction, vertices, faces, voxel.Size);
                    }
                }
            }

            if (vertices.Count <= 0 || faces.Count <= 0)
            {
                throw new InvalidOperationException("Vertices or Triangle failed to generate.");
            }

            PMesh mesh = new PMesh(vertices, faces);
            return mesh;
        }


        public override PMesh GenerateMesh(List<PVector3d> positions, List<PVector3d> voxelSizes)
        {
            if (positions.Count != voxelSizes.Count)
            {
                throw new ArgumentException("Positions and voxelSizes must have the same length.");
            }
            float cellSize = voxelSizes.First().Min(); // Get the minimum size of the voxels
            // Initialize the spatial hasher
            var hasher = new SpatialHasher(cellSize);
            var spatialHash = new Dictionary<int, List<(PVector3d, PVector3d)>>();

            // Populate the spatial hash with positions and voxelSizes
            for (int i = 0; i < positions.Count; i++)
            {
                var position = positions[i];
                var voxelSize = voxelSizes[i];
                int hash = hasher.Hash(position);

                if (!spatialHash.ContainsKey(hash))
                {
                    spatialHash[hash] = new List<(PVector3d, PVector3d)>();
                }
                spatialHash[hash].Add((position, voxelSize));
            }

            List<PVector3d> vertices = new List<PVector3d>();
            List<int[]> faces = new List<int[]>();

            // Iterate over each voxel and generate faces if no neighbor is found
            for (int i = 0; i < positions.Count; i++)
            {
                var position = positions[i];
                var voxelSize = voxelSizes[i];

                foreach (var direction in Directions)
                {
                    // Element-wise multiplication of direction and voxel size
                    PVector3d neighborOffset = direction * voxelSize;
                    PVector3d neighborPosition = position + neighborOffset;

                    int neighborHash = hasher.Hash(neighborPosition);

                    // Check if there's a voxel in the neighboring cell
                    bool neighborExists = false;
                    if (spatialHash.ContainsKey(neighborHash))
                    {
                        // Check if any voxel in the neighboring cell actually overlaps
                        foreach (var (neighborPos, neighborSize) in spatialHash[neighborHash])
                        {
                            // Check for exact adjacency based on voxel size and direction
                            if (IsAdjacent(position, voxelSize, neighborPos, neighborSize, direction))
                            {
                                neighborExists = true;
                                break;
                            }
                        }
                    }

                    // If no neighbor is found, generate the face
                    if (!neighborExists)
                    {
                        GenerateFace(position, direction, vertices, faces, voxelSize);
                    }
                }
            }

            // Validate that faces and vertices were generated
            if (vertices.Count == 0 || faces.Count == 0)
            {
                throw new InvalidOperationException("Vertices or faces failed to generate.");
            }

            // Return the final mesh
            return new PMesh(vertices, faces);
        }

        private bool IsAdjacent(PVector3d pos1, PVector3d size1, PVector3d pos2, PVector3d size2, PVector3d direction)
        {
            // Check for adjacency along the axis of the direction vector
            if (direction.X != 0)
            {
                return Math.Abs(pos1.X + direction.X * size1.X - pos2.X) < size1.X + size2.X
                       && Math.Abs(pos1.Y - pos2.Y) < (size1.Y + size2.Y) * 0.5
                       && Math.Abs(pos1.Z - pos2.Z) < (size1.Z + size2.Z) * 0.5;
            }
            else if (direction.Y != 0)
            {
                return Math.Abs(pos1.Y + direction.Y * size1.Y - pos2.Y) < size1.Y + size2.Y
                       && Math.Abs(pos1.X - pos2.X) < (size1.X + size2.X) * 0.5
                       && Math.Abs(pos1.Z - pos2.Z) < (size1.Z + size2.Z) * 0.5;
            }
            else if (direction.Z != 0)
            {
                return Math.Abs(pos1.Z + direction.Z * size1.Z - pos2.Z) < size1.Z + size2.Z
                       && Math.Abs(pos1.X - pos2.X) < (size1.X + size2.X) * 0.5
                       && Math.Abs(pos1.Y - pos2.Y) < (size1.Y + size2.Y) * 0.5;
            }
            return false;
        }
    }
}
