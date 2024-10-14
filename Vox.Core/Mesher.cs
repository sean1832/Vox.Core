using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Vox.Core.DataModels;
using Vox.Core.Meshing;

namespace Vox.Core
{
    public enum MeshingAlgorithm
    {
        Naive,
        FaceCulling,
        MarchingCubes,
        Greedy
    }

    public static class Mesher
    {
        public static List<PMesh> Generate(MeshingAlgorithm algorithm, List<PVector3d> positions, List<PVector3d> voxelSizes, CordSystem cordSystem = CordSystem.RightHanded)
        {
            switch (algorithm)
            {
                case MeshingAlgorithm.Naive:
                    NaiveMesher naiveMesher = new NaiveMesher(cordSystem);
                    return naiveMesher.GenerateMeshes(positions, voxelSizes);
                case MeshingAlgorithm.FaceCulling:
                    FaceCullingMesher faceCullingMesher = new FaceCullingMesher(cordSystem);
                    return new List<PMesh>() { faceCullingMesher.GenerateMesh(positions, voxelSizes) };
                case MeshingAlgorithm.MarchingCubes:
                    throw new NotImplementedException($"Algorithm not implemented: {nameof(MeshingAlgorithm.MarchingCubes)}");
                case MeshingAlgorithm.Greedy:
                    throw new NotImplementedException($"Algorithm not implemented: {nameof(MeshingAlgorithm.Greedy)}");
                default:
                    throw new InvalidEnumArgumentException();
            }
        }
    }
}
