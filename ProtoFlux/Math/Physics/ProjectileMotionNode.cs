using FrooxEngine;
using FrooxEngine.ProtoFlux;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Physics
{
    [NodeCategory("Obsidian/Math/Physics")]
    public class ProjectileMotionNode : ValueFunctionNode<ExecutionContext, float3>
    {
        public ValueInput<float> InitialVelocity;
        public ValueInput<float> LaunchAngle;
        public ValueInput<float> Gravity;

        protected override float3 Compute(ExecutionContext context)
        {
            float initialVelocity = InitialVelocity.Evaluate(context);
            float launchAngle = LaunchAngle.Evaluate(context);
            float gravity = Gravity.Evaluate(context);

            // Convert launch angle to radians
            float launchAngleRad = launchAngle * Mathf.Deg2Rad;

            // Calculate horizontal and vertical components of velocity
            float horizontalVelocity = initialVelocity * Mathf.Cos(launchAngleRad);
            float verticalVelocity = initialVelocity * Mathf.Sin(launchAngleRad);

            // Calculate time of flight
            float timeOfFlight = (2 * verticalVelocity) / gravity;

            // Calculate horizontal position at time of flight
            float horizontalPosition = horizontalVelocity * timeOfFlight;

            // Calculate maximum height reached
            float maxHeight = (verticalVelocity * verticalVelocity) / (2 * gravity);

            return new float3(horizontalPosition, maxHeight, timeOfFlight);
        }
    }
}
