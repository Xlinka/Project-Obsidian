using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Elements.Assets;
using Elements.Core;
using FrooxEngine;

namespace Obsidian.Elements;

public class FilterButterworth<S> where S : unmanaged, IAudioSample<S>
{
    /// resonance amount goes from sqrt(2) to ~ 0.1

    private float c, a1, a2, a3, b1, b2;

    /// <summary>
    /// Array of input values, latest are in front
    /// </summary>
    private S[] inputHistory = new S[2];

    /// <summary>
    /// Array of output values, latest are in front
    /// </summary>
    private S[] outputHistory = new S[3];

    public void UpdateCoefficients(float frequency, int sampleRate, PassType passType, float resonance)
    {
        switch (passType)
        {
            case PassType.Lowpass:
                c = 1.0f / (float)Math.Tan(Math.PI * frequency / sampleRate);
                a1 = 1.0f / (1.0f + resonance * c + c * c);
                a2 = 2f * a1;
                a3 = a1;
                b1 = 2.0f * (1.0f - c * c) * a1;
                b2 = (1.0f - resonance * c + c * c) * a1;
                break;
            case PassType.Highpass:
                c = (float)Math.Tan(Math.PI * frequency / sampleRate);
                a1 = 1.0f / (1.0f + resonance * c + c * c);
                a2 = -2f * a1;
                a3 = a1;
                b1 = 2.0f * (c * c - 1.0f) * a1;
                b2 = (1.0f - resonance * c + c * c) * a1;
                break;
        }
    }

    public enum PassType
    {
        Highpass,
        Lowpass,
    }

    public void Update(ref S newInput)
    {
        S first = newInput.Multiply(a1);
        S second = this.inputHistory[0].Multiply(a2);
        S third = this.inputHistory[1].Multiply(a3);
        S fourth = this.outputHistory[0].Multiply(b1);
        S fifth = this.outputHistory[1].Multiply(b2);
        S final = first.Add(second).Add(third).Subtract(fourth).Subtract(fifth);

        //for (int i = 0; i < final.ChannelCount; i++)
        //{
        //    if (final[i] > 1f) final = final.SetChannel(i, 1f);
        //    else if (final[i] < -1f) final = final.SetChannel(i, -1f);
        //}

        this.inputHistory[1] = this.inputHistory[0];
        this.inputHistory[0] = newInput;

        this.outputHistory[2] = this.outputHistory[1];
        this.outputHistory[1] = this.outputHistory[0];
        this.outputHistory[0] = final;

        newInput = final;
    }

    public S Value
    {
        get { return this.outputHistory[0]; }
    }
}

public class ButterworthFilterController
{
    private Dictionary<Type, object> filters = new();

    public void Clear()
    {
        filters.Clear();
    }

    public void Process<S>(Span<S> buffer, bool lowPass, float freq, float resonance) where S : unmanaged, IAudioSample<S>
    {
        // avoid dividing by zero
        if (freq == 0f)
        {
            buffer.Fill(default(S));
            Clear();
            return;
        }

        if (!filters.TryGetValue(typeof(S), out object filter))
        {
            filter = new FilterButterworth<S>();
            filters.Add(typeof(S), filter);
        }

        ((FilterButterworth<S>)filter).UpdateCoefficients(freq, Engine.Current.AudioSystem.SampleRate, lowPass ? FilterButterworth<S>.PassType.Lowpass : FilterButterworth<S>.PassType.Highpass, resonance);

        for (int i = 0; i < buffer.Length; i++)
        {
            ((FilterButterworth<S>)filter).Update(ref buffer[i]);
        }
    }
}

public class BandPassFilterController
{
    private Dictionary<Type, object> lowFilters = new();
    private Dictionary<Type, object> highFilters = new();

    public void Clear()
    {
        lowFilters.Clear();
        highFilters.Clear();
    }

