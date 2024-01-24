using System;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Physics;

[Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/Math/Physics" })]
public class Refraction : FrooxEngine.ProtoFlux.Runtimes.Execution.ValueFunctionNode<ExecutionContext, float>
{
    public readonly SyncRef<INodeValueOutput<float>> RefractiveIndex1;
    public readonly SyncRef<INodeValueOutput<float>> RefractiveIndex2;
    public readonly SyncRef<INodeValueOutput<float>> AngleOfIncidence;

    public override Type NodeType => typeof(RefractionNode);

    public RefractionNode TypedNodeInstance { get; private set; }

    public override INode NodeInstance => TypedNodeInstance;

    public override int NodeInputCount => 3;

    public override N Instantiate<N>()
    {
        if (TypedNodeInstance != null)
        {
            throw new InvalidOperationException("Node has already been instantiated");
        }
        TypedNodeInstance = new RefractionNode();
        return TypedNodeInstance as N;
    }

    protected override void AssociateInstanceInternal(INode node)
    {
        TypedNodeInstance = node as RefractionNode ?? throw new ArgumentException("Node instance is not of type RefractionNode");
    }

    public override void ClearInstance()
    {
        TypedNodeInstance = null;
    }

    protected override ISyncRef GetInputInternal(ref int index)
    {
        switch (index)
        {
            case 0: return RefractiveIndex1;
            case 1: return RefractiveIndex2;
            case 2: return AngleOfIncidence;
            default: index -= 3; return null;
        }
    }
}
