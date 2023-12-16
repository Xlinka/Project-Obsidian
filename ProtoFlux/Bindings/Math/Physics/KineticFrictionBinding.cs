using System;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Physics;

[Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/Math/Physics" })]
public class KineticFrictionNodeBinding : FrooxEngine.ProtoFlux.Runtimes.Execution.ValueFunctionNode<ExecutionContext, float3>
{
    public readonly SyncRef<INodeValueOutput<float3>> NormalForce;
    public readonly SyncRef<INodeValueOutput<float>> KineticFrictionCoefficient;

    public override Type NodeType => typeof(KineticFrictionNode);

    public KineticFrictionNode TypedNodeInstance { get; private set; }

    public override INode NodeInstance => TypedNodeInstance;

    public override int NodeInputCount => 2;

    public override N Instantiate<N>()
    {
        if (TypedNodeInstance != null)
        {
            throw new InvalidOperationException("Node has already been instantiated");
        }
        TypedNodeInstance = new KineticFrictionNode();
        return TypedNodeInstance as N;
    }

    protected override void AssociateInstanceInternal(INode node)
    {
        TypedNodeInstance = node as KineticFrictionNode ?? throw new ArgumentException("Node instance is not of type KineticFrictionNode");
    }

    public override void ClearInstance()
    {
        TypedNodeInstance = null;
    }

    protected override ISyncRef GetInputInternal(ref int index)
    {
        switch (index)
        {
            case 0: return NormalForce;
            case 1: return KineticFrictionCoefficient;
            default: index -= 2; return null;
        }
    }
}
