using Elements.Core;
using FrooxEngine;

namespace Obsidian;

public class ComponentDataFeedItem : DataFeedItem
{
    public SlimList<DataFeedEntity<ISyncMember>> Members;

    public ComponentData Data { get; private set; }

    public ComponentDataFeedItem(ComponentData componentData)
    {
        InitBase(componentData.uniqueId, null, null, componentData.MainName);
        Data = componentData;
        foreach (ISyncMember member in componentData.members)
        {
            DataFeedEntity<ISyncMember> dataFeedEntity = new DataFeedEntity<ISyncMember>();
            dataFeedEntity.InitBase(member.ReferenceID.ToString(), null, null, member.Name);
            dataFeedEntity.InitEntity(member);
            Members.Add(dataFeedEntity);
        }
    }
}