using System.Threading.Tasks;
using Elements.Core;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Actions
{
    [NodeCategory("Obsidian/Actions")]
    public class TweenScale: AsyncActionNode<ExecutionContext>
    {
        public ValueInput<float3> To;
        public ValueInput<float3> From;
        [Core.DefaultValueAttribute(1f)]
        public ValueInput<float> Duration;
        [Core.DefaultValueAttribute(CurvePreset.Smooth)]
        public ValueInput<CurvePreset> Curve;
        public ValueInput<bool> ProportionalDuration;
        public ObjectInput<Slot> Target;
        public AsyncCall OnStarted;
        public Continuation OnDone;

        protected override async Task<IOperation> RunAsync(ExecutionContext context)
        {
            IField<float3> field = Target.Evaluate(context).Scale_Field;
            if (field == null)
            {
                return null;
            }
            float3 val = field.Value;
            float3 val2 = field.Value;
            if (To.Source != null)
            {
                val2 = To.Evaluate(context);
            }
            if (From.Source != null)
            {
                val = From.Evaluate(context);
            }
            float num = Duration.Evaluate(context, 1f);
            bool num2 = ProportionalDuration.Evaluate(context, defaultValue: false);
            CurvePreset curve = Curve.Evaluate(context, CurvePreset.Smooth);
            if (num2 && Coder<float3>.SupportsDistance)
            {
                float num3 = Coder<float3>.Distance(val, val2);
                if (num3.IsValid())
                {
                    num *= num3;
                }
            }
            TaskCompletionSource<bool> completion = new TaskCompletionSource<bool>();
            field.TweenFromTo(val, val2, num, curve, null, delegate
            {
                completion.SetResult(result: true);
            });
            await OnStarted.ExecuteAsync(context);
            await completion.Task;
            return OnDone.Target;
        }
    }
}
