using System;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Physics;

[Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/Math/Physics" })]
public class DragCalculation : FrooxEngine.ProtoFlux.Runtimes.Execution.ValueFunctionNode<ExecutionContext, float3>
{
    public readonly SyncRef<INodeValueOutput<float>> FluidDensity;
    public readonly SyncRef<INodeValueOutput<float3>> ObjectVelocity;
    public readonly SyncRef<INodeValueOutput<float>> DragCoefficient;
    public readonly SyncRef<INodeValueOutput<float>> CrossSectionalArea;

    public override Type NodeType => typeof(DragCalculationNode);

    public DragCalculationNode TypedNodeInstance { get; private set; }

    public override INode NodeInstance => TypedNodeInstance;

    public override int NodeInputCount => 4;

    public override N Instantiate<N>()
    {
        if (TypedNodeInstance != null)
        {
            throw new InvalidOperationException("Node has already been instantiated");
        }
        TypedNodeInstance = new DragCalculationNode();
        return TypedNodeInstance as N;
    }

    protected override void AssociateInstanceInternal(INode node)
    {
        TypedNodeInstance = node as DragCalculationNode ?? throw new ArgumentException("Node instance is not of type DragCalculationNode");
    }

    public override void ClearInstance()
    {
        TypedNodeInstance = null;
    }

    protected override ISyncRef GetInputInternal(ref int index)
    {
        switch (index)
        {
            case 0: return FluidDensity;
            case 1: return ObjectVelocity;
            case 2: return DragCoefficient;
            case 3: return CrossSectionalArea;
            default: index -= 4; return null;
        }
    }
}
