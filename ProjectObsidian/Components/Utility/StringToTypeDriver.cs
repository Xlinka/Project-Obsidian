using Elements.Core;
using FrooxEngine;
using System;

namespace Obsidian;

[Category(new string[] { "Obsidian/Utility" })]
public class StringToTypeDriver : Component
{
    public readonly Sync<string> Text;

    public readonly FieldDrive<Type> Target;

    protected override void OnChanges()
    {
        base.OnChanges();
        if (!Target.IsLinkValid) return;
        if (string.IsNullOrWhiteSpace(Text.Value))
        {
            Target.Target.Value = null;
        }
        else
        {
            try
            {
                var parsedType = World.Types.ParseNiceType(Text.Value, allowAmbigious: true);
                Target.Target.Value = parsedType;
            }
            catch (Exception ex)
            {
                UniLog.Warning("Exception when parsing type from string:\n" + ex.ToString());
                Target.Target.Value = null;
            }
        }
    }
}