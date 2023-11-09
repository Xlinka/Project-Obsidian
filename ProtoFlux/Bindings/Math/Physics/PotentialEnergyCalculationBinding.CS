using FrooxEngine.ProtoFlux;
using ProtoFlux.Runtimes.Execution;
using System;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Physics
{
    [Category("Obsidian/Math/Physics")]
    public class PotentialEnergyCalculationBinding : FrooxEngine.ProtoFlux.Runtimes.Execution.NodeBinding<PotentialEnergyCalculationNode>
    {
        public readonly SyncRef<INodeValueOutput<float>> Mass;
        public readonly SyncRef<INodeValueOutput<float>> Height;

        public override Type NodeType => typeof(PotentialEnergyCalculationNode);

        public PotentialEnergyCalculationNode TypedNodeInstance { get; private set; }

        public override INode NodeInstance => TypedNodeInstance;

        public override int NodeInputCount => base.NodeInputCount + 2;

        public override TN Instantiate<TN>()
        {
            try
            {
                if (TypedNodeInstance != null)
                    throw new InvalidOperationException("Node has already been instantiated");

                var instance = (TypedNodeInstance = new PotentialEnergyCalculationNode());
                return instance as TN;
            }
            catch (Exception ex)
            {
                UniLog.Log($"Error in PotentialEnergyCalculationBinding.Instantiate: {ex.Message}");
                throw;
            }
        }

        protected override void AssociateInstanceInternal(INode node)
        {
            try
            {
                if (node is not PotentialEnergyCalculationNode typedNodeInstance)
                    throw new ArgumentException("Node instance is not of type " + typeof(PotentialEnergyCalculationNode));

                TypedNodeInstance = typedNodeInstance;
            }
            catch (Exception ex)
            {
                UniLog.Log($"Error in PotentialEnergyCalculationBinding.AssociateInstanceInternal: {ex.Message}");
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
                    return Mass;
                case 1:
                    return Height;
                default:
                    index -= 2;
                    return null;
            }
        }
    }
}
