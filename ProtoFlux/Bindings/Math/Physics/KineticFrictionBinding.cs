using System;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Physics;

[Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/Math/Physics" })]
public class KineticFrictionCalculation : FrooxEngine.ProtoFlux.Runtimes.Execution.ValueFunctionNode<ExecutionContext, float3>
{
    public readonly SyncRef<INodeValueOutput<float3>> NormalForce;
    public readonly SyncRef<INodeValueOutput<float>> KineticFrictionCoefficient;

    public override Type NodeType => typeof(KineticFrictionNode);

    public KineticFrictionNode TypedNodeInstance { get; private set; }

    public override INode NodeInstance => TypedNodeInstance;

    public override int NodeInputCount => base.NodeInputCount + 2;

    public override TN Instantiate<TN>()
    {
        if (TypedNodeInstance != null)
            throw new InvalidOperationException("Node has already been instantiated");
        var instance = (TypedNodeInstance = new KineticFrictionNode());
        return instance as TN;
    }

    protected override void AssociateInstanceInternal(INode node)
    {
        if (node is not KineticFrictionNode typedNodeInstance)
            throw new ArgumentException("Node instance is not of type " + typeof(KineticFrictionNode));
        TypedNodeInstance = typedNodeInstance;
    }

    public override void ClearInstance() => TypedNodeInstance = null;

    protected override ISyncRef GetInputInternal(ref int index)
    {
        var inputInternal = base.GetInputInternal(ref index);
        if (inputInternal != null)
        {
            return inputInternal;
        }
        switch (index)
        {
            case 0:
                return NormalForce;
            case 1:
                return KineticFrictionCoefficient;
            default:
                index -= 2;
                return null;
        }
    }
}
