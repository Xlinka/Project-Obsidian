using FrooxEngine;

namespace Obsidian;

public readonly struct ComponentDataResult
{
    public readonly ComponentData data;

    public readonly DataFeedItemChange change;

    public ComponentDataResult(ComponentData data, DataFeedItemChange change)
    {
        this.data = data;
        this.change = change;
    }
}