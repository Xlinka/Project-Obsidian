using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Time
{
    [NodeCategory("Obsidian/Time")]
    [NodeName("Local Stopwatch")]
    public class LocalStopwatch : VoidNode<FrooxEngineContext>
    {
        [ContinuouslyChanging]
        public readonly ValueOutput<float> ElapsedTime;

        [ContinuouslyChanging]
        public readonly ValueOutput<bool> IsRunning;

        [PossibleContinuations(new string[] { "OnStart" })]
        public readonly Operation Start;

        [PossibleContinuations(new string[] { "OnStop" })]
        public readonly Operation Stop;

        [PossibleContinuations(new string[] { "OnReset" })]
        public readonly Operation Reset;

        public Continuation OnStart;
        public Continuation OnStop;
        public Continuation OnReset;

        private double _startTime = -1.0;
        private double _elapsedTime = 0.0;
        private bool _isRunning = false;

        public LocalStopwatch()
        {
            ElapsedTime = new ValueOutput<float>(this);
            IsRunning = new ValueOutput<bool>(this);
            Start = new Operation(this, 0);
            Stop = new Operation(this, 1);
            Reset = new Operation(this, 2);
        }

        protected override void ComputeOutputs(FrooxEngineContext context)
        {
            double currentTime = context.World.Time.WorldTime;

            // Update elapsed time if running
            if (_isRunning)
            {
                _elapsedTime += currentTime - _startTime;
                _startTime = currentTime;  
            }

            // Write outputs
            ElapsedTime.Write((float)_elapsedTime, context);
            IsRunning.Write(_isRunning, context);
        }

        private IOperation DoStart(FrooxEngineContext context)
        {
            _isRunning = true;
            _startTime = context.World.Time.WorldTime;
            return OnStart.Target;
        }

        private IOperation DoStop(FrooxEngineContext context)
        {
            _isRunning = false;
            return OnStop.Target;
        }

        private IOperation DoReset(FrooxEngineContext context)
        {
            _elapsedTime = 0.0;
            _startTime = context.World.Time.WorldTime;
            return OnReset.Target;
        }
    }
}
