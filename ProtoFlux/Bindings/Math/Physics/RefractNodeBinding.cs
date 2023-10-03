using System;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Physics;

[Category(new string[] { "ProtoFlux/Runtimes/Execution/Nodes/Obsidian/Math/Physics" })]
public class RefractionCalculation : FrooxEngine.ProtoFlux.Runtimes.Execution.ValueFunctionNode<ExecutionContext, float>
{
    public readonly SyncRef<INodeValueOutput<float>> RefractiveIndex1;
    public readonly SyncRef<INodeValueOutput<float>> RefractiveIndex2;
    public readonly SyncRef<INodeValueOutput<float>> AngleOfIncidence;

    public override Type NodeType => typeof(RefractionNode);

    public RefractionNode TypedNodeInstance { get; private set; }

    public override INode NodeInstance => TypedNodeInstance;

    public override int NodeInputCount => base.NodeInputCount + 3;

    public override TN Instantiate<TN>()
    {
        try
        {
            if (TypedNodeInstance != null)
                throw new InvalidOperationException("Node has already been instantiated");
            var refractionInstance = (TypedNodeInstance = new RefractionNode());
            return refractionInstance as TN;
        }
        catch (Exception ex)
        {
            UniLog.Log($"Error in RefractionCalculation.Instantiate: {ex.Message}");
            throw;
        }
    }

    protected override void AssociateInstanceInternal(INode node)
    {
        try
        {
            if (node is not RefractionNode typedNodeInstance)
                throw new ArgumentException("Node instance is not of type " + typeof(RefractionNode));
            TypedNodeInstance = typedNodeInstance;
        }
        catch (Exception ex)
        {
            UniLog.Log($"Error in RefractionCalculation.AssociateInstanceInternal: {ex.Message}");
            throw;
        }
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
                return RefractiveIndex1;
            case 1:
                return RefractiveIndex2;
            case 2:
                return AngleOfIncidence;
            default:
                index -= 3;
                return null;
        }
    }
}
