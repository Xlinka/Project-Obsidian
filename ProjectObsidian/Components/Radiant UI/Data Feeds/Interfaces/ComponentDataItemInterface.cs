using System;
using FrooxEngine;

namespace Obsidian;

[Category(new string[] { "Obsidian/Radiant UI/Data Feeds/Interfaces" })]
public class ComponentDataItemInterface : FeedItemInterface
{
    public readonly SyncRef<SyncRef<Component>> Component;

    public readonly SyncRef<IField<System.Type>> Type;

    public readonly SyncRef<IField<bool>> IsGenericType;

    public readonly SyncRef<IField<System.Type>> GenericTypeDefinition;

    public readonly SyncRef<IField<int>> MemberCount;

    public readonly FeedSubTemplate<DataFeedEntity<ISyncMember>, FeedEntityInterface<ISyncMember>> Members;

    public override void Set(IDataFeedView view, DataFeedItem item)
    {
        base.Set(view, item);
        if (item is ComponentDataFeedItem componentDataFeedItem)
        {
            Component.TrySetTarget(componentDataFeedItem.Data.component);
            Type.TrySetTarget(componentDataFeedItem.Data.ComponentType);
            IsGenericType.TrySetTarget(componentDataFeedItem.Data.IsGenericType);
            GenericTypeDefinition.TrySetTarget(componentDataFeedItem.Data.GenericTypeDefinition);
            MemberCount.TrySetTarget(componentDataFeedItem.Data.MemberCount);
            Members.Set(componentDataFeedItem.Members, view);
        }
    }
}