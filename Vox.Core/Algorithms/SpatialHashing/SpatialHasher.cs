using System;
using Vox.Core.DataModels;

namespace Vox.Core.Algorithms.SpatialHashing
{
    internal class SpatialHasher
    {
        private readonly float _cellSize;

        public SpatialHasher(float cellSize)
        {
            _cellSize = cellSize;
        }

        // Hash function to map a PVector3d to an integer key
        public int Hash(PVector3d position)
        {
            int x = (int)Math.Floor(position.X / _cellSize);
            int y = (int)Math.Floor(position.Y / _cellSize);
            int z = (int)Math.Floor(position.Z / _cellSize);

            // Combine the coordinates into a single hash
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + x;
                hash = hash * 31 + y;
                hash = hash * 31 + z;
                return hash;
            }
        }
    }

}
