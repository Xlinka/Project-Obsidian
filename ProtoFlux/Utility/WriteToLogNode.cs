using Elements.Core;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Utility
{
    public enum LogSeverity
    {
        Log,
        Warning,
        Error
    }

    [NodeCategory("Obsidian/Utility/WriteToLog")]
    public class WriteToLogNode : AsyncActionNode<ExecutionContext>
    {
        public readonly ValueInput<string> Value;
        public readonly ValueInput<LogSeverity> Severity;
        public readonly ValueInput<string> Tag;
        public readonly ValueInput<User> HandlingUser;

        public AsyncCall OnWriteStart;
        public Continuation OnWriteComplete;

        protected override async Task<IOperation> RunAsync(ExecutionContext context)
        {
            User user = HandlingUser.Evaluate(context, context.LocalUser);
            if (user != null)
            {
                await OnWriteStart.ExecuteAsync(context);
                switch (Severity.Evaluate(context))
                {
                    case LogSeverity.Log:
                        UniLog.Log(Tag.EvaluateRaw(context) + Value.EvaluateRaw(context)?.ToString());
                        break;
                    case LogSeverity.Warning:
                        UniLog.Warning(Tag.EvaluateRaw(context) + Value.EvaluateRaw(context)?.ToString());
                        break;
                    case LogSeverity.Error:
                        UniLog.Error(Tag.EvaluateRaw(context) + Value.EvaluateRaw(context)?.ToString());
                        break;
                }
                return OnWriteComplete.Target;
            }
            return null;
        }
    }
}
