using FrooxEngine.ProtoFlux;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json
{
    [NodeCategory("Obsidian/Json")]
    public class JsonEmptyJObject : ObjectFunctionNode<FrooxEngineContext, JObject>
    {
        protected override JObject Compute(FrooxEngineContext context)
        {
            return new JObject();
        }
    }
}