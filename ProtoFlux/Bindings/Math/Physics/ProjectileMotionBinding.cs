using System;
using FrooxEngine;
using FrooxEngine.ProtoFlux;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Physics
{
    [Category("Obsidian/Math/Physics")]
    public class ProjectileMotionCalculation : FrooxEngine.ProtoFlux.Runtimes.Execution.ValueFunctionNodeBinding<ProjectileMotionNode>
    {
        public readonly SyncRef<INodeValueOutput<float>> InitialVelocity;
        public readonly SyncRef<INodeValueOutput<float>> LaunchAngle;
        public readonly SyncRef<INodeValueOutput<float>> Gravity;

        public override TN Instantiate<TN>()
        {
            try
            {
                if (NodeInstance != null)
                    throw new InvalidOperationException("Node has already been instantiated");
                var projectileMotionInstance = (NodeInstance = new ProjectileMotionNode());
                return projectileMotionInstance as TN;
            }
            catch (Exception ex)
            {
                UniLog.Log($"Error in ProjectileMotionCalculation.Instantiate: {ex.Message}");
                throw;
            }
        }

        protected override void AssociateInstanceInternal(INode node)
        {
            try
            {
                if (node is not ProjectileMotionNode typedNodeInstance)
                    throw new ArgumentException("Node instance is not of type " + typeof(ProjectileMotionNode));
                NodeInstance = typedNodeInstance;
            }
            catch (Exception ex)
            {
                UniLog.Log($"Error in ProjectileMotionCalculation.AssociateInstanceInternal: {ex.Message}");
                throw;
            }
        }

        public override void ClearInstance() => NodeInstance = null;

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
                    return InitialVelocity;
                case 1:
                    return LaunchAngle;
                case 2:
                    return Gravity;
                default:
                    index -= 3;
                    return null;
            }
        }
    }
}
