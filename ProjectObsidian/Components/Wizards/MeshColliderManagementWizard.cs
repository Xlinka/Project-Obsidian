using System;
using System.Collections.Generic;
using System.Linq;
using FrooxEngine;
using FrooxEngine.UIX;
using Elements.Core;
using FrooxEngine.Undo;

namespace Obsidian
{
    [DataModelType]
    public enum ReplacementColliderComponent
    {
        BoxCollider,
        SphereCollider,
        CapsuleCollider,
        CylinderCollider,
        ConvexHullCollider
    }

    [DataModelType]
    public enum SetupBoundsType
    {
        None,
        SetupFromLocalBounds,
        SetupFromPreciseBounds,
    }

    [DataModelType]
    public enum UseTagMode
    {
        IgnoreTag,
        IncludeOnlyWithTag,
        ExcludeAllWithTag
    }

    [Category("Obsidian/Wizards")]
    public class MeshColliderManagementWizard : Component, IDeveloperInterface
    {
        public readonly SyncRef<Slot> Root;
        public readonly Sync<bool> IgnoreInactive;
        public readonly Sync<bool> IgnoreDisabled;
        public readonly Sync<bool> IgnoreNonPersistent;
        public readonly Sync<bool> PreserveColliderSettings;
        public readonly Sync<ReplacementColliderComponent> replacementColliderComponent;
        public readonly Sync<SetupBoundsType> setupBoundsType;
        public readonly Sync<UseTagMode> useTagMode;
        public readonly Sync<float> HighlightDuration;
        public readonly Sync<colorX> HighlightColor; // Changed to colorX to avoid color type mismatch
        public readonly SyncRef<TextField> tag;
        public readonly SyncRef<Text> resultsText;

        private Slot _scrollAreaRoot;
        private UIBuilder _listBuilder;

        protected override void OnAttach()
        {
            base.OnAttach();
            InitializeUI();
        }

        private void InitializeUI()
        {
            UIBuilder ui = RadiantUI_Panel.SetupPanel(Slot, "Mesh Collider Management Wizard", new float2(420f, 800f));
            Slot.LocalScale *= 0.0005f;
            RadiantUI_Constants.SetupEditorStyle(ui);

            ui.VerticalLayout(4f);
            ui.Style.Height = 24f;

            ui.Text("Root Slot");
            ui.Next("Root");
            ui.Current.AttachComponent<RefEditor>().Setup(Root);

            AddBooleanOption(ui, "Ignore Inactive", IgnoreInactive);
            AddBooleanOption(ui, "Ignore Disabled", IgnoreDisabled);
            AddBooleanOption(ui, "Ignore Non-Persistent", IgnoreNonPersistent);
            AddBooleanOption(ui, "Preserve Collider Settings", PreserveColliderSettings);

            ui.Text("Tag Filter");
            ui.Next("Tag");
            tag.Target = ui.TextField();
            ui.EnumMemberEditor(useTagMode);

            ui.Text("Replacement Collider Component");
            ui.EnumMemberEditor(replacementColliderComponent);

            ui.Text("Setup Bounds Type");
            ui.EnumMemberEditor(setupBoundsType);

            ui.Text("Highlight Duration");
            ui.FloatField(HighlightDuration);
            ui.Text("Highlight Color");
            ui.ColorXMemberEditor(HighlightColor);

            ui.Button("List MeshColliders", (IButton button, ButtonEventData eventData) => OnListMeshColliders());
            ui.Button("Replace All MeshColliders", (IButton button, ButtonEventData eventData) => OnReplaceAllMeshColliders());
            ui.Button("Remove All MeshColliders", (IButton button, ButtonEventData eventData) => OnRemoveAllMeshColliders());

            resultsText.Target = ui.Text("");
        }

        private void AddBooleanOption(UIBuilder ui, string label, Sync<bool> syncValue)
        {
            ui.HorizontalElementWithLabel(label, 0.8f, () => ui.BooleanMemberEditor(syncValue));
        }

        private void OnListMeshColliders()
        {
            PopulateList();
        }

        private void PopulateList()
        {
            _scrollAreaRoot?.DestroyChildren();
            GetMeshColliders().ForEach(mc => CreateColliderElement(mc));
        }

