using FrooxEngine.ProtoFlux;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json
{
    [NodeCategory("Obsidian/Json")]
    public class JsonEmptyJArray : ObjectFunctionNode<FrooxEngineContext, JArray>
    {
        protected override JArray Compute(FrooxEngineContext context)
        {
            return new JArray();
        }
    }
}