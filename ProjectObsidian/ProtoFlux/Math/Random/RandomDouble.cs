using Elements.Core;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Actions;

[NodeCategory("ProtoFlux/Obsidian/Math/Random")]
[NodeName("Random Double")]
[ContinuouslyChanging]
public class RandomDouble : ValueFunctionNode<ExecutionContext, double>
{
    public ValueInput<double> Min;
    public ValueInput<double> Max;

    protected override double Compute(ExecutionContext context)
    {
        var min = Min.Evaluate(context);
        var max = Max.Evaluate(context);
        if (min > max)
        {
            var num1 = max;
            var num2 = min;
            min = num1;
            max = num2;
        }

        return min + RandomX.Double * (max - min);
    }
}
