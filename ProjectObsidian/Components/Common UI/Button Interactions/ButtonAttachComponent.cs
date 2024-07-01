using System;
using FrooxEngine;
using FrooxEngine.Undo;

namespace Obsidian;

[Category(new string[] { "Obsidian/Common UI/Button Interactions" })]
public class ButtonAttachComponent : Component, IButtonPressReceiver, IComponent, IComponentBase, IDestroyable, IWorker, IWorldElement, IUpdatable, IChangeable, IAudioUpdatable, IInitializable, ILinkable
{
    public readonly SyncRef<Slot> TargetSlot;

    public readonly SyncType ComponentType;

    public readonly Sync<bool> Undoable;

    protected override void OnAttach()
    {
        base.OnAwake();
        Undoable.Value = true;
    }

    public void Pressed(IButton button, ButtonEventData eventData)
    {
        Slot target = TargetSlot.Target;
        Type componentType = ComponentType.Value;
        if (target != null && componentType != null && componentType.ContainsGenericParameters == false)
        {
            var comp = target.AttachComponent(componentType);
            if (Undoable)
            {
                comp.CreateSpawnUndoPoint();
            }
        }
    }

    public void Pressing(IButton button, ButtonEventData eventData)
    {
    }

    public void Released(IButton button, ButtonEventData eventData)
    {
    }
}