    public void Process<S>(Span<S> buffer, float lowFreq, float highFreq, float resonance) where S : unmanaged, IAudioSample<S>
    {
        // avoid dividing by zero
        if (lowFreq == 0f || highFreq == 0f)
        {
            buffer.Fill(default(S));
            Clear();
            return;
        }

        if (!lowFilters.TryGetValue(typeof(S), out object lowFilter))
        {
            lowFilter = new FilterButterworth<S>();
            lowFilters.Add(typeof(S), lowFilter);
        }
        if (!highFilters.TryGetValue(typeof(S), out object highFilter))
        {
            highFilter = new FilterButterworth<S>();
            highFilters.Add(typeof(S), highFilter);
        }

        ((FilterButterworth<S>)lowFilter).UpdateCoefficients(highFreq, Engine.Current.AudioSystem.SampleRate, FilterButterworth<S>.PassType.Lowpass, resonance);
        ((FilterButterworth<S>)highFilter).UpdateCoefficients(lowFreq, Engine.Current.AudioSystem.SampleRate, FilterButterworth<S>.PassType.Highpass, resonance);

        for (int i = 0; i < buffer.Length; i++)
        {
            ((FilterButterworth<S>)lowFilter).Update(ref buffer[i]);
            ((FilterButterworth<S>)highFilter).Update(ref buffer[i]);
        }
    }
}

public interface IFirFilter
{
    public void SetCoefficients(float[] _coefficients); 
}

public class FirFilter<S> : IFirFilter where S : unmanaged, IAudioSample<S>
{
    public float[] coefficients;
    public S[] delayLine;
    public int delayLineIndex;
    private S[] lastBuffer = null;

    /// <summary>
    /// Creates a new FIR filter with the specified coefficients
    /// </summary>
    /// <param name="filterCoefficients">The filter coefficients that define the filter's behavior</param>
    public FirFilter(float[] filterCoefficients)
    {
        if (filterCoefficients == null || filterCoefficients.Length == 0)
            throw new ArgumentException("Filter coefficients cannot be null or empty");

        // Store the coefficients
        coefficients = (float[])filterCoefficients.Clone();

        // Create the delay line (buffer for previous samples)
        delayLine = new S[coefficients.Length];
        delayLineIndex = 0;
    }

    /// <summary>
    /// Process a single sample through the FIR filter
    /// </summary>
    /// <param name="input">The input sample</param>
    /// <returns>The filtered output sample</returns>
    public S ProcessSample(S input)
    {
        // Store the current input in the delay line
        delayLine[delayLineIndex] = input;

        // Calculate the output: sum of (coefficient * delayed sample)
        S output = default(S);
        int index = delayLineIndex;

        for (int i = 0; i < coefficients.Length; i++)
        {
            if (coefficients[i] != 0f && coefficients[i].IsValid() && (coefficients[i] * -1f).IsValid())
            {
                for (int channel = 0; channel < delayLine[index].ChannelCount; channel++)
                {
                    output = output.SetChannel(channel, output[channel] + (coefficients[i] * delayLine[index][channel]));
                }
            }

            // Move to the previous sample in the delay line (circular buffer)
            index--;
            if (index < 0)
                index = coefficients.Length - 1;
        }

        // Update the delay line index for the next sample
        delayLineIndex++;
        if (delayLineIndex >= coefficients.Length)
            delayLineIndex = 0;

        return output;
    }

    /// <summary>
    /// Process an entire buffer of samples through the FIR filter
    /// </summary>
    /// <param name="inputBuffer">Array of input samples</param>
    /// <returns>Array of filtered output samples</returns>
    public void ProcessBuffer(Span<S> inputBuffer, bool update)
    {
        if (inputBuffer == null)
            throw new ArgumentNullException(nameof(inputBuffer));

        if (!update && lastBuffer != null)
        {
            inputBuffer = lastBuffer.AsSpan();
            return;
        }

        for (int i = 0; i < inputBuffer.Length; i++)
        {
            inputBuffer[i] = ProcessSample(inputBuffer[i]);
        }

        if (update || lastBuffer == null)
        {
            lastBuffer = inputBuffer.ToArray();
        }
    }

    /// <summary>
    /// Reset the filter's internal state (delay line)
    /// </summary>
    public void Reset()
    {
        Array.Clear(delayLine, 0, delayLine.Length);
        delayLineIndex = 0;
    }

    public void SetCoefficients(float[] _coefficients)
    {
        coefficients = (float[])_coefficients.Clone();
    }
}

