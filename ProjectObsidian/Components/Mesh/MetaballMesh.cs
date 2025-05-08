using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Obsidian
{
    [Category(new string[] { "Obsidian/Assets/Procedural Meshes" })]
    public class MetaballMesh : ProceduralMesh
    {
        public readonly SyncRefList<MetaballPoint> Points;
        private MetaballShape shape;
        private List<MetaballPoint> _points;
        protected override void OnAwake()
        {
        }

        protected override void PrepareAssetUpdateData()
        {
            _points = Points.ToList();
        }

        protected override void ClearMeshData()
        {
            shape = null;
        }

        protected override void UpdateMeshData(MeshX meshx)
        {
            if (shape == null || shape.Points != _points)
            {
                shape?.Remove();
                shape = new MetaballShape(meshx, _points);
            }

            shape.Update();
        }
    }
}