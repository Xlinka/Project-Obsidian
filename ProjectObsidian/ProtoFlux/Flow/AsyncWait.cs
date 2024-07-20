using System;
using System.Threading.Tasks;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Flow
{
    [NodeCategory("Obsidian/Flow")]
    [NodeName("Async Wait", false)]
    public class AsyncWait : AsyncActionNode<ExecutionContext>
    {
        public ValueInput<bool> Condition;
        public ValueInput<float> Timeout;

        public AsyncCall OnStarted;

        public Continuation OnDone;
        public Continuation TimedOut;

        protected override async Task<IOperation> RunAsync(ExecutionContext context)
        {
            await OnStarted.ExecuteAsync(context);

            var timeoutInSeconds = Timeout.Evaluate(context);
            var startTime = DateTime.UtcNow;

            // Initial evaluation of the condition
            if (Condition.Evaluate(context, defaultValue: false))
            {
                return OnDone.Target;
            }

            while (!Condition.Evaluate(context, defaultValue: false))
            {
                if ((DateTime.UtcNow - startTime).TotalSeconds > timeoutInSeconds)
                {
                    return TimedOut.Target;
                }

                if (context.AbortExecution)
                {
                    throw new ExecutionAbortedException(base.Runtime as IExecutionRuntime, this, TimedOut.Target, isAsync: true);
                }

                await default(NextUpdate);
            }

            return OnDone.Target;
        }
    }
}