public interface IDelayEffect
{
    public void SetDelayTime(int newDelayTimeMs, int sampleRate);
}

public class DelayEffect<S> : IDelayEffect where S : unmanaged, IAudioSample<S>
{
    public S[] buffer;
    private int bufferSize;
    public int position = 0;
    private S[] lastBuffer = null;

    // Performance-optimized resampling parameters
    private const int FILTER_LENGTH = 4; // Reduced filter length for performance
    private S[] sincLUT; // Lookup table for sinc values
    private const int LUT_SIZE = 1024; // Size of lookup table
    private const int LUT_PRECISION = 10; // 2^10 = 1024 entries

    /// <summary>
    /// Creates a simple delay effect
    /// </summary>
    /// <param name="delayTimeMs">Delay time in milliseconds</param>
    /// <param name="sampleRate">Sample rate in Hz</param>
    public DelayEffect(int delayTimeMs, int sampleRate)
    {
        // Calculate buffer size from delay time
        bufferSize = (delayTimeMs * sampleRate) / 1000;
        bufferSize = Math.Max(1, bufferSize); // Ensure minimum size
        buffer = new S[bufferSize];

        // Initialize lookup table for faster resampling
        InitializeSincLUT();
    }

    /// <summary>
    /// Initialize lookup table for fast sinc calculations
    /// </summary>
    private void InitializeSincLUT()
    {
        sincLUT = new S[LUT_SIZE * (2 * FILTER_LENGTH)];

        for (int i = 0; i < LUT_SIZE; i++)
        {
            float fraction = (float)i / LUT_SIZE;

            for (int j = -FILTER_LENGTH + 1; j <= FILTER_LENGTH; j++)
            {
                float x = j - fraction;
                S weight = default;

                if (x == 0)
                {
                    for (int k = 0; k < weight.ChannelCount; k++)
                    {
                        weight = weight.SetChannel(k, 1f);
                    }
                }
                else if (Math.Abs(x) >= FILTER_LENGTH)
                {
                    weight = default;
                }
                else
                {
                    // Lanczos-2 kernel (simplified for performance)
                    float pi_x = (float)(Math.PI * x);
                    float sinc = (float)Math.Sin(pi_x) / pi_x;
                    float lanczos = (float)Math.Sin(pi_x / FILTER_LENGTH) / (pi_x / FILTER_LENGTH);
                    for (int k = 0; k < weight.ChannelCount; k++)
                    {
                        weight = weight.SetChannel(k, sinc * lanczos);
                    }
                }

                // Store in lookup table
                sincLUT[i * (2 * FILTER_LENGTH) + (j + FILTER_LENGTH - 1)] = weight;
            }
        }
    }

    /// <summary>
    /// Process samples in bulk for better performance
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void ProcessBulk(Span<S> samples, float dryWet, float feedback)
    {
        // Calculate how many samples we can process without hitting the buffer wraparound
        int samplesBeforeWrap = Math.Min(samples.Length, bufferSize - position);

        // Set feedback (limit to 0.99 to prevent excessive buildup)
        var fb = Math.Min(0.99f, Math.Max(0.0f, feedback));

        // Process first chunk (up to buffer wraparound)
        fixed (S* pSamples = samples, pBuffer = buffer)
        {
            for (int i = 0; i < samplesBeforeWrap; i++)
            {
                S input = pSamples[i];
                S delayed = pBuffer[position + i];

                // Write new value with feedback
                pBuffer[position + i] = input.Add(delayed.Multiply(fb));

                float newDryWet = MathX.Clamp(dryWet, 0f, 1f);

                // Mix dry and wet
                pSamples[i] = input.Multiply(1f - newDryWet).Add(delayed.Multiply(newDryWet));
            }

            // Process remaining samples (after buffer wraparound)
            for (int i = samplesBeforeWrap; i < samples.Length; i++)
            {
                int bufferPos = (position + i) % bufferSize;
                S input = pSamples[i];
                S delayed = pBuffer[bufferPos];

                // Write new value with feedback
                pBuffer[bufferPos] = input.Add(delayed.Multiply(fb));

                float newDryWet = MathX.Clamp(dryWet, 0f, 1f);

                // Mix dry and wet
                pSamples[i] = input.Multiply(1f - newDryWet).Add(delayed.Multiply(newDryWet));
            }
        }

        // Update position
        position = (position + samples.Length) % bufferSize;
    }

