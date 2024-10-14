using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Vox.Core.DataModels;

namespace Vox.Core.Meshing
{
    public enum CoordinateSystem
    {
        LeftHanded,
        RightHanded
    }

    internal abstract class BaseMesher
    {
        public abstract PMesh GenerateMesh(List<PVector3d> positions, List<PVector3d> voxelSizes, CoordinateSystem coordinateSystem = CoordinateSystem.RightHanded);

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

        protected PMesh MakeCube(PVector3d position, PVector3d voxelSize, CoordinateSystem coordinateSystem)
        {
            List<PVector3d> vertices = new List<PVector3d>();
            List<int[]> triangles = new List<int[]>();

            foreach (var direction in Directions)
            {
                MakeFace(position, direction, vertices, triangles, voxelSize, coordinateSystem);
            }

            // Validate that faces and vertices were generated
            if (vertices.Count == 0 || triangles.Count == 0)
            {
                throw new InvalidOperationException("Vertices or faces failed to generate.");
            }

            return new PMesh(vertices, triangles);
        }

        protected void MakeFace(PVector3d position, PVector3d direction, List<PVector3d> vertices,
            List<int[]> faces, PVector3d voxelSize, CoordinateSystem coordinateSystem, bool isQuad = false)
        {
            int vertexIndex = vertices.Count;
            PVector3d[] faceVertices = MakeFaceVertices(position, direction, voxelSize, coordinateSystem);
            vertices.AddRange(faceVertices);

            if (isQuad)
            {
                faces.Add(new int[] { vertexIndex, vertexIndex + 1, vertexIndex + 2, vertexIndex + 3 });
            }
            else
            {
                faces.Add(new int[] { vertexIndex, vertexIndex + 1, vertexIndex + 2 });
                faces.Add(new int[] { vertexIndex, vertexIndex + 2, vertexIndex + 3 });
            }
        }

        private PVector3d[] MakeFaceVertices(PVector3d position, PVector3d direction, PVector3d voxelSize, CoordinateSystem coordinateSystem)
        {
            PVector3d[] faceVertices = new PVector3d[4];
            PVector3d halfSize = voxelSize * 0.5f;

            if (direction.X > 0) // Front face (X+)
            {
                faceVertices[0] = position + new PVector3d(halfSize.X, -halfSize.Y, -halfSize.Z);
                faceVertices[1] = position + new PVector3d(halfSize.X, halfSize.Y, -halfSize.Z);
                faceVertices[2] = position + new PVector3d(halfSize.X, halfSize.Y, halfSize.Z);
                faceVertices[3] = position + new PVector3d(halfSize.X, -halfSize.Y, halfSize.Z);
            }
            else if (direction.X < 0) // Back face (X-)
            {
                faceVertices[0] = position + new PVector3d(-halfSize.X, -halfSize.Y, halfSize.Z);
                faceVertices[1] = position + new PVector3d(-halfSize.X, halfSize.Y, halfSize.Z);
                faceVertices[2] = position + new PVector3d(-halfSize.X, halfSize.Y, -halfSize.Z);
                faceVertices[3] = position + new PVector3d(-halfSize.X, -halfSize.Y, -halfSize.Z);
            }
            else if (direction.Y > 0) // Right face (Y+)
            {
                if (coordinateSystem == CoordinateSystem.RightHanded)
                {
                    faceVertices[0] = position + new PVector3d(-halfSize.X, halfSize.Y, -halfSize.Z);
                    faceVertices[1] = position + new PVector3d(halfSize.X, halfSize.Y, -halfSize.Z);
                    faceVertices[2] = position + new PVector3d(halfSize.X, halfSize.Y, halfSize.Z);
                    faceVertices[3] = position + new PVector3d(-halfSize.X, halfSize.Y, halfSize.Z);
                }
                else
                {
                    // (Rhino uses left-handed coordinate system)
                    // Reversed winding here to fix the Y+ face normal 
                    faceVertices[0] = position + new PVector3d(halfSize.X, halfSize.Y, -halfSize.Z);
                    faceVertices[1] = position + new PVector3d(-halfSize.X, halfSize.Y, -halfSize.Z);
                    faceVertices[2] = position + new PVector3d(-halfSize.X, halfSize.Y, halfSize.Z);
                    faceVertices[3] = position + new PVector3d(halfSize.X, halfSize.Y, halfSize.Z);
                }
                
            }
            else if (direction.Y < 0) // Left face (Y-)
            {
                if (coordinateSystem == CoordinateSystem.RightHanded)
                {
                    faceVertices[0] = position + new PVector3d(halfSize.X, -halfSize.Y, -halfSize.Z);
                    faceVertices[1] = position + new PVector3d(-halfSize.X, -halfSize.Y, -halfSize.Z);
                    faceVertices[2] = position + new PVector3d(-halfSize.X, -halfSize.Y, halfSize.Z);
                    faceVertices[3] = position + new PVector3d(halfSize.X, -halfSize.Y, halfSize.Z);
                }
                else
                {
                    // Correct winding for Y- face normal
                    faceVertices[0] = position + new PVector3d(-halfSize.X, -halfSize.Y, -halfSize.Z);
                    faceVertices[1] = position + new PVector3d(halfSize.X, -halfSize.Y, -halfSize.Z);
                    faceVertices[2] = position + new PVector3d(halfSize.X, -halfSize.Y, halfSize.Z);
                    faceVertices[3] = position + new PVector3d(-halfSize.X, -halfSize.Y, halfSize.Z);
                }
                
            }
            else if (direction.Z > 0) // Top face (Z+)
            {
                faceVertices[0] = position + new PVector3d(-halfSize.X, -halfSize.Y, halfSize.Z);
                faceVertices[1] = position + new PVector3d(halfSize.X, -halfSize.Y, halfSize.Z);
                faceVertices[2] = position + new PVector3d(halfSize.X, halfSize.Y, halfSize.Z);
                faceVertices[3] = position + new PVector3d(-halfSize.X, halfSize.Y, halfSize.Z);
            }
            else if (direction.Z < 0) // Bottom face (Z-)
            {
                faceVertices[0] = position + new PVector3d(-halfSize.X, halfSize.Y, -halfSize.Z);
                faceVertices[1] = position + new PVector3d(halfSize.X, halfSize.Y, -halfSize.Z);
                faceVertices[2] = position + new PVector3d(halfSize.X, -halfSize.Y, -halfSize.Z);
                faceVertices[3] = position + new PVector3d(-halfSize.X, -halfSize.Y, -halfSize.Z);
            }

            return faceVertices;
        }

    }
}
