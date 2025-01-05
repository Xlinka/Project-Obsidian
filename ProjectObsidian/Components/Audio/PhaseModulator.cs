using System;
using FrooxEngine;
using Elements.Assets;
using System.Threading;

namespace Obsidian.Components.Audio
{
    [Category(new string[] { "Obsidian/Audio" })]
    public class PhaseModulator : Component, IAudioSource, IWorldElement
    {
        [Range(0f, 5f, "0.00")]
        public readonly Sync<float> ModulationIndex;

        public readonly SyncRef<IAudioSource> CarrierSource;
        public readonly SyncRef<IAudioSource> ModulatorSource;

        public bool IsActive
        {
            get
            {
                return CarrierSource.Target != null &&
                       ModulatorSource.Target != null &&
                       CarrierSource.Target.IsActive &&
                       ModulatorSource.Target.IsActive;
            }
        }

        public int ChannelCount
        {
            get
            {
                return CarrierSource.Target?.ChannelCount ?? 0;
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            ModulationIndex.Value = 1f; // Default modulation index
        }

        /// <summary>
        /// Calculates instantaneous phase of a signal using a simple Hilbert transform approximation
        /// </summary>
        private double[] CalculateInstantaneousPhase<S>(Span<S> buffer) where S : unmanaged, IAudioSample<S>
        {
            int length = buffer.Length;
            double[] phase = new double[length];
            double[] avgAmplitudes = new double[length];

            for (int i = 1; i < length - 1; i++)
            {
                for (int j = 0; j < buffer[i].ChannelCount; j++)
                {
                    avgAmplitudes[i] += buffer[i][j];
                }
                avgAmplitudes[i] /= buffer[i].ChannelCount;
            }

            // Simple 3-point derivative for phase approximation
            for (int i = 1; i < length - 1; i++)
            {
                double derivative = (avgAmplitudes[i + 1] - avgAmplitudes[i - 1]) / 2.0;
                double hilbertApprox = avgAmplitudes[i] / Math.Sqrt(avgAmplitudes[i] * avgAmplitudes[i] + derivative * derivative);
                phase[i] = Math.Acos(hilbertApprox);

                // Correct phase quadrant based on derivative sign
                if (derivative < 0)
                    phase[i] = 2 * Math.PI - phase[i];
            }

            // Handle edge cases
            phase[0] = phase[1];
            phase[length - 1] = phase[length - 2];

            return phase;
        }

        // TODO: Make this not click when the signal goes silent and then not silent?
        public void Read<S>(Span<S> buffer) where S : unmanaged, IAudioSample<S>
        {
            if (!IsActive)
            {
                buffer.Fill(default(S));
                return;
            }

            int channelCount = ChannelCount;
            if (channelCount == 0)
            {
                buffer.Fill(default(S));
                return;
            }

            Span<S> carrierBuffer = stackalloc S[buffer.Length];
            Span<S> modulatorBuffer = stackalloc S[buffer.Length];

            CarrierSource.Target.Read(carrierBuffer);
            ModulatorSource.Target.Read(modulatorBuffer);

            float modulationIndex = ModulationIndex.Value;

            double[] carrierPhase = CalculateInstantaneousPhase(carrierBuffer);

            // Apply phase modulation
            for (int i = 0; i < buffer.Length; i++)
            {
                for (int j = 0; j < buffer[i].ChannelCount; j++)
                {
                    double modifiedPhase = carrierPhase[i] + (modulationIndex * modulatorBuffer[i][j]);

                    // Calculate amplitude using original carrier amplitude
                    float amplitude = carrierBuffer[i][j];

                    // Generate output sample
                    buffer[i] = buffer[i].SetChannel(j, amplitude * (float)Math.Sin(modifiedPhase));

                    if (buffer[i][j] > 1f) buffer[i] = buffer[i].SetChannel(j, 1f);
                    if (buffer[i][j] < -1f) buffer[i] = buffer[i].SetChannel(j, -1f);
                }
            }
        }
    }
}