    /// <summary>
    /// Process an array of samples
    /// </summary>
    public void Process(Span<S> inputBuffer, float dryWet, float feedback, bool update)
    {
        if (!update && lastBuffer != null)
        {
            inputBuffer = lastBuffer.AsSpan();
            return;
        }
        ProcessBulk(inputBuffer, dryWet, feedback);
        if (update || lastBuffer == null)
        {
            lastBuffer = inputBuffer.ToArray();
        }
    }

    /// <summary>
    /// Change delay time with efficient high-quality resampling
    /// </summary>
    public void SetDelayTime(int newDelayTimeMs, int sampleRate)
    {
        // Calculate new buffer size
        int newBufferSize = Math.Max(1, (newDelayTimeMs * sampleRate) / 1000);

        // No change needed
        if (newBufferSize == bufferSize)
            return;

        // Fast path for small delay changes - use linear interpolation for ±10% changes
        if (Math.Abs(newBufferSize - bufferSize) < bufferSize / 10)
        {
            SetDelayTimeLinear(newBufferSize);
            return;
        }

        // Standard path - use high-quality resampling
        SetDelayTimeLanczos(newBufferSize);
    }

    /// <summary>
    /// Fast linear interpolation for small delay time changes
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetDelayTimeLinear(int newBufferSize)
    {
        // Create new buffer
        S[] newBuffer = new S[newBufferSize];

        // Use simple linear interpolation for speed
        for (int i = 0; i < newBufferSize; i++)
        {
            float exactPos = (float)i * bufferSize / newBufferSize;
            int pos1 = (int)exactPos;
            int pos2 = (pos1 + 1) % bufferSize;
            float fraction = exactPos - pos1;

            pos1 = (position + pos1) % bufferSize;
            pos2 = (position + pos2) % bufferSize;

            newBuffer[i] = buffer[pos1].Multiply(1 - fraction).Add(buffer[pos2].Multiply(fraction));
        }

        // Update state
        buffer = newBuffer;
        bufferSize = newBufferSize;
        position = 0;
    }

    /// <summary>
    /// High-quality Lanczos resampling for significant delay time changes
    /// </summary>
    private unsafe void SetDelayTimeLanczos(int newBufferSize)
    {
        // Create new buffer
        S[] newBuffer = new S[newBufferSize];

        // Use lookup-based Lanczos resampling
        fixed (S* pBuffer = buffer, pNewBuffer = newBuffer, pSincLUT = sincLUT)
        {
            for (int i = 0; i < newBufferSize; i++)
            {
                float exactPos = (float)i * bufferSize / newBufferSize;
                int intPos = (int)exactPos;
                float fraction = exactPos - intPos;

                // Convert fraction to lookup table index
                int lutIdx = (int)(fraction * LUT_SIZE);
                if (lutIdx >= LUT_SIZE) lutIdx = LUT_SIZE - 1;

                // Offset in the lookup table for this fraction
                int lutOffset = lutIdx * (2 * FILTER_LENGTH);

                // Apply filter
                S sum = default;
                for (int j = -FILTER_LENGTH + 1; j <= FILTER_LENGTH; j++)
                {
                    int samplePos = (position + intPos + j + bufferSize) % bufferSize;
                    S weight = pSincLUT[lutOffset + (j + FILTER_LENGTH - 1)];
                    sum = sum.Add(pBuffer[samplePos].LerpTo(weight, 1f));
                }

                pNewBuffer[i] = sum;
            }
        }

        // Update state
        buffer = newBuffer;
        bufferSize = newBufferSize;
        position = 0;
    }
}

