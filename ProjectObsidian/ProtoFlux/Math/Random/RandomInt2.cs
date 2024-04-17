using Elements.Core;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

[NodeCategory("ProtoFlux/Obsidian/Math/Random")]
[NodeName("Random Int2")]
public class RandomInt2 : ValueFunctionNode<ExecutionContext, int2>
{
    public ValueInput<int2> Min;
    public ValueInput<int2> Max;

    protected override int2 Compute(ExecutionContext context)
    {
        int2 min = Min.Evaluate(context,int2.Zero);
        int2 max = Max.Evaluate(context,int2.One);

        return new int2(RandomX.Range(min.x, max.x), RandomX.Range(min.y, max.y));
    }
}
