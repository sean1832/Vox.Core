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
        public FaceCullingMesher(CordSystem cordSystem) : base(cordSystem)
        {
        }

        public override PMesh GenerateMesh(List<PVector3d> positions, List<PVector3d> voxelSizes)
        {
            if (positions.Count != voxelSizes.Count)
            {
                throw new ArgumentException("Positions and voxelSizes must have the same length.");
            }
            // Create a dictionary or spatial hash to quickly check for neighbors
            Dictionary<PVector3d, PVector3d> voxelMap = new Dictionary<PVector3d, PVector3d>();
            for (int i = 0; i < positions.Count; i++)
            {
                voxelMap[positions[i]] = voxelSizes[i];
            }

            List<PVector3d> vertices = new List<PVector3d>();
            List<int[]> faces = new List<int[]>();

            for (int i = 0; i < positions.Count; i++)
            {
                var position = positions[i];
                var voxelSize = voxelSizes[i];

                // Generate faces for all six sides of the voxel
                foreach (var direction in Directions)
                {
                    // if the neighbor does not exist, make a face
                    if (!GetNeighbor(position, direction, voxelSize, voxelMap, out _))
                    {
                        MakeFace(position, direction, vertices, faces, voxelSize, isQuad:true);
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

        private bool GetNeighbor(PVector3d position, PVector3d direction, PVector3d voxelSize, Dictionary<PVector3d, PVector3d> voxelMap, out PVector3d neighbor)
        {
            PVector3d neighborPos = position + (direction * voxelSize);

            // Adjust comparison to use a small tolerance for floating-point precision issues
            foreach (var key in voxelMap.Keys)
            {
                if (PVector3d.Equals(neighborPos, key))
                {
                    neighbor = key;
                    return true;
                }
            }

            neighbor = PVector3d.Zero;
            return false;
        }
    }
}
