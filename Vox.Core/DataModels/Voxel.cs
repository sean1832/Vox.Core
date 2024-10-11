using System;
using System.Collections.Generic;
using System.Text;

namespace Vox.Core.DataModels
{
    public enum VoxelState
    {
        Outside,
        Intersecting,
        Inside,
    }

    public readonly struct Voxel
    {
        public readonly PVector3d Position;
        public readonly PVector3d Size;
        public readonly VoxelState VoxelState;
        

        public Voxel(PVector3d position, PVector3d size, VoxelState state)
        {
            Position = position;
            Size = size;
            VoxelState = state;
        }
    }
}
