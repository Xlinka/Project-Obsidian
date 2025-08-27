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
        private float _threshold;
        private float3 _fieldSize;
        private int _resolution;
        private List<MetaballPoint> _subscribedPoints = new();
        private List<MetaballPoint> _subscribedPointsCopy;
        private bool _scheduleRangeDatasRecompute;
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
            _scheduleRangeDatasRecompute = true;
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
            _scheduleRangeDatasRecompute = true;
            MarkChangeDirty();
        }

        private void OnPointChange(IChangeable change)
        {
            _scheduleRangeDatasRecompute = true;
            MarkChangeDirty();
        }

        private void OnPointDestroyed(IDestroyable destroy)
        {
            _scheduleRangeDatasRecompute = true;
            MarkChangeDirty();
        }

        private void OnWorldTransformChanged(Slot s)
        {
            _scheduleRangeDatasRecompute = true;
            MarkChangeDirty();
        }

        protected override void PrepareAssetUpdateData()
        {
            _threshold = Threshold.Value;
            _fieldSize = FieldSize.Value;
            _resolution = Resolution.Value;
            _subscribedPointsCopy = _subscribedPoints.ToList();
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

            shape.Points = _subscribedPointsCopy;
            shape.OriginSlot = Slot;
            shape.Threshold = _threshold;
            shape.FieldSize = _fieldSize;
            shape.Resolution = _resolution;
            shape.scheduleRangeDatasRecompute = _scheduleRangeDatasRecompute;
            _scheduleRangeDatasRecompute = false;

            meshx.Clear();
            shape.Update();
        }
    }
}