using Elements.Core;
using FrooxEngine;
using System;

namespace Obsidian;

public class TypeFeedItem : DataFeedItem
{
    public Type Type;

    public bool IsGenericType;

    public Type GenericTypeDefinition;

    public SlimList<TypeFeedItem> GenericTypes;

    public int GenericTypesCount;

    public TypeFeedItem(Type type)
    {
        InitBase(type.GetHashCode().ToString(), null, null, type.GetNiceName());
        Type = type;
        IsGenericType = type.IsGenericType;
        GenericTypeDefinition = IsGenericType ? type.GetGenericTypeDefinition() : null;
        int count = 0;
        if (type.IsGenericTypeDefinition)
        {
            foreach (Type genericType in WorkerInitializer.GetCommonGenericTypes(type))
            {
                TypeFeedItem typeFeedItem = new TypeFeedItem(genericType);
                typeFeedItem.InitBase(genericType.GetHashCode().ToString(), null, null, genericType.GetNiceName());
                GenericTypes.Add(typeFeedItem);
                count++;
            }
        }
        GenericTypesCount = count;
    }
}