        private void CreateColliderElement(MeshCollider mc)
        {
            var element = _listBuilder.Next("Element");
            var refField = element.AttachComponent<ReferenceField<MeshCollider>>();
            refField.Reference.Target = mc;

            var builder = new UIBuilder(element);
            builder.HorizontalLayout(10f);

            builder.Button("Jump", (IButton button, ButtonEventData eventData) => JumpToCollider(mc.Slot));
            builder.Button("Highlight", (IButton button, ButtonEventData eventData) => HighlightCollider(mc.Slot));
            builder.Button("Replace", (IButton button, ButtonEventData eventData) => ReplaceCollider(mc));
            builder.Button("Remove", (IButton button, ButtonEventData eventData) => RemoveCollider(mc));

            builder.Current.AttachComponent<RefEditor>().Setup(refField.Reference);
        }

        private List<MeshCollider> GetMeshColliders()
        {
            string tagText = tag.Target?.ToString() ?? string.Empty;
            var colliders = (Root.Target ?? World.RootSlot)
                .GetComponentsInChildren<MeshCollider>()
                .Where(mc => (!IgnoreInactive.Value || mc.Slot.IsActive)
                             && (!IgnoreDisabled.Value || mc.Enabled)
                             && (!IgnoreNonPersistent.Value || mc.IsPersistent)
                             && (useTagMode.Value == UseTagMode.IgnoreTag
                                 || (useTagMode.Value == UseTagMode.IncludeOnlyWithTag && mc.Slot.Tag == tagText)
                                 || (useTagMode.Value == UseTagMode.ExcludeAllWithTag && mc.Slot.Tag != tagText)))
                .ToList();

            ShowResults($"{colliders.Count} MeshColliders found.");
            return colliders;
        }

        private void ShowResults(string message)
        {
            if (resultsText.Target != null)
                resultsText.Target.Content.Value = message;
        }

        private void JumpToCollider(Slot slot)
        {
            var userRoot = LocalUser?.GetComponent<UserRoot>();
            if (userRoot != null)
            {
                userRoot.JumpToPoint(slot.GlobalPosition, distance: 1.5f);
            }
            else
            {
                UniLog.Log("Failed to retrieve UserRoot or LocalUser.");
            }
        }

        private void HighlightCollider(Slot slot)
        {
            HighlightHelper.FlashHighlight(slot, null, HighlightColor.Value, HighlightDuration.Value);
        }

        private void ReplaceCollider(MeshCollider mc)
        {
            var slot = mc.Slot;
            mc.UndoableDestroy();

            Component newCollider = replacementColliderComponent.Value switch
            {
                ReplacementColliderComponent.BoxCollider => slot.AttachComponent<BoxCollider>(),
                ReplacementColliderComponent.SphereCollider => slot.AttachComponent<SphereCollider>(),
                ReplacementColliderComponent.CapsuleCollider => slot.AttachComponent<CapsuleCollider>(),
                ReplacementColliderComponent.CylinderCollider => slot.AttachComponent<CylinderCollider>(),
                ReplacementColliderComponent.ConvexHullCollider => slot.AttachComponent<ConvexHullCollider>(),
                _ => null
            };

            SetupColliderBounds(newCollider, mc);
        }

        private void SetupColliderBounds(Component collider, Component mc)
        {
            if (collider is BoxCollider bc && mc is BoxCollider oldBc)
            {
                bc.Type.Value = oldBc.Type.Value;
                bc.Mass.Value = oldBc.Mass.Value;
                bc.Size.Value = oldBc.Size.Value; 
            }
            else if (collider is SphereCollider sc && mc is SphereCollider oldSc)
            {
                sc.Radius.Value = oldSc.Radius.Value;
                sc.Mass.Value = oldSc.Mass.Value; 
            }
            // Additional collider type handling as needed
        }

        private void RemoveCollider(MeshCollider mc)
        {
            mc.UndoableDestroy();
            PopulateList();
        }

        private void OnReplaceAllMeshColliders()
        {
            GetMeshColliders().ForEach(mc => ReplaceCollider(mc));
            ShowResults("All matching MeshColliders replaced.");
        }

        private void OnRemoveAllMeshColliders()
        {
            GetMeshColliders().ForEach(mc => mc.UndoableDestroy());
            PopulateList();
            ShowResults("All matching MeshColliders removed.");
        }
    }
}
