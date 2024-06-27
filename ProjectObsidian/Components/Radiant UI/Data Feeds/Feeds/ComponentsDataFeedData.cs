using System.Collections.Generic;
using FrooxEngine;

namespace Obsidian;

internal class ComponentsDataFeedData
{
    private List<ComponentData> _data = new List<ComponentData>();

    private Dictionary<string, ComponentData> _dataByUniqueId = new Dictionary<string, ComponentData>();

    public IEnumerable<ComponentData> ComponentData => _data;

    public void Clear()
    {
        _data.Clear();
        _dataByUniqueId.Clear();
    }

    public ComponentData RegisterComponent(Component c)
    {
        bool created;
        ComponentData componentData = EnsureEntry(c, out created);
        if (componentData.component != null)
        {
            //throw new InvalidOperationException("Component with this ReferenceID has already been added!");
        }
        componentData.component = c;
        return componentData;
    }

    private ComponentData RegisterMember(ISyncMember member, out bool createdEntry)
    {
        ComponentData componentData = EnsureEntry(member.FindNearestParent<Component>(), out createdEntry);
        componentData.AddMember(member);
        _dataByUniqueId[member.ReferenceID.ToString()] = componentData;
        return componentData;
    }

    public ComponentDataResult AddMember(ISyncMember member)
    {
        bool createdEntry;
        return new ComponentDataResult(RegisterMember(member, out createdEntry), (!createdEntry) ? DataFeedItemChange.Updated : DataFeedItemChange.Added);
    }

    public ComponentDataResult RemoveComponent(Component c)
    {
        if (!_dataByUniqueId.TryGetValue(c.ReferenceID.ToString(), out var value))
        {
            return new ComponentDataResult(null, DataFeedItemChange.Unchanged);
        }
        _dataByUniqueId.Remove(c.ReferenceID.ToString());
        RemoveEntry(value);
        return new ComponentDataResult(value, DataFeedItemChange.Removed);
    }

    private void RemoveEntry(ComponentData data)
    {
        _data.Remove(data);
        _dataByUniqueId.Remove(data.component.ReferenceID.ToString());
    }

    private ComponentData EnsureEntry(Component c, out bool created)
    {
        if (_dataByUniqueId.TryGetValue(c.ReferenceID.ToString(), out var value))
        {
            created = false;
            return value;
        }
        value = new ComponentData(c);
        _data.Add(value);
        _dataByUniqueId.Add(c.ReferenceID.ToString(), value);
        created = true;
        return value;
    }
}