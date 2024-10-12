using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Vox.Core.DataModels;

namespace Vox.Core.Meshing
{
    internal abstract class BaseMesher
    {
        public abstract PMesh GenerateMesh(List<PVector3d> positions, List<PVector3d> voxelSizes);

        /// <summary>
        /// Directions: Front, Back, Left, Right, Top, Bottom
        /// </summary>
        protected static readonly PVector3d[] Directions = new PVector3d[]
        {
            new PVector3d(1, 0, 0),  // Front (X+)
            new PVector3d(-1, 0, 0), // Back (X-)
            new PVector3d(0, -1, 0), // Left (Y-)
            new PVector3d(0, 1, 0),  // Right (Y+)
            new PVector3d(0, 0, 1),  // Top (Z+)
            new PVector3d(0, 0, -1)  // Bottom (Z-)
        };

        protected PMesh MakeCube(PVector3d position, PVector3d voxelSize)
        {
            List<PVector3d> vertices = new List<PVector3d>();
            List<int[]> triangles = new List<int[]>();

            foreach (var direction in Directions)
            {
                MakeFace(position, direction, vertices, triangles, voxelSize);
            }

            // Validate that faces and vertices were generated
            if (vertices.Count == 0 || triangles.Count == 0)
            {
                throw new InvalidOperationException("Vertices or faces failed to generate.");
            }

            return new PMesh(vertices, triangles);
        }

        protected void MakeFace(PVector3d position, PVector3d direction, List<PVector3d> vertices,
            List<int[]> triangles, PVector3d voxelSize)
        {
            int vertexIndex = vertices.Count;
            PVector3d[] faceVertices = MakeFaceVertices(position, direction, voxelSize);
            vertices.AddRange(faceVertices);

            // Determine the correct winding order based on the direction vector
            if (direction.X != 0)
            {
                // For X faces (Front and Back), use the original winding order for positive X and reverse for negative X
                if (direction.X > 0)
                {
                    triangles.Add(new int[] { vertexIndex, vertexIndex + 1, vertexIndex + 2 });
                    triangles.Add(new int[] { vertexIndex, vertexIndex + 2, vertexIndex + 3 });
                }
                else
                {
                    triangles.Add(new int[] { vertexIndex, vertexIndex + 2, vertexIndex + 1 });
                    triangles.Add(new int[] { vertexIndex, vertexIndex + 3, vertexIndex + 2 });
                }
            }
            else if (direction.Y != 0)
            {
                // For Y faces (Top and Bottom), use the original winding order for positive Y and reverse for negative Y
                if (direction.Y > 0)
                {
                    // Bottom face (Y-)
                    triangles.Add(new int[] { vertexIndex, vertexIndex + 2, vertexIndex + 1 });
                    triangles.Add(new int[] { vertexIndex, vertexIndex + 3, vertexIndex + 2 });
                    
                }
                else
                {
                    // Top face (Y+)
                    triangles.Add(new int[] { vertexIndex, vertexIndex + 1, vertexIndex + 2 });
                    triangles.Add(new int[] { vertexIndex, vertexIndex + 2, vertexIndex + 3 });
                }
            }
            else if (direction.Z != 0)
            {
                // For Z faces (Top and Bottom), use the original winding order for positive Z and reverse for negative Z
                if (direction.Z > 0)
                {
                    triangles.Add(new int[] { vertexIndex, vertexIndex + 1, vertexIndex + 2 });
                    triangles.Add(new int[] { vertexIndex, vertexIndex + 2, vertexIndex + 3 });
                }
                else
                {
                    triangles.Add(new int[] { vertexIndex, vertexIndex + 2, vertexIndex + 1 });
                    triangles.Add(new int[] { vertexIndex, vertexIndex + 3, vertexIndex + 2 });
                }
            }
        }


        private PVector3d[] MakeFaceVertices(PVector3d position, PVector3d direction, PVector3d voxelSize)
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
    }
}
