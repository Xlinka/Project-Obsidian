using System;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Physics;

[Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/Math/Physics" })]
public class CentripetalForceCalculation : FrooxEngine.ProtoFlux.Runtimes.Execution.ValueFunctionNode<ExecutionContext, float>
{
    public readonly SyncRef<INodeValueOutput<float>> Mass;
    public readonly SyncRef<INodeValueOutput<float>> Velocity;
    public readonly SyncRef<INodeValueOutput<float>> Radius;

    public override Type NodeType => typeof(CentripetalForceCalculation);

    public CentripetalForceCalculationNode TypedNodeInstance { get; private set; }

    public override INode NodeInstance => TypedNodeInstance;

    public override int NodeInputCount => base.NodeInputCount + 3;

    public override TN Instantiate<TN>()
    {
        if (TypedNodeInstance != null)
            throw new InvalidOperationException("Node has already been instantiated");
        var instance = (TypedNodeInstance = new CentripetalForceCalculationNode());
        return instance as TN;
    }

    protected override void AssociateInstanceInternal(INode node)
    {
        if (node is not CentripetalForceCalculationNode typedNodeInstance)
            throw new ArgumentException("Node instance is not of type " + typeof(CentripetalForceCalculationNode));
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
                return Mass;
            case 1:
                return Velocity;
            case 2:
                return Radius;
            default:
                index -= 3;
                return null;
        }
    }
}
