using FrooxEngine.UIX;
using System;
using FrooxEngine;
using FrooxEngine.Undo;
using Elements.Core;
using System.Collections.Generic;

namespace Obsidian
{
    [Category("Obsidian/Wizards")]
    public class LightSourcesWizard : Component, IDeveloperInterface
    {
        public readonly SyncRef<Slot> Root;
        public readonly Sync<bool> ProcessPointLights;
        public readonly Sync<bool> ProcessSpotLights;
        public readonly Sync<bool> ProcessDirectionalLights;
        public readonly Sync<bool> ProcessDisabled;
        public readonly Sync<ShadowType> TargetShadowType;
        public readonly Sync<bool> FilterColors;
        public readonly Sync<colorX> Color;
        private readonly SyncRef<TextField> _tag;
        private readonly SyncRef<FloatTextEditorParser> _intensityField;
        private readonly SyncRef<FloatTextEditorParser> _rangeField;
        private readonly SyncRef<FloatTextEditorParser> _spotAngleField;
        private readonly SyncRef<FloatTextEditorParser> _maxColorVariance;

        protected override void OnAwake()
        {
            base.OnAwake();
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            ProcessPointLights.Value = true;
            ProcessSpotLights.Value = true;
            ProcessDirectionalLights.Value = true;
            ProcessDisabled.Value = false;
        }

        protected override void OnAttach()
        {
            base.OnAttach();
            SetupUI();
        }

        private void SetupUI()
        {
            UIBuilder ui = RadiantUI_Panel.SetupPanel(Slot, "Light Source Wizard", new float2(500f, 1100f));
            Slot.LocalScale *= 0.0005f;
            RadiantUI_Constants.SetupEditorStyle(ui);

            ui.VerticalLayout(4f);
            ui.Style.Height = 24f;

            ui.Text("Light Source Wizard", alignment: Alignment.MiddleCenter);

            SetupMainUI(ui);
            SetupColorFilterUI(ui);
            SetupLightControlsUI(ui);
            SetupLightActionsUI(ui);
        }

        private void SetupMainUI(UIBuilder ui)
        {
            ui.Text("Process Root");
            ui.Next("Root");
            ui.Current.AttachComponent<RefEditor>().Setup(Root);

            AddBooleanOption(ui, "Point Lights", ProcessPointLights);
            AddBooleanOption(ui, "Spot Lights", ProcessSpotLights);
            AddBooleanOption(ui, "Directional Lights", ProcessDirectionalLights);
            AddBooleanOption(ui, "Disabled Lights", ProcessDisabled);

            ui.Text("Tag Filter");
            _tag.Target = ui.TextField();
        }

        private void SetupColorFilterUI(UIBuilder ui)
        {
            AddBooleanOption(ui, "Filter Colors", FilterColors);
            ui.Text("Color Filter");
            ui.ColorXMemberEditor(Color);
            ui.Text("Max Color Variance");
            _maxColorVariance.Target = ui.FloatField(0.0f, 1f, int.MaxValue);
        }

        private void SetupLightControlsUI(UIBuilder ui)
        {
            ui.Text("-------");
            ui.EnumMemberEditor(TargetShadowType);
            ui.Button("Set Shadow Type", SetShadowType);

            ui.Text("-------");
            _intensityField.Target = ui.FloatField(0.0f, float.PositiveInfinity, int.MaxValue);
            ui.Button("Multiply Intensity", MultiplyIntensity);
            ui.Button("Set Intensity", SetIntensity);

            ui.Text("-------");
            _rangeField.Target = ui.FloatField(0.0f, float.PositiveInfinity, int.MaxValue);
            ui.Button("Multiply Range", MultiplyRange);
            ui.Button("Set Range", SetRange);

            ui.Text("-------");
            _spotAngleField.Target = ui.FloatField(0.0f, 180.0f, int.MaxValue);
            ui.Button("Set Spot Angle", SetSpotAngle);
        }

        private void SetupLightActionsUI(UIBuilder ui)
        {
            ui.Text("-------");
            ui.Button("Enable Lights", Enable);
            ui.Button("Disable Lights", Disable);
            ui.Button("Delete Lights", Remove);
        }

        private void AddBooleanOption(UIBuilder ui, string label, Sync<bool> syncValue)
        {
            ui.HorizontalElementWithLabel(label, 0.8f, () => ui.BooleanMemberEditor(syncValue));
        }

        private void SetShadowType(IButton button, ButtonEventData eventData) =>
            ProcessLights(l => { l.ShadowType.CreateUndoPoint(); l.ShadowType.Value = TargetShadowType.Value; });

        private void MultiplyIntensity(IButton button, ButtonEventData eventData) =>
            ProcessLights(l => { l.Intensity.CreateUndoPoint(); l.Intensity.Value *= (float)_intensityField.Target.ParsedValue; });

        private void SetIntensity(IButton button, ButtonEventData eventData) =>
            ProcessLights(l => { l.Intensity.CreateUndoPoint(); l.Intensity.Value = (float)_intensityField.Target.ParsedValue; });

        private void MultiplyRange(IButton button, ButtonEventData eventData) =>
            ProcessLights(l => { l.Range.CreateUndoPoint(); l.Range.Value *= (float)_rangeField.Target.ParsedValue; });

        private void SetRange(IButton button, ButtonEventData eventData) =>
            ProcessLights(l => { l.Range.CreateUndoPoint(); l.Range.Value = (float)_rangeField.Target.ParsedValue; });

        private void SetSpotAngle(IButton button, ButtonEventData eventData) =>
            ProcessLights(l => { l.SpotAngle.CreateUndoPoint(); l.SpotAngle.Value = (float)_spotAngleField.Target.ParsedValue; });

        private void Remove(IButton button, ButtonEventData eventData) =>
            ProcessLights(l => l.UndoableDestroy());

        private void Disable(IButton button, ButtonEventData eventData) =>
            ProcessLights(l => { l.EnabledField.CreateUndoPoint(); l.Enabled = false; });

        private void Enable(IButton button, ButtonEventData eventData) =>
            ProcessLights(l => { l.EnabledField.CreateUndoPoint(); l.Enabled = true; });

        private void ProcessLights(Action<Light> process)
        {
            string tag = _tag.Target.TargetString;
            color filterColor = (color)Color.Value;
            World.BeginUndoBatch("Modify lights");

            foreach (Light light in GetFilteredLights(tag, filterColor))
            {
                process(light);
            }

            World.EndUndoBatch();
        }

        private IEnumerable<Light> GetFilteredLights(string tag, color filterColor)
        {
            return (Root.Target ?? World.RootSlot).GetComponentsInChildren<Light>(l =>
                IsLightEligible(l, tag, filterColor));
        }

        private bool IsLightEligible(Light light, string tag, color filterColor)
        {
            if (!ProcessDisabled.Value && (!light.Enabled || !light.Slot.IsActive)) return false;
            if (!string.IsNullOrEmpty(tag) && light.Slot.Tag != tag) return false;
            if (FilterColors && !ColorWithinTolerance((color)light.Color.Value, filterColor, _maxColorVariance.Target.ParsedValue)) return false;

            return light.LightType.Value switch
            {
                LightType.Point => ProcessPointLights.Value,
                LightType.Directional => ProcessDirectionalLights.Value,
                LightType.Spot => ProcessSpotLights.Value,
                _ => false
            };
        }

        private bool ColorWithinTolerance(color col1, color col2, float tolerance) =>
            Math.Abs(col1.r - col2.r) < tolerance &&
            Math.Abs(col1.g - col2.g) < tolerance &&
            Math.Abs(col1.b - col2.b) < tolerance;
    }
}
