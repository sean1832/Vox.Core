using System;
using System.Collections.Generic;
using System.Text;
using Vox.Core.DataModels;

namespace Vox.Core.Meshing
{
    internal class NaiveMesher: BaseMesher
    {
        private static readonly PVector3d[] Directions = new PVector3d[]
        {
        new PVector3d(1, 0, 0),  // Front (X+)
        new PVector3d(-1, 0, 0), // Back (X-)
        new PVector3d(0, -1, 0), // Left (Y-)
        new PVector3d(0, 1, 0),  // Right (Y+)
        new PVector3d(0, 0, 1),  // Top (Z+)
        new PVector3d(0, 0, -1)  // Bottom (Z-)
        };

        public override PMesh GenerateMesh(List<PVector3d> positions, List<PVector3d> voxelSizes)
        {
            if (positions.Count != voxelSizes.Count)
            {
                throw new ArgumentException("Positions and voxelSizes must have the same length.");
            }

            List<PVector3d> vertices = new List<PVector3d>();
            List<int[]> faces = new List<int[]>();

            // Iterate over each voxel to generate a full cube for each one
            for (int i = 0; i < positions.Count; i++)
            {
                var position = positions[i];
                var voxelSize = voxelSizes[i];

                // Generate faces for all six sides of the voxel
                foreach (var direction in Directions)
                {
                    GenerateFace(position, direction, vertices, faces, voxelSize);
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
    }

}
