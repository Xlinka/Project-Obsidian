using System;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Strings;

[Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/String" })]
public class EncodeHMAC : FrooxEngine.ProtoFlux.Runtimes.Execution.ObjectFunctionNode<ExecutionContext, string>
{
    public readonly SyncRef<INodeObjectOutput<string>> Message;
    public readonly SyncRef<INodeObjectOutput<string>> Key;
    public readonly SyncRef<INodeObjectOutput<HashFunction>> HashAlgorithm;

    public override Type NodeType => typeof(HMACNode);

    public HMACNode TypedNodeInstance { get; private set; }

    public override INode NodeInstance => TypedNodeInstance;

    public override int NodeInputCount => base.NodeInputCount + 3;

    public override N Instantiate<N>()
    {
        if (TypedNodeInstance != null)
        {
            throw new InvalidOperationException("Node has already been instantiated");
        }
        HMACNode hmacNodeInstance = (TypedNodeInstance = new HMACNode());
        return hmacNodeInstance as N;
    }

    protected override void AssociateInstanceInternal(INode node)
    {
        if (node is HMACNode typedNodeInstance)
        {
            TypedNodeInstance = typedNodeInstance;
            return;
        }
        throw new ArgumentException("Node instance is not of type " + typeof(HMACNode));
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
                return Message;
            case 1:
                return Key;
            case 2:
                return HashAlgorithm;
            default:
                index -= 3;
                return null;
        }
    }
}
