using System;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Strings;

[Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/String" })]
public class CountSubstring : FrooxEngine.ProtoFlux.Runtimes.Execution.ValueFunctionNode<ExecutionContext, int>
{
    public readonly SyncRef<INodeObjectOutput<string>> String;
    public readonly SyncRef<INodeObjectOutput<string>> Pattern;

    public override Type NodeType => typeof(CountSubstringNode);

    public CountSubstringNode TypedNodeInstance { get; private set; }

    public override INode NodeInstance => TypedNodeInstance;

    public override int NodeInputCount => base.NodeInputCount + 2;

    public override N Instantiate<N>()
    {
        if (TypedNodeInstance != null)
        {
            throw new InvalidOperationException("Node has already been instantiated");
        }
        CountSubstringNode countSubstringNodeInstance = (TypedNodeInstance = new CountSubstringNode());
        return countSubstringNodeInstance as N;
    }

    protected override void AssociateInstanceInternal(INode node)
    {
        if (node is CountSubstringNode typedNodeInstance)
        {
            TypedNodeInstance = typedNodeInstance;
            return;
        }
        throw new ArgumentException("Node instance is not of type " + typeof(CountSubstringNode));
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
                return String;
            case 1:
                return Pattern;
            default:
                index -= 2;
                return null;
        }
    }
}
