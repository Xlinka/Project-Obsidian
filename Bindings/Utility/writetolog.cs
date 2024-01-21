using System;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.ProtoFlux.Runtimes.Execution;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Utility;


[Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/Utility" })]
public class WriteToLogBinding : AsyncActionNode<FrooxEngineContext>
{
    public readonly SyncRef<INodeObjectOutput<string>> Value;
    public readonly SyncRef<INodeValueOutput<LogSeverity>> Severity;
    public readonly SyncRef<INodeObjectOutput<string>> Tag;
    public readonly SyncRef<INodeObjectOutput<User>> HandlingUser;

    public readonly SyncRef<INodeOperation> OnWriteStart;
    public readonly SyncRef<INodeOperation> OnWriteComplete;

    public override Type NodeType => typeof(WriteToLogNode);

    public WriteToLogNode TypedNodeInstance { get; private set; }

    public override INode NodeInstance => TypedNodeInstance;

    public override int NodeInputCount => base.NodeInputCount + 4;

    public override int NodeImpulseCount => base.NodeImpulseCount + 2;

    public override N Instantiate<N>()
    {
        if (TypedNodeInstance != null)
        {
            throw new InvalidOperationException("Node has already been instantiated");
        }
        WriteToLogNode writeToLogNode = (TypedNodeInstance = new WriteToLogNode());
        return writeToLogNode as N;
    }

    protected override void AssociateInstanceInternal(INode node)
    {
        if (node is WriteToLogNode typedNodeInstance)
        {
            TypedNodeInstance = typedNodeInstance;
            return;
        }
        throw new ArgumentException("Node instance is not of type " + typeof(WriteToLogNode));
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
                return Value;
            case 1:
                return Severity;
            case 2:
                return Tag;
            case 3:
                return HandlingUser;
            default:
                index -= 4;
                return null;
        }
    }

    protected override ISyncRef GetImpulseInternal(ref int index)
    {
        ISyncRef impulseInternal = base.GetImpulseInternal(ref index);
        if (impulseInternal != null)
        {
            return impulseInternal;
        }
        switch (index)
        {
            case 0:
                return OnWriteStart;
            case 1:
                return OnWriteComplete;
            default:
                index -= 2;
                return null;
        }
    }
}
