using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Core;
using FrooxEngine;
using SkyFrost.Base;

namespace Obsidian;

[Category(new string[] { "Obsidian/Radiant UI/Data Feeds/Feeds" })]
public class ComponentsDataFeed : Component, IDataFeedComponent, IDataFeed, IWorldElement
{
    private Dictionary<SearchPhraseFeedUpdateHandler, ComponentsDataFeedData> _updateHandlers = new Dictionary<SearchPhraseFeedUpdateHandler, ComponentsDataFeedData>();

    public bool SupportsBackgroundQuerying => true;

    public readonly SyncRef<Slot> TargetSlot;

    public readonly Sync<bool> IncludeChildrenSlots;

    private Slot _lastSlot = null;

    private static HashSet<Type> _componentTypes = new();

    private static bool SearchStringValid(string str)
    {
        return !string.IsNullOrWhiteSpace(str) && str.Length >= 3;
    }

    private void AddComponent(Component c)
    {
        // If local elements are written to synced fields it can cause exceptions and crashes
        if (c.IsLocalElement) return;
        foreach (KeyValuePair<SearchPhraseFeedUpdateHandler, ComponentsDataFeedData> updateHandler in _updateHandlers)
        {
            var result = updateHandler.Value.AddComponent(c);
            foreach (ISyncMember syncMember in result.data.component.SyncMembers)
            {
                if (FilterMember(syncMember))
                {
                    result.data.AddMember(syncMember);
                }
            }
            ProcessUpdate(updateHandler.Key, result.data);
        }
    }

    private void RemoveComponent(Component c)
    {
        foreach (KeyValuePair<SearchPhraseFeedUpdateHandler, ComponentsDataFeedData> updateHandler in _updateHandlers)
        {
            var result = updateHandler.Value.RemoveComponent(c);
            result.data.ClearSubmitted();
            updateHandler.Key.handler(ToItem(result.data), DataFeedItemChange.Removed);
        }
    }

    private void Update()
    {
        foreach (KeyValuePair<SearchPhraseFeedUpdateHandler, ComponentsDataFeedData> updateHandler in _updateHandlers)
        {
            updateHandler.Key.handler(null, DataFeedItemChange.PathItemsInvalidated);
        }
    }

    private bool FilterMember(ISyncMember member)
    {
        if (member.IsLocalElement) return false;
        return true;
    }

    private void ProcessUpdate(SearchPhraseFeedUpdateHandler handler, ComponentData data)
    {
        bool flag = true;
        if (!string.IsNullOrEmpty(handler.searchPhrase))
        {
            List<string> optionalTerms = Pool.BorrowList<string>();
            List<string> requiredTerms = Pool.BorrowList<string>();
            List<string> excludedTerms = Pool.BorrowList<string>();
            SearchQueryParser.Parse(handler.searchPhrase, optionalTerms, requiredTerms, excludedTerms);
            if (!data.MatchesSearchParameters(optionalTerms, requiredTerms, excludedTerms))
            {
                flag = false;
            }
            Pool.Return(ref optionalTerms);
            Pool.Return(ref requiredTerms);
            Pool.Return(ref excludedTerms);
        }
        if (!flag)
        {
            if (data.Submitted)
            {
                data.ClearSubmitted();
                handler.handler(ToItem(data), DataFeedItemChange.Removed);
            }
            return;
        }
        data.MarkSubmitted();
        DataFeedItem item = ToItem(data);
        handler.handler(item, DataFeedItemChange.Added);
    }

    private void Subscribe(Slot s)
    {
        s.ComponentAdded += AddComponent;
        s.ComponentRemoved += RemoveComponent;
    }

    private void Unsubscribe(Slot s)
    {
        s.ComponentAdded -= AddComponent;
        s.ComponentRemoved -= RemoveComponent;
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        _lastSlot = TargetSlot.Target;
    }

    protected override void OnChanges()
    {
        base.OnChanges();
        if (!TargetSlot.WasChanged && !IncludeChildrenSlots.WasChanged)
        {
            return;
        }
        if (TargetSlot.WasChanged)
        {
            if (_lastSlot != null)
            {
                Unsubscribe(_lastSlot);
                if (IncludeChildrenSlots)
                {
                    _lastSlot.ForeachChild(childSlot => Unsubscribe(childSlot));
                }
            }
            if (TargetSlot.Target is Slot slot)
            {
                Subscribe(slot);
                if (IncludeChildrenSlots)
                {
                    TargetSlot.Target.ForeachChild(childSlot => Subscribe(childSlot));
                }
                _lastSlot = slot;
            }
            else
            {
                _lastSlot = null;
            }
        }
        if (IncludeChildrenSlots.WasChanged && TargetSlot.Target != null)
        {
            if (IncludeChildrenSlots.Value)
            {
                TargetSlot.Target.ForeachChild(childSlot => Subscribe(childSlot));
            }
            else if (!IncludeChildrenSlots.Value)
            {
                TargetSlot.Target.ForeachChild(childSlot => Unsubscribe(childSlot));
            }
        }
        TargetSlot.WasChanged = false;
        IncludeChildrenSlots.WasChanged = false;
        Update();
    }

