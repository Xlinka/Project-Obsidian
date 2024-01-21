using System;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Strings;

[Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/String" })]
public class EncodeBase64 : FrooxEngine.ProtoFlux.Runtimes.Execution.ObjectFunctionNode<ExecutionContext, string>
{
    public readonly SyncRef<INodeObjectOutput<string>> Input;

    public override Type NodeType => typeof(EncodeBase64Node);

    public EncodeBase64Node TypedNodeInstance { get; private set; }

    public override INode NodeInstance => TypedNodeInstance;

    public override int NodeInputCount => base.NodeInputCount + 1;

    public override N Instantiate<N>()
    {
        if (TypedNodeInstance != null)
        {
            throw new InvalidOperationException("Node has already been instantiated");
        }
        EncodeBase64Node encodeBase64NodeInstance = (TypedNodeInstance = new EncodeBase64Node());
        return encodeBase64NodeInstance as N;
    }

    protected override void AssociateInstanceInternal(INode node)
    {
        if (node is EncodeBase64Node typedNodeInstance)
        {
            TypedNodeInstance = typedNodeInstance;
            return;
        }
        throw new ArgumentException("Node instance is not of type " + typeof(EncodeBase64Node));
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
        if (index == 0)
        {
            return Input;
        }
        index -= 1;
        return null;
    }
}
