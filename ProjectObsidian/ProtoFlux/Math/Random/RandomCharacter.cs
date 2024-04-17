using Elements.Core;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Actions;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Random;

[NodeCategory("Obsidian/Math/Random")]
[NodeName("Random Character")]
[ContinuouslyChanging]
public class RandomCharacter : ValueFunctionNode<FrooxEngineContext, char>
{
    public ValueInput<int> Start;
    public ValueInput<int> End;
    public ObjectInput<string> String;

    protected override char Compute(FrooxEngineContext context)
    {
        var str = String.Evaluate(context);
        var start = MathX.Clamp(Start.Evaluate(context), 0, str.Length);
        var end = MathX.Clamp(End.Evaluate(context,str.Length), start, str.Length);
        return str[RandomX.Range(start, end)];
    }
}
