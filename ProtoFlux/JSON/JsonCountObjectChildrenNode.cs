﻿using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Json
{
    [NodeCategory("ProtoFlux/Obsidian/Json")]
    public class JsonCountObjectChildrenNode : ValueFunctionNode<ExecutionContext, int>
    {
        public readonly ObjectInput<JObject> Input;

        protected override int Compute(ExecutionContext context)
        {
            var input = Input.Evaluate(context);
            return input?.Count ?? -1;
        }
    }
}
