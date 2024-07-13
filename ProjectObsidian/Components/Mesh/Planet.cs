using Elements.Assets;
using Elements.Core;
using System;
using System.Collections.Generic;

namespace Obsidian
{
    public class Planet : MeshXShape
    {
        public int Subdivisions;
        public float Radius;
        public float NoiseScale;
        public float NoiseStrength;

        public Planet(MeshX mesh, int subdivisions, float radius, float noiseScale, float noiseStrength) : base(mesh)
        {
            Subdivisions = subdivisions;
            Radius = radius;
            NoiseScale = noiseScale;
            NoiseStrength = noiseStrength;
            mesh.Clear();
            GeneratePlanet(mesh, Subdivisions, Radius, NoiseScale, NoiseStrength);
        }

        private void GeneratePlanet(MeshX mesh, int subdivisions, float radius, float noiseScale, float noiseStrength)
        {
            List<float3> vertices = new List<float3>();
            List<int> triangles = new List<int>();

            // Generate initial icosahedron
            CreateIcosahedron(vertices, triangles);

            // Subdivide icosahedron
            for (int i = 0; i < subdivisions; i++)
            {
                Subdivide(vertices, triangles);
            }

            // Apply noise to vertices
            ApplyPerlinNoise(vertices, radius, noiseScale, noiseStrength);

            // Assign vertices, normals, and triangles to the mesh
            mesh.SetVertexCount(vertices.Count);
            for (int i = 0; i < vertices.Count; i++)
            {
                mesh.SetVertex(i, vertices[i]);
                mesh.SetNormal(i, vertices[i].Normalized); // Set normals directly
            }

            for (int i = 0; i < triangles.Count; i += 3)
            {
                mesh.AddTriangle(triangles[i], triangles[i + 1], triangles[i + 2]);
            }

        }

        private void CreateIcosahedron(List<float3> vertices, List<int> triangles)
        {
            float t = (1.0f + MathX.Sqrt(5.0f)) / 2.0f;

            vertices.Add(new float3(-1, t, 0).Normalized);
            vertices.Add(new float3(1, t, 0).Normalized);
            vertices.Add(new float3(-1, -t, 0).Normalized);
            vertices.Add(new float3(1, -t, 0).Normalized);

            vertices.Add(new float3(0, -1, t).Normalized);
            vertices.Add(new float3(0, 1, t).Normalized);
            vertices.Add(new float3(0, -1, -t).Normalized);
            vertices.Add(new float3(0, 1, -t).Normalized);

            vertices.Add(new float3(t, 0, -1).Normalized);
            vertices.Add(new float3(t, 0, 1).Normalized);
            vertices.Add(new float3(-t, 0, -1).Normalized);
            vertices.Add(new float3(-t, 0, 1).Normalized);

            triangles.AddRange(new int[] {
                0, 11, 5,
                0, 5, 1,
                0, 1, 7,
                0, 7, 10,
                0, 10, 11,

                1, 5, 9,
                5, 11, 4,
                11, 10, 2,
                10, 7, 6,
                7, 1, 8,

                3, 9, 4,
                3, 4, 2,
                3, 2, 6,
                3, 6, 8,
                3, 8, 9,

                4, 9, 5,
                2, 4, 11,
                6, 2, 10,
                8, 6, 7,
                9, 8, 1
            });
        }

        private void Subdivide(List<float3> vertices, List<int> triangles)
        {
            Dictionary<long, int> midpointCache = new Dictionary<long, int>();
            List<int> newTriangles = new List<int>();

            for (int i = 0; i < triangles.Count; i += 3)
            {
                int v1 = triangles[i];
                int v2 = triangles[i + 1];
                int v3 = triangles[i + 2];

                int a = GetMidpoint(midpointCache, vertices, v1, v2);
                int b = GetMidpoint(midpointCache, vertices, v2, v3);
                int c = GetMidpoint(midpointCache, vertices, v3, v1);

                newTriangles.AddRange(new int[] { v1, a, c });
                newTriangles.AddRange(new int[] { v2, b, a });
                newTriangles.AddRange(new int[] { v3, c, b });
                newTriangles.AddRange(new int[] { a, b, c });
            }

            triangles.Clear();
            triangles.AddRange(newTriangles);
        }

        private int GetMidpoint(Dictionary<long, int> midpointCache, List<float3> vertices, int v1, int v2)
        {
            long key = ((long)Math.Min(v1, v2) << 32) + Math.Max(v1, v2);

            if (midpointCache.TryGetValue(key, out int midpoint))
            {
                return midpoint;
            }

            float3 p1 = vertices[v1];
            float3 p2 = vertices[v2];
            float3 middle = (p1 + p2) / 2.0f;

            midpoint = vertices.Count;
            vertices.Add(middle.Normalized);

            midpointCache[key] = midpoint;
            return midpoint;
        }

        private void ApplyPerlinNoise(List<float3> vertices, float radius, float noiseScale, float noiseStrength)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                float3 vertex = vertices[i];
                float noise = NodeExtensions.PerlinNoise(vertex.x * noiseScale, vertex.y * noiseScale, vertex.z * noiseScale);
                float displacement = radius + noise * noiseStrength;
                vertices[i] = vertex.Normalized * displacement;
            }
        }

        public override void Update()
        {
            Mesh.RecalculateNormals(AllTriangles);
            Mesh.RecalculateTangents(AllTriangles);
        }
    }
}
