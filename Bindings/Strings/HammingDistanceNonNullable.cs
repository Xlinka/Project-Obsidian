using System;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Strings;

[Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/String" })]
public class HammingDistanceNonNullable : FrooxEngine.ProtoFlux.Runtimes.Execution.ValueFunctionNode<ExecutionContext, int>
{
    public readonly SyncRef<INodeObjectOutput<string>> String1;
    public readonly SyncRef<INodeObjectOutput<string>> String2;

    public override Type NodeType => typeof(HammingDistanceNonNullableNode);

    public HammingDistanceNonNullableNode TypedNodeInstance { get; private set; }

    public override INode NodeInstance => TypedNodeInstance;

    public override int NodeInputCount => base.NodeInputCount + 2;

    public override N Instantiate<N>()
    {
        if (TypedNodeInstance != null)
        {
            throw new InvalidOperationException("Node has already been instantiated");
        }
        HammingDistanceNonNullableNode hammingDistanceNonNullableNodeInstance = (TypedNodeInstance = new HammingDistanceNonNullableNode());
        return hammingDistanceNonNullableNodeInstance as N;
    }

    protected override void AssociateInstanceInternal(INode node)
    {
        if (node is HammingDistanceNonNullableNode typedNodeInstance)
        {
            TypedNodeInstance = typedNodeInstance;
            return;
        }
        throw new ArgumentException("Node instance is not of type " + typeof(HammingDistanceNonNullableNode));
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
