using System;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Strings;

[Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/String" })]
public class HammingDistanceBinding : FrooxEngine.ProtoFlux.Runtimes.Execution.ObjectFunctionNode<FrooxEngineContext, int?>
{
    public readonly SyncRef<INodeObjectOutput<string>> String1;
    public readonly SyncRef<INodeObjectOutput<string>> String2;

    public override Type NodeType => typeof(HammingDistanceNode);

    public HammingDistanceNode TypedNodeInstance { get; private set; }

    public override INode NodeInstance => TypedNodeInstance;

    public override int NodeInputCount => base.NodeInputCount + 2;

    public override N Instantiate<N>()
    {
        if (TypedNodeInstance != null)
        {
            throw new InvalidOperationException("Node has already been instantiated");
        }
        HammingDistanceNode hammingDistanceNodeInstance = (TypedNodeInstance = new HammingDistanceNode());
        return hammingDistanceNodeInstance as N;
    }

    protected override void AssociateInstanceInternal(INode node)
    {
        if (node is HammingDistanceNode typedNodeInstance)
        {
            TypedNodeInstance = typedNodeInstance;
            return;
        }
        throw new ArgumentException("Node instance is not of type " + typeof(HammingDistanceNode));
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
                return String1;
            case 1:
                return String2;
            default:
                index -= 2;
                return null;
        }
    }
}
