using System;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using FrooxEngine.ProtoFlux;
using FrooxEngine;
using Elements.Assets;
using Elements.Core;
using System.Linq;
using System.Collections.Generic;
using Elements.Data;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Math
{
    [DataModelType]
    public enum NoteScale
    {
        Chromatic,
        Major,
        Minor,
        Blues,
        Dorian,
        Phrygian,
        Lydian,
        Mixolydian,
        Locrian
    }

    [NodeCategory("Obsidian/Math")]
    public class FrequencyQuantize : VoidNode<FrooxEngineContext>
    {
        public readonly ValueInput<float> Frequency;
        public readonly ValueInput<NoteScale> Scale;
        public readonly ValueInput<bool> RoundToNearest;

        public readonly ValueInput<int> Offset;

        public readonly ValueOutput<float> QuantizedFrequency;
        public readonly ValueOutput<bool> InScale;

        public static readonly List<int> ChromaticScale = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
        public static readonly List<int> MajorScale = [0, 2, 4, 5, 7, 9, 11];
        public static readonly List<int> MinorScale = [0, 2, 3, 5, 7, 8, 10];
        public static readonly List<int> BluesScale = [0, 3, 5, 6, 7, 10];
        public static readonly List<int> DorianScale = [0, 2, 3, 5, 7, 9, 10];
        public static readonly List<int> PhrygianScale = [0, 1, 3, 5, 7, 8, 10];
        public static readonly List<int> LydianScale = [0, 2, 4, 6, 7, 9, 11];
        public static readonly List<int> MixolydianScale = [0, 2, 4, 5, 7, 9, 10];
        public static readonly List<int> LocrianScale = [0, 1, 3, 5, 6, 8, 10];

        private static int FreqToNearestMidi(float freq)
        {
            var n = 12f * (float)System.Math.Log(freq / 440f, 2f) + 69f;
            return (int)MathX.Round(n, MidpointRounding.AwayFromZero);
        }

        private static float MidiToFreq(int midi)
        {
            return 440f * MathX.Pow(2, (midi - 69f) / 12f);
        }

        protected override void ComputeOutputs(FrooxEngineContext context)
        {
            var freq = Frequency.Evaluate(context);
            
            var offset = Offset.Evaluate(context);
            var rootNote = 69 + offset;

            int midi;
            if (freq == 0) midi = 0;
            else midi = FreqToNearestMidi(freq);

            bool inScale = false;
            List<int> scaleList;
            switch (Scale.Evaluate(context))
            {
                case NoteScale.Chromatic:
                    scaleList = ChromaticScale;
                    break;
                case NoteScale.Major:
                    scaleList = MajorScale;
                    break;
                case NoteScale.Minor:
                    scaleList = MinorScale;
                    break;
                case NoteScale.Blues:
                    scaleList = BluesScale;
                    break;
                case NoteScale.Dorian:
                    scaleList = DorianScale;
                    break;
                case NoteScale.Phrygian:
                    scaleList = PhrygianScale;
                    break;
                case NoteScale.Lydian:
                    scaleList = LydianScale;
                    break;
                case NoteScale.Mixolydian:
                    scaleList = MixolydianScale;
                    break;
                case NoteScale.Locrian:
                    scaleList = LocrianScale;
                    break;
                default:
                    scaleList = ChromaticScale;
                    break;
            }
            foreach (var note in scaleList)
            {
                var scaleDegree = (midi - rootNote) % 12;
                if (scaleDegree < 0) scaleDegree += 12;
                if (note == scaleDegree)
                {
                    inScale = true;
                    break;
                }
            }
            if (!inScale)
            {
                if (RoundToNearest.Evaluate(context))
                {
                    int scaleDegree = (midi - rootNote) % 12;
                    if (scaleDegree < 0)
                        scaleDegree += 12;

                    int closestSemitone = scaleList[0];
                    int minDifference = int.MaxValue;

                    foreach (int scaleSemitone in scaleList)
                    {
                        int difference = MathX.Abs(scaleDegree - scaleSemitone);
                        if (difference < minDifference)
                        {
                            minDifference = difference;
                            closestSemitone = scaleSemitone;
                        }
                    }

                    midi += closestSemitone - scaleDegree;

                    inScale = true;
                }
            }
            InScale.Write(inScale, context);
            if (inScale)
            {
                var res = MidiToFreq(midi);
                QuantizedFrequency.Write(res, context);
            }
            else
            {
                QuantizedFrequency.Write(0f, context);
            }
        }

        public FrequencyQuantize()
        {
            QuantizedFrequency = new ValueOutput<float>(this);
            InScale = new ValueOutput<bool>(this);
        }
    }
}