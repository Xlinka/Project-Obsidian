using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using System.Collections.Generic;

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
        private float _threshold;
        private float3 _fieldSize;
        private int _resolution;
        private List<MetaballPoint> _subscribedPoints = new();
        protected override void OnStart()
        {
            Points.Changed += OnListChange;
            foreach (var point in Points)
            {
                if (point != null)
                {
                    point.Slot.WorldTransformChanged += OnWorldTransformChanged;
                    point.Changed += OnPointChange;
                    point.Destroyed += OnPointDestroyed;
                    _subscribedPoints.Add(point);
                }
            }
        }

        protected override void OnAwake()
        {
            Threshold.Value = 1f;
            FieldSize.Value = float3.One * 10f;
            Resolution.Value = 32;
        }

        private void OnListChange(IChangeable change)
        {
            foreach (var point in _subscribedPoints)
            {
                point.Slot.WorldTransformChanged -= OnWorldTransformChanged;
                point.Changed -= OnPointChange;
                point.Destroyed -= OnPointDestroyed;
            }
            _subscribedPoints.Clear();
            foreach (var point in Points)
            {
                if (point != null)
                {
                    point.Slot.WorldTransformChanged += OnWorldTransformChanged;
                    point.Changed += OnPointChange;
                    point.Destroyed += OnPointDestroyed;
                    _subscribedPoints.Add(point);
                }
            }
            MarkChangeDirty();
            if (shape != null)
                shape.scheduleRangeDatasRecompute = true;

        }

        private void OnPointChange(IChangeable change)
        {
            MarkChangeDirty();
            if (shape != null)
                shape.scheduleRangeDatasRecompute = true;
        }

        private void OnPointDestroyed(IDestroyable destroy)
        {
            MarkChangeDirty();
            if (shape != null)
                shape.scheduleRangeDatasRecompute = true;
        }

        private void OnWorldTransformChanged(Slot s)
        {
            MarkChangeDirty();
        }

        protected override void PrepareAssetUpdateData()
        {
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
            if (shape == null)
            {
                shape = new MetaballShape(meshx);
            }

            shape.Points = _subscribedPoints;
            shape.OriginSlot = Slot;
            shape.Threshold = _threshold;
            shape.FieldSize = _fieldSize;
            shape.Resolution = _resolution;

            meshx.Clear();
            shape.Update();
        }
    }
}