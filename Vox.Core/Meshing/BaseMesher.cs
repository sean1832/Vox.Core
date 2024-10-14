using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Vox.Core.DataModels;

namespace Vox.Core.Meshing
{
    public enum CordSystem
    {
        /// <summary>
        /// OpenGL and DirectX use right-handed coordinate systems
        /// </summary>
        RightHanded,

        /// <summary>
        /// Rhino3D uses left-handed coordinate system
        /// </summary>
        LeftHanded
    }

    internal abstract class BaseMesher
    {
        protected CordSystem CordSystem;
        protected BaseMesher(CordSystem cordSystem)
        {
            CordSystem = cordSystem;
        }

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
            List<int[]> faces, PVector3d voxelSize, bool isQuad = false)
        {
            int vertexIndex = vertices.Count;
            PVector3d[] faceVertices = MakeFaceVertices(position, direction, voxelSize, CordSystem);
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

        private PVector3d[] MakeFaceVertices(PVector3d position, PVector3d direction, PVector3d voxelSize, CordSystem cordSystem)
        {
            PVector3d[] faceVertices = new PVector3d[4];
            PVector3d halfSize = voxelSize * 0.5f;

            // Switch based on direction vector
            switch (direction.X, direction.Y, direction.Z)
            {
                case { X: > 0 }: // Front face (X+)
                    faceVertices[0] = position + new PVector3d(halfSize.X, -halfSize.Y, -halfSize.Z);
                    faceVertices[1] = position + new PVector3d(halfSize.X, halfSize.Y, -halfSize.Z);
                    faceVertices[2] = position + new PVector3d(halfSize.X, halfSize.Y, halfSize.Z);
                    faceVertices[3] = position + new PVector3d(halfSize.X, -halfSize.Y, halfSize.Z);
                    break;

                case { X: < 0 }: // Back face (X-)
                    faceVertices[0] = position + new PVector3d(-halfSize.X, -halfSize.Y, halfSize.Z);
                    faceVertices[1] = position + new PVector3d(-halfSize.X, halfSize.Y, halfSize.Z);
                    faceVertices[2] = position + new PVector3d(-halfSize.X, halfSize.Y, -halfSize.Z);
                    faceVertices[3] = position + new PVector3d(-halfSize.X, -halfSize.Y, -halfSize.Z);
                    break;

                case { Y: > 0 }: // Right face (Y+)
                    if (cordSystem == CordSystem.RightHanded)
                    {
                        faceVertices[0] = position + new PVector3d(-halfSize.X, halfSize.Y, -halfSize.Z);
                        faceVertices[1] = position + new PVector3d(halfSize.X, halfSize.Y, -halfSize.Z);
                        faceVertices[2] = position + new PVector3d(halfSize.X, halfSize.Y, halfSize.Z);
                        faceVertices[3] = position + new PVector3d(-halfSize.X, halfSize.Y, halfSize.Z);
                    }
                    else
                    {
                        faceVertices[0] = position + new PVector3d(halfSize.X, halfSize.Y, -halfSize.Z);
                        faceVertices[1] = position + new PVector3d(-halfSize.X, halfSize.Y, -halfSize.Z);
                        faceVertices[2] = position + new PVector3d(-halfSize.X, halfSize.Y, halfSize.Z);
                        faceVertices[3] = position + new PVector3d(halfSize.X, halfSize.Y, halfSize.Z);
                    }
                    break;

                case { Y: < 0 }: // Left face (Y-)
                    if (cordSystem == CordSystem.RightHanded)
                    {
                        faceVertices[0] = position + new PVector3d(halfSize.X, -halfSize.Y, -halfSize.Z);
                        faceVertices[1] = position + new PVector3d(-halfSize.X, -halfSize.Y, -halfSize.Z);
                        faceVertices[2] = position + new PVector3d(-halfSize.X, -halfSize.Y, halfSize.Z);
                        faceVertices[3] = position + new PVector3d(halfSize.X, -halfSize.Y, halfSize.Z);
                    }
                    else
                    {
                        // Correct winding for left-handed system
                        faceVertices[0] = position + new PVector3d(-halfSize.X, -halfSize.Y, -halfSize.Z);
                        faceVertices[1] = position + new PVector3d(halfSize.X, -halfSize.Y, -halfSize.Z);
                        faceVertices[2] = position + new PVector3d(halfSize.X, -halfSize.Y, halfSize.Z);
                        faceVertices[3] = position + new PVector3d(-halfSize.X, -halfSize.Y, halfSize.Z);
                    }
                    break;

                case { Z: > 0 }: // Top face (Z+)
                    faceVertices[0] = position + new PVector3d(-halfSize.X, -halfSize.Y, halfSize.Z);
                    faceVertices[1] = position + new PVector3d(halfSize.X, -halfSize.Y, halfSize.Z);
                    faceVertices[2] = position + new PVector3d(halfSize.X, halfSize.Y, halfSize.Z);
                    faceVertices[3] = position + new PVector3d(-halfSize.X, halfSize.Y, halfSize.Z);
                    break;

                case { Z: < 0 }: // Bottom face (Z-)
                    faceVertices[0] = position + new PVector3d(-halfSize.X, halfSize.Y, -halfSize.Z);
                    faceVertices[1] = position + new PVector3d(halfSize.X, halfSize.Y, -halfSize.Z);
                    faceVertices[2] = position + new PVector3d(halfSize.X, -halfSize.Y, -halfSize.Z);
                    faceVertices[3] = position + new PVector3d(-halfSize.X, -halfSize.Y, -halfSize.Z);
                    break;
            }

            return faceVertices;
        }


    }
}
