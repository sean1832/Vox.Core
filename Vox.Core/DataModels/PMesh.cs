using System;
using System.Collections.Generic;

namespace Vox.Core.DataModels
{
    public class PMesh
    {
        public List<PVector3d> Vertices;
        public List<int[]> Faces;
        public List<PBoundingBox> TriangleBounds { get; set; }

        public PMesh(List<PVector3d> vertices, List<int[]> faces)
        {
            Vertices = vertices;
            Faces = faces;
            TriangleBounds = new List<PBoundingBox>();
        }

        public List<PVector3d> ClosestPoints(List<PVector3d> points, double distance)
        {
            throw new NotImplementedException();
        }

        public bool IsClose()
        {
            // Dictionary to count the occurrences of each edge
            Dictionary<(int, int), int> edgeCount = new Dictionary<(int, int), int>();

            // Loop over each face and each edge in the face
            foreach (var face in Faces)
            {
                int numVertices = face.Length;

                // Loop over the edges of the face (assuming the face is a triangle or polygon)
                for (int i = 0; i < numVertices; i++)
                {
                    int v1 = face[i];
                    int v2 = face[(i + 1) % numVertices]; // Get the next vertex, wrapping around at the end

                    // Create an edge and store it in the dictionary
                    var edge = CreateEdge(v1, v2);

                    if (edgeCount.ContainsKey(edge))
                    {
                        edgeCount[edge]++;
                    }
                    else
                    {
                        edgeCount[edge] = 1;
                    }
                }
            }

            // Check if any edge is shared by only one face
            foreach (var count in edgeCount.Values)
            {
                if (count != 2)
                {
                    return false; // Mesh is open (some edge is not shared by exactly two faces)
                }
            }

            return true; // All edges are shared by exactly two faces, mesh is closed
        }

        private (int, int) CreateEdge(int v1, int v2)
        {
            return (Math.Min(v1, v2), (Math.Max(v1, v2)));
        }

        public PBoundingBox GetBoundingBox()
        {
            double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

            foreach (var vertex in Vertices)
            {
                if (vertex.X < minX) minX = vertex.X;
                if (vertex.Y < minY) minY = vertex.Y;
                if (vertex.Z < minZ) minZ = vertex.Z;

                if (vertex.X > maxX) maxX = vertex.X;
                if (vertex.Y > maxY) maxY = vertex.Y;
                if (vertex.Z > maxZ) maxZ = vertex.Z;
            }

            return new PBoundingBox(new PVector3d(minX, minY, minZ), new PVector3d(maxX, maxY, maxZ));
        }

        // Call this method after loading the mesh data
        public void ComputeTriangleBounds()
        {
            foreach (var face in Faces)
            {
                var v0 = Vertices[face[0]];
                var v1 = Vertices[face[1]];
                var v2 = Vertices[face[2]];

                var triMin = new PVector3d(
                    Math.Min(v0.X, Math.Min(v1.X, v2.X)),
                    Math.Min(v0.Y, Math.Min(v1.Y, v2.Y)),
                    Math.Min(v0.Z, Math.Min(v1.Z, v2.Z))
                );

                var triMax = new PVector3d(
                    Math.Max(v0.X, Math.Max(v1.X, v2.X)),
                    Math.Max(v0.Y, Math.Max(v1.Y, v2.Y)),
                    Math.Max(v0.Z, Math.Max(v1.Z, v2.Z))
                );

                TriangleBounds.Add(new PBoundingBox(triMin, triMax));
            }
        }
    }
}
