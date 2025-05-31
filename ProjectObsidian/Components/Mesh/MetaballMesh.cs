using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using System.Collections.Generic;
using System.Linq;

namespace Obsidian
{
    [Category(new string[] { "Obsidian/Assets/Procedural Meshes" })]
    public class MetaballMesh : ProceduralMesh
    {
        public readonly Sync<float> Threshold;
        public readonly Sync<float3> FieldSize;
        public readonly Sync<int> Resolution;
        public readonly SyncRefList<MetaballPoint> Points;
        private MetaballShape shape;
        private List<MetaballPoint> _points;
        private float _threshold;
        private float3 _fieldSize;
        private int _resolution;
        private HashSet<MetaballPoint> _subscribedPoints = new();
        private bool dirty;
        protected override void OnStart()
        {
            Points.Changed += OnListChange;
            foreach (var point in Points)
            {
                point.Slot.WorldTransformChanged += OnWorldTransformChanged;
                point.Changed += OnChange;
                _subscribedPoints.Add(point);
            }
        }

        private void OnListChange(IChangeable change)
        {
            foreach (var point in _subscribedPoints)
            {
                point.Slot.WorldTransformChanged -= OnWorldTransformChanged;
                point.Changed -= OnChange;
            }
            _subscribedPoints.Clear();
            foreach (var point in Points)
            {
                if (point != null)
                {
                    point.Slot.WorldTransformChanged += OnWorldTransformChanged;
                    point.Changed += OnChange;
                    _subscribedPoints.Add(point);
                }
            }
        }

        private void OnChange(IChangeable change)
        {
            MarkChangeDirty();
            dirty = true;
        }

        private void OnWorldTransformChanged(Slot s)
        {
            MarkChangeDirty();
            dirty = true;
        }

        protected override void PrepareAssetUpdateData()
        {
            _points = Points.ToList();
            _threshold = Threshold.Value;
            _fieldSize = FieldSize.Value;
            _resolution = Resolution.Value;
        }

        protected override void ClearMeshData()
        {
            shape?.Remove();
            shape = null;
        }

        protected override void UpdateMeshData(MeshX meshx)
        {
            if (shape == null || dirty)
            {
                shape?.Remove();
                shape = new MetaballShape(meshx, _points, Slot);
            }

            shape.Threshold = _threshold;
            shape.FieldSize = _fieldSize;
            shape.Resolution = _resolution;
            meshx.Clear();
            shape.Update();
            dirty = false;
        }
    }
}