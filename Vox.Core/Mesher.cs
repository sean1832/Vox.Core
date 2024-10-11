using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using Vox.Core.DataModels;

namespace Vox.Core
{
    public class Mesher
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
            List<PVector3d> vertices = new List<PVector3d>();
            List<int[]> faces = new List<int[]>();


            var tolerance = voxels.First().Size.MinComponent() * 0.001f;
            // Use a HashSet for quick voxel position lookups (O(1) average time complexity)
            HashSet<PVector3d> positionSet = new HashSet<PVector3d>(voxels.Select(v => v.Position), new PVector3dEqualityComparer(tolerance));


            // Iterate over each voxel
            foreach (var voxel in voxels)
            {
                // Check all six directions
                foreach (var direction in Directions)
                {
                    // Element-wise multiplication of direction and voxel size
                    PVector3d neighborOffset = direction * voxel.Size;
                    PVector3d neighborPosition = voxel.Position + neighborOffset;


                    // Use HashSet to check if a neighbor exists
                    if (!positionSet.Contains(neighborPosition))
                    {
                        // Generate face if no neighbor is found
                        GenerateFace(voxel.Position, direction, vertices, faces, voxel.Size);
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


        public PMesh Generate(List<PVector3d> positions, List<PVector3d> voxelSizes)
        {
            float tolerance = voxelSizes.First().MinComponent() * 0.001f;
            HashSet<PVector3d> positionSet = new HashSet<PVector3d>(positions , new PVector3dEqualityComparer(tolerance));

            List<PVector3d> vertices = new List<PVector3d>();
            List<int[]> faces = new List<int[]>();

            for (int i = 0; i < positions.Count; i++)
            {
                var position = positions[i];
                var voxelSize = voxelSizes[i];
                foreach (var direction in Directions)
                {
                    // Element-wise multiplication of direction and voxel size
                    PVector3d neighborOffset = direction * voxelSize;
                    PVector3d neighborPosition = position + neighborOffset;

                    if (!positionSet.Contains(neighborPosition))
                    {
                        // no neighbor at this position, generate a face
                        GenerateFace(position, direction, vertices, faces, voxelSize);
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

        private void GenerateFace(PVector3d position, PVector3d direction, List<PVector3d> vertices,
            List<int[]> triangles, PVector3d voxelSize)
        {
            int vertexIndex = vertices.Count;
            PVector3d[] faceVertices = GenerateFaceVertices(position, direction, voxelSize);
            vertices.AddRange(faceVertices);

            // Determine the correct winding order based on the face normal
            bool reverse = (direction.X + direction.Y + direction.Z) < 0;

            if (reverse)
            {
                // For negative directions, reverse the vertex order
                triangles.Add(new int[] { vertexIndex, vertexIndex + 2, vertexIndex + 1 });
                triangles.Add(new int[] { vertexIndex, vertexIndex + 3, vertexIndex + 2 });
            }
            else
            {
                // For positive directions, keep the original order
                triangles.Add(new int[] { vertexIndex, vertexIndex + 1, vertexIndex + 2 });
                triangles.Add(new int[] { vertexIndex, vertexIndex + 2, vertexIndex + 3 });
            }
        }


        private PVector3d[] GenerateFaceVertices(PVector3d position, PVector3d direction, PVector3d voxelSize)
        {
            PVector3d[] faceVertices = new PVector3d[4];
            PVector3d halfSize = voxelSize * 0.5f;

            // depending on the direction, calculate the 4 corner vertices of the face
            if (direction.X != 0) // X faces (Front, Back)
            {
                float sign = direction.X;
                faceVertices[0] = position + new PVector3d(sign * halfSize.X, -halfSize.Y, -halfSize.Z);
                faceVertices[1] = position + new PVector3d(sign * halfSize.X, halfSize.Y, -halfSize.Z);
                faceVertices[2] = position + new PVector3d(sign * halfSize.X, halfSize.Y, halfSize.Z);
                faceVertices[3] = position + new PVector3d(sign * halfSize.X, -halfSize.Y, halfSize.Z);
            }
            else if (direction.Y != 0) // Y faces (Left, Right)
            {
                float sign = direction.Y;
                faceVertices[0] = position + new PVector3d(-halfSize.X, sign * halfSize.Y, -halfSize.Z);
                faceVertices[1] = position + new PVector3d(halfSize.X, sign * halfSize.Y, -halfSize.Z);
                faceVertices[2] = position + new PVector3d(halfSize.X, sign * halfSize.Y, halfSize.Z);
                faceVertices[3] = position + new PVector3d(-halfSize.X, sign * halfSize.Y, halfSize.Z);
            }
            else if (direction.Z != 0) // Z faces (Top, Bottom)
            {
                float sign = direction.Z;
                faceVertices[0] = position + new PVector3d(-halfSize.X, -halfSize.Y, sign * halfSize.Z);
                faceVertices[1] = position + new PVector3d(halfSize.X, -halfSize.Y, sign * halfSize.Z);
                faceVertices[2] = position + new PVector3d(halfSize.X, halfSize.Y, sign * halfSize.Z);
                faceVertices[3] = position + new PVector3d(-halfSize.X, halfSize.Y, sign * halfSize.Z);
            }

            return faceVertices;
        }

        public class PVector3dEqualityComparer : IEqualityComparer<PVector3d>
        {
            private readonly float _tolerance;

            public PVector3dEqualityComparer(float tolerance)
            {
                _tolerance = tolerance;
            }

            public bool Equals(PVector3d a, PVector3d b)
            {
                if (ReferenceEquals(a, b))
                    return true;
                if (a is null || b is null)
                    return false;

                return Math.Abs(a.X - b.X) < _tolerance &&
                       Math.Abs(a.Y - b.Y) < _tolerance &&
                       Math.Abs(a.Z - b.Z) < _tolerance;
            }

            public int GetHashCode(PVector3d obj)
            {
                int xInt = (int)Math.Round(obj.X / _tolerance);
                int yInt = (int)Math.Round(obj.Y / _tolerance);
                int zInt = (int)Math.Round(obj.Z / _tolerance);

                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + xInt;
                    hash = hash * 23 + yInt;
                    hash = hash * 23 + zInt;
                    return hash;
                }
            }
        }

    }
}
