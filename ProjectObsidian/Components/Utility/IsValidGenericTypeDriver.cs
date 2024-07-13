using Elements.Core;
using FrooxEngine;
using System;
using System.Linq;

namespace Obsidian;

[Category(new string[] { "Obsidian/Utility" })]
public class IsValidGenericTypeDriver : Component
{
    public readonly SyncType Type;

    public readonly FieldDrive<bool> Target;

    protected override void OnChanges()
    {
        base.OnChanges();
        if (!Target.IsLinkValid) return;
        if (Type.Value == null || !Type.Value.IsGenericType)
        {
            Target.Target.Value = false;
        }
        else
        {
            Target.Target.Value = Type.Value.IsValidGenericType(validForInstantiation: true);
        }
    }
}