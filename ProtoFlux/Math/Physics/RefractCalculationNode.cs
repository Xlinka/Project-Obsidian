using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using System;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Physics
{
    [NodeCategory("Obsidian/Math/Physics")]
    public class RefractionNode : ValueFunctionNode<ExecutionContext, float>
    {
        public ValueInput<float> RefractiveIndex1;  
        public ValueInput<float> RefractiveIndex2;  
        public ValueInput<float> AngleOfIncidence;  

        protected override float Compute(ExecutionContext context)
        {
            float n1 = RefractiveIndex1.Evaluate(context);
            float n2 = RefractiveIndex2.Evaluate(context);
            float theta1Rad = AngleOfIncidence.Evaluate(context) * (float)Math.PI / 180.0f;  
            // Calculate using Snell's Law
            float sinTheta2 = n1 * (float)Math.Sin(theta1Rad) / n2;

            // Ensure value is within [-1, 1] due to numerical inaccuracies
            sinTheta2 = Math.Min(Math.Max(sinTheta2, -1.0f), 1.0f);

            float theta2Rad = (float)Math.Asin(sinTheta2);
            return theta2Rad * 180.0f / (float)Math.PI;  
        }
    }
}