public static class FilterDesign
{
    /// <summary>
    /// Creates a low-pass FIR filter using the window method
    /// </summary>
    /// <param name="cutoffFrequency">Cutoff frequency in Hz</param>
    /// <param name="sampleRate">Sample rate in Hz</param>
    /// <param name="filterLength">Length of the filter (number of taps)</param>
    /// <returns>The filter coefficients</returns>
    public static float[] DesignLowPassFilter(float cutoffFrequency, float sampleRate, int filterLength)
    {
        // Ensure filter length is odd for symmetric filter
        if (filterLength % 2 == 0)
            filterLength++;

        float[] coefficients = new float[filterLength];
        float fc = cutoffFrequency / sampleRate; // Normalized cutoff frequency
        int center = filterLength / 2;

        // Create ideal lowpass filter response (sinc function)
        for (int i = 0; i < filterLength; i++)
        {
            if (i == center)
            {
                // Handle the center point to avoid division by zero
                coefficients[i] = 2.0f * fc;
            }
            else
            {
                // Calculate sinc function: sin(x)/x
                float x = 2.0f * (float)Math.PI * fc * (i - center);
                coefficients[i] = (float)Math.Sin(x) / x;
            }

            // Apply Hamming window to reduce ringing effects
            float window = 0.54f - 0.46f * (float)Math.Cos(2.0f * Math.PI * i / (filterLength - 1));
            coefficients[i] *= window;
        }

        // Normalize the filter to ensure unity gain at DC
        float sum = 0;
        for (int i = 0; i < filterLength; i++)
        {
            sum += coefficients[i];
        }

        for (int i = 0; i < filterLength; i++)
        {
            coefficients[i] /= sum;
        }

        return coefficients;
    }

    /// <summary>
    /// Creates a high-pass FIR filter using the window method
    /// </summary>
    /// <param name="cutoffFrequency">Cutoff frequency in Hz</param>
    /// <param name="sampleRate">Sample rate in Hz</param>
    /// <param name="filterLength">Length of the filter (number of taps)</param>
    /// <returns>The filter coefficients</returns>
    public static float[] DesignHighPassFilter(float cutoffFrequency, float sampleRate, int filterLength)
    {
        // First design a lowpass filter
        float[] lowpass = DesignLowPassFilter(cutoffFrequency, sampleRate, filterLength);

        // Convert to highpass using spectral inversion
        for (int i = 0; i < filterLength; i++)
        {
            lowpass[i] = -lowpass[i];
        }

        // Add 1 to the center tap
        int center = filterLength / 2;
        lowpass[center] += 1.0f;

        return lowpass;
    }
}

public static class Algorithms
{
    public static void SineShapedRingModulation<S>(Span<S> buffer, Span<S> input1, Span<S> input2, float modulationIndex, int channelCount) where S : unmanaged, IAudioSample<S>
    {
        // Apply sine-shaped ring modulation
        for (int i = 0; i < buffer.Length; i++)
        {
            for (int j = 0; j < channelCount; j++)
            {
                float carrierValue = input1[i][j];
                float modulatorValue = input2[i][j];

                float modulatedValue = (float)(carrierValue * Math.Sin(2 * Math.PI * modulationIndex * modulatorValue));

                buffer[i] = buffer[i].SetChannel(j, modulatedValue);

                //if (buffer[i][j] > 1f) buffer[i] = buffer[i].SetChannel(j, 1f);
                //if (buffer[i][j] < -1f) buffer[i] = buffer[i].SetChannel(j, -1f);
            }
        }
    }

