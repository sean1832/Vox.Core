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

    public class Mesher
    {
        public PMesh Generate(MeshingAlgorithm algorithm, List<PVector3d> positions, List<PVector3d> voxelSizes)
        {
            switch (algorithm)
            {
                case MeshingAlgorithm.Naive:
                    throw new NotImplementedException();
                case MeshingAlgorithm.FaceCulling:
                    FaceCullingMesher mesher = new FaceCullingMesher();
                    return mesher.Generate(positions, voxelSizes);
                case MeshingAlgorithm.MarchingCubes:
                    throw new NotImplementedException();
                case MeshingAlgorithm.Greedy:
                    throw new NotImplementedException();
                default:
                    throw new InvalidEnumArgumentException();
            }
        }
    }
}
