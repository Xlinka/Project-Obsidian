using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;
using Elements.Core;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math
{
    [NodeCategory("Obsidian/Math")]
    public class MIDI_NoteFrequency : ValueFunctionNode<FrooxEngineContext, float>
    {
        public readonly ValueInput<int> NoteNumber;
        protected override float Compute(FrooxEngineContext context)
        {
            var note = NoteNumber.Evaluate(context);
            return 440f * MathX.Pow(2, (note - 69f) / 12f);
        }
    }
}