    protected override void OnPrepareDestroy()
    {
        base.OnPrepareDestroy();
        if (TargetSlot.Target != null)
        {
            Unsubscribe(TargetSlot.Target);
            if (IncludeChildrenSlots)
            {
                TargetSlot.Target.ForeachChild(childSlot => Unsubscribe(childSlot));
            }
        }
    }

    private void GetAllTypes(HashSet<Type> allTypes, CategoryNode<Type> categoryNode)
    {
        foreach (var elem in categoryNode.Elements)
        {
            allTypes.Add(elem);
        }
        foreach (var subCat in categoryNode.Subcategories)
        {
            GetAllTypes(allTypes, subCat);
        }
    }

    private IEnumerable<Type> EnumerateAllTypes(CategoryNode<Type> categoryNode)
    {
        foreach (var elem in categoryNode.Elements)
        {
            yield return elem;
        }
        foreach (var subCat in categoryNode.Subcategories)
        {
            foreach(var elem2 in EnumerateAllTypes(subCat))
            {
                yield return elem2;
            }
        }
    }

    public async IAsyncEnumerable<DataFeedItem> Enumerate(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string searchPhrase, object viewData)
    {
        if (TargetSlot.Target != null && (path != null && path.Count > 0))
        {
            yield break;
        }
        if (TargetSlot.Target == null && (path == null || path.Count == 0) && !SearchStringValid(searchPhrase))
        {
            yield break;
        }
        if (groupKeys != null && groupKeys.Count > 0)
        {
            yield break;
        }

        ComponentsDataFeedData componentDataFeedData = (ComponentsDataFeedData)viewData;
        componentDataFeedData.Clear();
        searchPhrase = searchPhrase?.Trim();

        if (TargetSlot.Target == null)
        {
            var lib = WorkerInitializer.ComponentLibrary;
            if (path != null && path.Count > 0)
            {
                var catNode = lib;
                foreach (var str in path)
                {
                    var subCat = catNode.Subcategories.FirstOrDefault(x => x.Name == str);
                    if (subCat != null)
                    {
                        catNode = subCat;
                    }
                    else
                    {
                        yield break;
                    }
                }
                if (SearchStringValid(searchPhrase))
                {
                    foreach (var elem in EnumerateAllTypes(catNode))
                    {
                        componentDataFeedData.AddComponentType(elem);
                    }
                }
                else
                {
                    foreach (var elem in catNode.Elements)
                    {
                        componentDataFeedData.AddComponentType(elem);
                    }
                }
            }
            else
            {
                if (_componentTypes.Count == 0)
                {
                    GetAllTypes(_componentTypes, lib);
                }
                foreach (var elem in _componentTypes)
                {
                    componentDataFeedData.AddComponentType(elem);
                }
            }
        }
        else
        {
            var components = IncludeChildrenSlots ? TargetSlot.Target.GetComponentsInChildren<Component>() : TargetSlot.Target.GetComponents<Component>();
            foreach (Component allComponent in components)
            {
                // If local elements are written to synced fields it can cause exceptions and crashes
                if (allComponent.IsLocalElement) continue;
                var result = componentDataFeedData.AddComponent(allComponent);
                foreach (ISyncMember syncMember in allComponent.SyncMembers)
                {
                    if (FilterMember(syncMember))
                    {
                        result.data.AddMember(syncMember);
                    }
                }
            }
        }
        
        List<string> optionalTerms = Pool.BorrowList<string>();
        List<string> requiredTerms = Pool.BorrowList<string>();
        List<string> excludedTerms = Pool.BorrowList<string>();
        SearchQueryParser.Parse(searchPhrase, optionalTerms, requiredTerms, excludedTerms);
        foreach (ComponentData componentData in componentDataFeedData.ComponentData)
        {
            if (componentData.MatchesSearchParameters(optionalTerms, requiredTerms, excludedTerms))
            {
                componentData.MarkSubmitted();
                yield return ToItem(componentData);
            }
        }
        Pool.Return(ref optionalTerms);
        Pool.Return(ref requiredTerms);
        Pool.Return(ref excludedTerms);
    }

    private DataFeedItem ToItem(ComponentData data)
    {
        return new ComponentDataFeedItem(data);
    }

    public void ListenToUpdates(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string searchPhrase, DataFeedUpdateHandler handler, object viewData)
    {
        if ((path == null || path.Count <= 0) && (groupKeys == null || groupKeys.Count <= 0))
        {
            var data = (ComponentsDataFeedData)viewData;
            _updateHandlers.Add(new SearchPhraseFeedUpdateHandler(handler, searchPhrase?.Trim()), data);
        }
    }

    public void UnregisterListener(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string searchPhrase, DataFeedUpdateHandler handler)
    {
        if ((path == null || path.Count <= 0) && (groupKeys == null || groupKeys.Count <= 0))
        {
            _updateHandlers.Remove(new SearchPhraseFeedUpdateHandler(handler, searchPhrase?.Trim()));
        }
    }

    public LocaleString PathSegmentName(string segment, int depth)
    {
        return null;
    }

    public object RegisterViewData()
    {
        return new ComponentsDataFeedData();
    }

    public void UnregisterViewData(object data)
    {
    }

    protected override void OnDispose()
    {
        _updateHandlers.Clear();
        base.OnDispose();
    }
}