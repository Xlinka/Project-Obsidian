using System;
using FrooxEngine;

namespace Obsidian;

[Category(new string[] { "Obsidian/Radiant UI/Data Feeds/Interfaces" })]
public class TypeItemInterface : FeedItemInterface
{
    public readonly SyncRef<IField<System.Type>> Type;

    public readonly SyncRef<IField<bool>> IsGenericType;

    public readonly SyncRef<IField<System.Type>> GenericTypeDefinition;

    public readonly SyncRef<IField<int>> GenericTypesCount;

    public readonly FeedSubTemplate<TypeFeedItem, TypeItemInterface> GenericTypes;

    public override void Set(IDataFeedView view, DataFeedItem item)
    {
        base.Set(view, item);
        if (item is TypeFeedItem typeFeedItem)
        {
            Type.TrySetTarget(typeFeedItem.Type);
            IsGenericType.TrySetTarget(typeFeedItem.IsGenericType);
            GenericTypeDefinition.TrySetTarget(typeFeedItem.GenericTypeDefinition);
            GenericTypesCount.TrySetTarget(typeFeedItem.GenericTypesCount);
            GenericTypes.Set(typeFeedItem.GenericTypes, view);
        }
    }
}