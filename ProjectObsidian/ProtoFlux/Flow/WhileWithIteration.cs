using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Flow
{
    [NodeCategory("Obsidian/Flow")]
    [NodeName("While With I", false)]
    public class WhileWithIteration : ActionNode<ExecutionContext>
    {
        public ValueInput<bool> Condition;
        public Call LoopStart;
        public Call LoopIteration;
        public Call LoopEnd;
        public readonly ValueOutput<int> i;
        private int iter;

        protected override IOperation Run(ExecutionContext context)
        {
            iter = 0;
            LoopStart.Execute(context);
            while (Condition.Evaluate(context, defaultValue: false))
            {
                iter++;
                i.Write(iter, context);
                if (context.AbortExecution)
                {
                    throw new ExecutionAbortedException(base.Runtime as IExecutionRuntime, this, LoopIteration.Target, isAsync: false);
                }
                LoopIteration.Execute(context);
            }
            return LoopEnd.Target;
        }

        public WhileWithIteration()
        {
            i = new ValueOutput<int>(this);
        }
    }
}