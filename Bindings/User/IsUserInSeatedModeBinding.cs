using System;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux.Status;

[Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/Locomotion" })]
public class IsUserInSeatedModeBinding : FrooxEngine.ProtoFlux.Runtimes.Execution.ValueFunctionNode<ExecutionContext, bool>
{
    public readonly SyncRef<INodeObjectOutput<User>> User;

    public override Type NodeType => typeof(IsUserInSeatedModeNode);

    public IsUserInSeatedModeNode TypedNodeInstance { get; private set; }

    public override INode NodeInstance => TypedNodeInstance;

    public override int NodeInputCount => base.NodeInputCount + 1;

    public override N Instantiate<N>()
    {
        if (TypedNodeInstance != null)
        {
            throw new InvalidOperationException("Node has already been instantiated");
        }
        IsUserInSeatedModeNode isUserInSeatedModeNodeInstance = (TypedNodeInstance = new IsUserInSeatedModeNode());
        return isUserInSeatedModeNodeInstance as N;
    }

    protected override void AssociateInstanceInternal(INode node)
    {
        if (node is IsUserInSeatedModeNode typedNodeInstance)
        {
            TypedNodeInstance = typedNodeInstance;
            return;
        }
        throw new ArgumentException("Node instance is not of type " + typeof(IsUserInSeatedModeNode));
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
            return User;
        }
        index -= 1;
        return null;
    }
}