    /// <summary>
    /// Calculates instantaneous phase of a signal using a more robust Hilbert transform approximation
    /// </summary>
    private static double[] CalculateInstantaneousPhase<S>(Span<S> buffer) where S : unmanaged, IAudioSample<S>
    {
        int length = buffer.Length;
        double[] phase = new double[length];
        double[] avgAmplitudes = new double[length];

        // Calculate average amplitudes across channels
        for (int i = 0; i < length; i++)
        {
            double sum = 0;
            for (int j = 0; j < buffer[i].ChannelCount; j++)
            {
                sum += buffer[i][j];
            }
            avgAmplitudes[i] = sum / buffer[i].ChannelCount;
        }

        // Use a wider window for derivative calculation to reduce noise
        const int windowSize = 5;
        const double epsilon = 1e-10; // Small value to prevent division by zero

        for (int i = windowSize; i < length - windowSize; i++)
        {
            // Calculate smoothed derivative using a wider window
            double derivative = 0;
            for (int j = 1; j <= windowSize; j++)
            {
                derivative += (avgAmplitudes[i + j] - avgAmplitudes[i - j]) / (2.0 * j);
            }
            derivative /= windowSize;

            // Calculate analytic signal magnitude with protection against zero
            double magnitude = Math.Sqrt(avgAmplitudes[i] * avgAmplitudes[i] + derivative * derivative + epsilon);

            // Normalize with smoothing to prevent discontinuities
            double normalizedSignal = avgAmplitudes[i] / magnitude;

            // Clamp to valid arccos range to prevent NaN
            normalizedSignal = Math.Max(-1.0, Math.Min(1.0, normalizedSignal));

            // Calculate phase
            phase[i] = Math.Acos(normalizedSignal);

            // Correct phase quadrant based on derivative sign
            if (derivative < 0)
                phase[i] = 2 * Math.PI - phase[i];
        }

        // Smooth out edge cases using linear interpolation
        for (int i = 0; i < windowSize; i++)
        {
            phase[i] = phase[windowSize];
        }
        for (int i = length - windowSize; i < length; i++)
        {
            phase[i] = phase[length - windowSize - 1];
        }

        return phase;
    }

    public static void PhaseModulation<S>(Span<S> buffer, Span<S> input1, Span<S> input2, float modulationIndex, int channelCount) where S : unmanaged, IAudioSample<S>
    {
        double[] carrierPhase = CalculateInstantaneousPhase(input1);

        // Apply phase modulation with improved amplitude handling
        for (int i = 0; i < buffer.Length; i++)
        {
            // Get carrier amplitude for envelope
            float carrierAmplitude = 0;
            for (int j = 0; j < channelCount; j++)
            {
                carrierAmplitude += Math.Abs(input1[i][j]);
            }
            carrierAmplitude /= channelCount;

            for (int j = 0; j < channelCount; j++)
            {
                // Apply modulation with smooth amplitude envelope
                double modifiedPhase = carrierPhase[i] + (modulationIndex * input2[i][j]);

                // Generate output sample with envelope following
                float outputSample = carrierAmplitude * (float)Math.Sin(modifiedPhase);

                // Soft clip instead of hard limiting
                //if (Math.Abs(outputSample) > 1f)
                //{
                //    outputSample = Math.Sign(outputSample) * (1f - 1f / (Math.Abs(outputSample) + 1f));
                //}

                buffer[i] = buffer[i].SetChannel(j, outputSample);
            }
        }
    }

    public static void RingModulation<S>(Span<S> buffer, Span<S> input1, Span<S> input2, float modulationIndex, int channelCount) where S : unmanaged, IAudioSample<S>
    {
        // Apply ring modulation
        for (int i = 0; i < buffer.Length; i++)
        {
            for (int j = 0; j < channelCount; j++)
            {
                float carrierValue = input1[i][j];
                float modulatorValue = input2[i][j];

                float modulatedValue = (float)(carrierValue * modulatorValue * modulationIndex);

                buffer[i] = buffer[i].SetChannel(j, modulatedValue);

                //if (buffer[i][j] > 1f) buffer[i] = buffer[i].SetChannel(j, 1f);
                //if (buffer[i][j] < -1f) buffer[i] = buffer[i].SetChannel(j, -1f);
            }
        }
    }

    // smoothingFactor is between 0.0 (no smoothing) and 0.9999.. (almost smoothing to DC) - *kind* of the inverse of cutoff frequency
    public static void EMAIIRSmoothSignal<S>(ref Span<S> input, int N, float smoothingFactor = 0.8f) where S : unmanaged, IAudioSample<S>
    {
        // forward EMA IIR
        S acc = input[0];
        for (int i = 0; i < N; ++i)
        {
            acc = input[i].LerpTo(acc, smoothingFactor);
            input[i] = acc;
        }

        // backward EMA IIR - required only if we need to preserve the phase (aka make the filter symetric) - we usually want this
        acc = input[N - 1];
        for (int i = N - 1; i >= 0; --i)
        {
            acc = input[i].LerpTo(acc, smoothingFactor);
            input[i] = acc;
        }
    }
}