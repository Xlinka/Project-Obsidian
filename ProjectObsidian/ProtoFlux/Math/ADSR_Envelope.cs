using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using System.Threading.Tasks;
using Elements.Core;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math
{
    [NodeCategory("Obsidian/Math")]
    public class ADSR_Envelope : AsyncActionNode<FrooxEngineContext>
    {
        public readonly ValueInput<float> AttackTime;
        public readonly ValueInput<float> DecayTime;
        public readonly ValueInput<float> SustainTime;
        public readonly ValueInput<float> SustainValue;
        public readonly ValueInput<float> ReleaseTime;

        [DefaultValueAttribute(CurvePreset.Smooth)]
        public ValueInput<CurvePreset> Curve;

        public ObjectInput<IField<float>> Target;

        public AsyncCall OnStarted;

        public Continuation OnDone;

        protected override async Task<IOperation> RunAsync(FrooxEngineContext context)
        {
            IField<float> field = Target.Evaluate(context);
            if (field == null)
            {
                return null;
            }

            CurvePreset curve = Curve.Evaluate(context, CurvePreset.Smooth);
            float attackTime = AttackTime.Evaluate(context);
            float decayTime = DecayTime.Evaluate(context);
            float sustainValue = SustainValue.Evaluate(context);
            float sustainTime = SustainTime.Evaluate(context);
            float releaseTime = ReleaseTime.Evaluate(context);

            // ATTACK: tween from 0 to 1 over attackTime seconds
            TaskCompletionSource<bool> completion = new TaskCompletionSource<bool>();
            field.TweenFromTo(0f, 1f, attackTime, curve, null, delegate
            {
                completion.SetResult(result: true);
            });
            await OnStarted.ExecuteAsync(context);
            await completion.Task;

            // DECAY: tween from 1 to min(sustainValue, 1) over decayTime seconds
            TaskCompletionSource<bool> completion2 = new TaskCompletionSource<bool>();
            field.TweenFromTo(1f, MathX.Min(sustainValue, 1f), decayTime, curve, null, delegate
            {
                completion2.SetResult(result: true);
            });
            await completion2.Task;

            // SUSTAIN: stay at min(sustainValue, 1) for sustainTime seconds
            await Task.Delay(TimeSpan.FromSeconds(sustainTime));

            // RELEASE: tween from min(sustainValue, 1) to 0 over releaseTime seconds
            TaskCompletionSource<bool> completion3 = new TaskCompletionSource<bool>();
            field.TweenFromTo(MathX.Min(sustainValue, 1f), 0f, releaseTime, curve, null, delegate
            {
                completion3.SetResult(result: true);
            });
            await completion3.Task;

            return OnDone.Target;
        }
    }
}