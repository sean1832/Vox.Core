using System;
using System.Collections.Generic;
using System.Text;
using Vox.Core.DataModels;

namespace Vox.Core.Algorithms.MortonCode
{
    internal class Morton3D
    {
        private readonly int offsetX;
        private readonly int offsetY;
        private readonly int offsetZ;

        public Morton3D(int minX, int minY, int minZ)
        {
            offsetX = -minX;
            offsetY = -minY;
            offsetZ = -minZ;
        }

        public ulong Encode(int x, int y, int z)
        {
            // Shift the coordinates to non-negative values
            uint shiftedX = (uint)(x + offsetX);
            uint shiftedY = (uint)(y + offsetY);
            uint shiftedZ = (uint)(z + offsetZ);

            ulong answer = 0;
            answer |= Part1By2(shiftedX) | (Part1By2(shiftedY) << 1) | (Part1By2(shiftedZ) << 2);
            return answer;
        }

        public (int, int, int) Decode(ulong mortonCode)
        {
            int x = Compact1By2(mortonCode) - offsetX;
            int y = Compact1By2(mortonCode >> 1) - offsetY;
            int z = Compact1By2(mortonCode >> 2) - offsetZ;
            return (x, y, z);
        }


        // Part 1 by 2: Spread the bits of the input by 2 positions
        private static ulong Part1By2(uint n)
        {
            n = (n | (n << 16)) & 0x030000FF;
            n = (n | (n << 8)) & 0x0300F00F;
            n = (n | (n << 4)) & 0x030C30C3;
            n = (n | (n << 2)) & 0x09249249;
            return n;
        }

        // Compact 1 by 2: Reverses the bit spreading (de-interleaves the bits)
        private static int Compact1By2(ulong n)
        {
            n &= 0x09249249;
            n = (n ^ (n >> 2)) & 0x030C30C3;
            n = (n ^ (n >> 4)) & 0x0300F00F;
            n = (n ^ (n >> 8)) & 0x030000FF;
            n = (n ^ (n >> 16)) & 0x000003FF;
            return (int)n;
        }
    }
}
