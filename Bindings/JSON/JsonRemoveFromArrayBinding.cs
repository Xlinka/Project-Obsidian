using System;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json;
using Newtonsoft.Json.Linq;


[Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/Json" })]
public class JsonRemoveFromArray : FrooxEngine.ProtoFlux.Runtimes.Execution.ObjectFunctionNode<FrooxEngineContext, JObject>
{
    public readonly SyncRef<INodeObjectOutput<JArray>> Array;
    public readonly SyncRef<INodeObjectOutput<int>> Index;

    public override Type NodeType => typeof(JsonRemoveFromArrayNode);

    public JsonRemoveFromArrayNode TypedNodeInstance { get; private set; }

    public override INode NodeInstance => TypedNodeInstance;

    public override int NodeInputCount => base.NodeInputCount + 2;

    public override N Instantiate<N>()
    {
        if (TypedNodeInstance != null)
        {
            throw new InvalidOperationException("Node has already been instantiated");
        }
        JsonRemoveFromArrayNode jsonAddToObjectInstance = (TypedNodeInstance = new JsonRemoveFromArrayNode());
        return jsonAddToObjectInstance as N;
    }

    protected override void AssociateInstanceInternal(INode node)
    {
        if (node is JsonRemoveFromArrayNode typedNodeInstance)
        {
            TypedNodeInstance = typedNodeInstance;
            return;
        }
        throw new ArgumentException("Node instance is not of type " + typeof(JsonRemoveFromArrayNode));
    }

    public override void ClearInstance()
    {
        TypedNodeInstance = null;
    }

    protected override ISyncRef GetInputInternal(ref int index)
    {
        ISyncRef inputInternal = base.GetInputInternal(ref index);
        if (inputInternal != null)
        {
            return inputInternal;
        }
        switch (index)
        {
            case 0:
                return Array;
            case 1:
                return Index;
            default:
                index -= 2;
                return null;
        }
    }
}
