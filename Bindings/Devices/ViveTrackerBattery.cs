using System;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.ProtoFlux.Runtimes.Execution;
using ProtoFlux.Core;


[Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/Devices" })]
public class ViveTrackerBattery : VoidNode<FrooxEngineContext>
{
    public readonly SyncRef<INodeObjectOutput<User>> User;

    public readonly SyncRef<INodeValueOutput<BodyNode>> BodyNode;

    public readonly NodeValueOutput<bool> IsActive;

    public readonly NodeValueOutput<float> BatteryLevel;

    public readonly NodeValueOutput<bool> IsBatteryCharging;

    public override Type NodeType => typeof(ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Input.ViveTrackerBattery);

    public ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Input.ViveTrackerBattery TypedNodeInstance { get; private set; }

    public override INode NodeInstance => TypedNodeInstance;

    public override int NodeInputCount => base.NodeInputCount + 2;

    public override int NodeOutputCount => base.NodeOutputCount + 3;

    public override N Instantiate<N>()
    {
        if (TypedNodeInstance != null)
        {
            throw new InvalidOperationException("Node has already been instantiated");
        }
        ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Input.ViveTrackerBattery viveTrackerBattery = (TypedNodeInstance = new ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Input.ViveTrackerBattery());
        return viveTrackerBattery as N;
    }

    protected override void AssociateInstanceInternal(INode node)
    {
        if (node is ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Input.ViveTrackerBattery typedNodeInstance)
        {
            TypedNodeInstance = typedNodeInstance;
            return;
        }
        throw new ArgumentException("Node instance is not of type " + typeof(ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Input.ViveTrackerBattery));
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
                return User;
            case 1:
                return BodyNode;
            default:
                index -= 2;
                return null;
        }
    }

    protected override INodeOutput GetOutputInternal(ref int index)
    {
        INodeOutput outputInternal = base.GetOutputInternal(ref index);
        if (outputInternal != null)
        {
            return outputInternal;
        }
        switch (index)
        {
            case 0:
                return IsActive;
            case 1:
                return BatteryLevel;
            case 2:
                return IsBatteryCharging;
            default:
                index -= 3;
                return null;
        }
    }
}