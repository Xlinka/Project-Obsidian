using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using System;
using Renderite.Shared;

namespace Obsidian
{
    [Category(new string[] { "Obsidian/Assets/Procedural Meshes" })]
    public class PlanetMesh : ProceduralMesh
    {
        [Range(1, 8)] public readonly Sync<int> Subdivisions;
        [Range(0.5f, 10.0f)] public readonly Sync<float> Radius;
        [Range(0.1f, 10.0f)] public readonly Sync<float> NoiseScale;
        [Range(0.0f, 2.0f)] public readonly Sync<float> NoiseStrength;

        private Planet planet;
        private int _subdivisions;
        private float _radius;
        private float _noiseScale;
        private float _noiseStrength;

        protected override void OnAwake()
        {
            base.OnAwake();
            Subdivisions.Value = 4;
            Radius.Value = 1.0f;
            NoiseScale.Value = 1.0f;
            NoiseStrength.Value = 0.5f;
        }

        protected override void PrepareAssetUpdateData()
        {
            _subdivisions = Subdivisions.Value;
            _radius = Radius.Value;
            _noiseScale = NoiseScale.Value;
            _noiseStrength = NoiseStrength.Value;
        }

        protected override void ClearMeshData()
        {
            planet = null;
        }

        protected override void UpdateMeshData(MeshX meshx)
        {
            bool value = false;
            if (planet == null || planet.Subdivisions != _subdivisions || planet.Radius != _radius || planet.NoiseScale != _noiseScale || planet.NoiseStrength != _noiseStrength)
            {
                planet?.Remove();
                planet = new Planet(meshx, _subdivisions, _radius, _noiseScale, _noiseStrength);
                value = true;
            }

            planet.Subdivisions = Subdivisions.Value;
            planet.Radius = Radius.Value;
            planet.NoiseScale = NoiseScale.Value;
            planet.NoiseStrength = NoiseStrength.Value;
            planet.Update();
            uploadHint[MeshUploadHint.Flag.Geometry] = value;
        }
    }
}
