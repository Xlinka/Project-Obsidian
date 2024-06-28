using System.Collections.Generic;
using System;
using FrooxEngine;
using Microsoft.CodeAnalysis.Operations;

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

	private string GetUniqueId(Component c)
	{
		return c.ReferenceID.ToString();
	}

	private string GetUniqueId(Type type)
	{
		return type.GetHashCode().ToString();
	}

	private ComponentData RegisterComponent(Component c, out bool createdEntry)
    {
        ComponentData componentData = EnsureEntry(c, out createdEntry);

        if (!createdEntry)
        {
            throw new InvalidOperationException("Component with this ReferenceID has already been added! RefId: " + GetUniqueId(c));
        }

        return componentData;
    }

    public ComponentDataResult AddComponent(Component c)
    {
        bool createdEntry;
        return new ComponentDataResult(RegisterComponent(c, out createdEntry), (!createdEntry) ? DataFeedItemChange.Updated : DataFeedItemChange.Added);
    }

    public ComponentDataResult RemoveComponent(Component c)
    {
        if (!_dataByUniqueId.TryGetValue(GetUniqueId(c), out var value))
        {
            return new ComponentDataResult(null, DataFeedItemChange.Unchanged);
        }
        RemoveEntry(value);
        return new ComponentDataResult(value, DataFeedItemChange.Removed);
    }

	private ComponentData RegisterComponentType(Type type, out bool createdEntry)
	{
		ComponentData componentData = EnsureEntry(type, out createdEntry);

		if (!createdEntry)
		{
			throw new InvalidOperationException("Component with this Type has already been added! Type: " + GetUniqueId(type));
		}

		return componentData;
	}

	public ComponentDataResult AddComponentType(Type type)
	{
		bool createdEntry;
		return new ComponentDataResult(RegisterComponentType(type, out createdEntry), (!createdEntry) ? DataFeedItemChange.Updated : DataFeedItemChange.Added);
	}

	public ComponentDataResult RemoveComponentType(Type type)
	{
		if (!_dataByUniqueId.TryGetValue(GetUniqueId(type), out var value))
		{
			return new ComponentDataResult(null, DataFeedItemChange.Unchanged);
		}
		RemoveEntry(value);
		return new ComponentDataResult(value, DataFeedItemChange.Removed);
	}

	private void RemoveEntry(ComponentData data)
    {
        _data.Remove(data);
        _dataByUniqueId.Remove(data.uniqueId);
    }

    private ComponentData EnsureEntry(Component c, out bool created)
    {
        if (_dataByUniqueId.TryGetValue(GetUniqueId(c), out var value))
        {
            created = false;
            return value;
        }
        value = new ComponentData(c);
		value.uniqueId = GetUniqueId(c);
        _data.Add(value);
        _dataByUniqueId.Add(GetUniqueId(c), value);
        created = true;
        return value;
    }

	private ComponentData EnsureEntry(Type type, out bool created)
	{
		if (_dataByUniqueId.TryGetValue(GetUniqueId(type), out var value))
		{
			created = false;
			return value;
		}
		value = new ComponentData(type);
		value.uniqueId = GetUniqueId(type);
		_data.Add(value);
		_dataByUniqueId.Add(GetUniqueId(type), value);
		created = true;
		return value;
	}
}