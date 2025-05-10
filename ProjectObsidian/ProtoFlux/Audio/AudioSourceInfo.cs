using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Audio
{
    [NodeCategory("Obsidian/Audio")]
    public class AudioSourceInfo : VoidNode<FrooxEngineContext>
    {
        public readonly ObjectInput<IWorldAudioDataSource> Source;

        [ContinuouslyChanging]
        public readonly ValueOutput<bool> IsActive;

        public readonly ValueOutput<int> ChannelCount;

        protected override void ComputeOutputs(FrooxEngineContext context)
        {
            IWorldAudioDataSource source = Source.Evaluate(context);
            if (source != null)
            {
                IsActive.Write(source.IsActive, context);
                ChannelCount.Write(source.ChannelCount, context);
            }
            else
            {
                IsActive.Write(false, context);
                ChannelCount.Write(0, context);
            }
        }

        public AudioSourceInfo()
        {
            IsActive = new ValueOutput<bool>(this);
            ChannelCount = new ValueOutput<int>(this);
        }
    }
}