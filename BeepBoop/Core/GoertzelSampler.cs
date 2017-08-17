using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace BeepBoop.Core
{
    /// <summary>
    /// This sampler is using Goertzel algorithm.
    /// </summary>
    internal class GoertzelSampler : ISampler
    {
        private const int Tempo = 10;

        private const int BlockSize = 3000;

        private const int FreqIncrement = 30000 / BlockSize;

        public IEnumerable<(Components component, int freq, int duration)> Sample(string fileName)
        {
            var result = new List<(Components component, int freq, int duration)>();

            using (var reader = new AudioFileReader(fileName))
            {
                var sampleProvider = reader.ToSampleProvider().ToMono();
                var sampleRate = sampleProvider.WaveFormat.SampleRate;
                var samplesForSecond = new float[sampleRate];

                sampleProvider.Read(samplesForSecond, 0, sampleRate);

                do
                {
                    result.AddRange(ProcessSamples(samplesForSecond, sampleRate));
                    samplesForSecond = new float[sampleRate];
                } while (sampleProvider.Read(samplesForSecond, 0, sampleRate) != 0);

                return StitchComponents(result);
            }
        }

        private IEnumerable<(Components component, int freq, int duration)> StitchComponents(IEnumerable<(Components component, int freq, int duration)> songComponents)
        {
            (Components component, int freq, int duration) tmp = (Components.Pause, 0, 0);

            foreach (var songComponent in songComponents)
            {
                if (Math.Abs(tmp.freq - songComponent.freq) <= FreqIncrement)
                {
                    tmp = (tmp.component, tmp.freq, tmp.duration + songComponent.duration);
                }
                else
                {
                    yield return tmp;

                    tmp = songComponent;
                }
            }
        }

        private IEnumerable<(Components component, int freq, int duration)> ProcessSamples(float[] samplesForSecond, int sampleRate)
        {
            var blockSize = sampleRate / Tempo;

            for (var i = 0; i < Tempo; i++)
            {
                var samples = samplesForSecond.Skip(blockSize * i).Take(blockSize).ToList();

                yield return ProcessBlock(samples, sampleRate);
            }
        }

        private (Components component, int freq, int duration) ProcessBlock(IReadOnlyList<float> samples, int sampleRate)
        {
            var bestFreq = 0.0;
            var maxAmp = 0.0;

            for (var i = 0; i < BlockSize; i++)
            {
                var freq = 0.5 + i * FreqIncrement;

                var amp = GoertzelFilter(samples, freq, sampleRate);

                if (amp > maxAmp)
                {
                    maxAmp = amp;
                    bestFreq = freq;
                }
            }

            var component = Components.Beep;

            if (bestFreq < 37)
            {
                component = Components.Pause;
            }

            return (component, (int) bestFreq, samples.Count * 1000 / sampleRate);
        }

        private static double GoertzelFilter(IReadOnlyList<float> samples, double freq, int sampleRate)
        {
            var tmp1 = 0.0;
            var tmp2 = 0.0;

            var normalizedfreq = freq / sampleRate;

            var coeff = 2 * Math.Cos(2 * Math.PI * normalizedfreq);

            for (var i = 0; i < samples.Count; i++)
            {
                var newTerm = samples[i] + coeff * tmp1 - tmp2;
                tmp2 = tmp1;
                tmp1 = newTerm;
            }

            var power = tmp2 * tmp2 + tmp1 * tmp1 - coeff * tmp1 * tmp2;

            return power;
        }
    }
}
