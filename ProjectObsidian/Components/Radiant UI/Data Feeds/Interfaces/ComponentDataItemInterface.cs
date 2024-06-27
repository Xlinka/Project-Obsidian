using FrooxEngine;

namespace Obsidian;

[Category(new string[] { "Obsidian/Radiant UI/Data Feeds/Interfaces" })]
public class ComponentDataItemInterface : FeedItemInterface
{
    public readonly SyncRef<SyncRef<Component>> Component;

    public readonly SyncRef<IField<int>> MemberCount;

    public readonly FeedSubTemplate<DataFeedEntity<ISyncMember>, FeedEntityInterface<ISyncMember>> Members;

    public override void Set(IDataFeedView view, DataFeedItem item)
    {
        base.Set(view, item);
        if (item is ComponentDataFeedItem componentDataFeedItem)
        {
            Component.TrySetTarget(componentDataFeedItem.Data.component);
            MemberCount.TrySetTarget(componentDataFeedItem.Data.MemberCount);
            Members.Set(componentDataFeedItem.Members, view);
        }
    }
}