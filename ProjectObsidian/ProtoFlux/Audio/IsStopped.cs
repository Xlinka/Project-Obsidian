using Elements.Core;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Playback
{
    [NodeCategory("Obsidian/Playback")]
    [NodeName("IsStopped", false)]
    public class IsStopped : ValueFunctionNode<ExecutionContext, bool>
    {
        public ObjectInput<IPlayable> Playable;

        protected override bool Compute(ExecutionContext context)
        {
            var target = Playable.Evaluate(context);
            var isPlaying = target != null && target.IsPlaying;
            var isAtStart = MathX.Approximately(target?.NormalizedPosition ?? 0f, 0f);
            return !isPlaying && isAtStart;
        }
    }
}
