using ProtoFlux.Core;
using System.Threading.Tasks;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Flow
{
    [NodeCategory("Obsidian/Flow")]
    [NodeName("AsyncWhileWithI", false)]
    public class AsyncWhileWithIteration : AsyncActionNode<ExecutionContext>
    {
        public ValueInput<bool> Condition;
        public AsyncCall LoopStart;
        public AsyncCall LoopIteration;
        public Continuation LoopEnd;
        public readonly ValueOutput<int> i;
        private int iter;

        protected override async Task<IOperation> RunAsync(ExecutionContext context)
        {
            iter = 0;
            await LoopStart.ExecuteAsync(context);
            while (Condition.Evaluate(context, defaultValue: false))
            {
                iter++;
                i.Write(iter, context);
                if (context.AbortExecution)
                {
                    throw new ExecutionAbortedException(base.Runtime as IExecutionRuntime, this, LoopIteration.Target, isAsync: true);
                }
                await LoopIteration.ExecuteAsync(context);
            }
            return LoopEnd.Target;
        }

        public AsyncWhileWithIteration()
        {
            i = new ValueOutput<int>(this);
        }
    }
}