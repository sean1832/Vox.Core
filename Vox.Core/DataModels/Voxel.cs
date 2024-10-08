using System;
using System.Collections.Generic;
using System.Text;
using Vox.Core.Algorithm.SVO;

namespace Vox.Core.DataModels
{
    public class Voxel
    {
        public PVector3d Position;
        public PVector3d Size;
        public VoxelState VoxelState;
        

        public Voxel(PVector3d position, PVector3d size, VoxelState state)
        {
            Position = position;
            Size = size;
            VoxelState = state;
        }
    }
}
