using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math.Constants;

[NodeCategory("Obsidian/Math/Constants")]
[NodeName("Not Generated")]
public class NotGenerated : ValueFunctionNode<FrooxEngineContext, int>
{
    protected override int Compute(FrooxEngineContext context)
    {
        return 7;
    }
}