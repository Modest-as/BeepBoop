using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace BeepBoop.Core
{
    internal class NaiveSampler : ISampler
    {
        private const int Tempo = 6;

        private const int CutOff = 400;

        private const int FreqScaling = 10;

        public IEnumerable<(Components component, int freq, int duration)> Sample(string fileName)
        {
            var samples = new List<bool>();

            using (var reader = new AudioFileReader(fileName))
            {
                var sampleProvider = reader.ToSampleProvider().ToMono();
                var sampleRate = sampleProvider.WaveFormat.SampleRate;
                var samplesForSecond = new float[sampleRate];

                sampleProvider.Read(samplesForSecond, 0, sampleRate);

                do
                {
                    samples.AddRange(ProcessSamples(samplesForSecond));
                    samplesForSecond = new float[sampleRate];
                } while (sampleProvider.Read(samplesForSecond, 0, sampleRate) != 0);

                return GetCodeElements(samples, sampleRate);
            }
        }

        private static IEnumerable<bool> ProcessSamples(float[] samplesForSecond)
        {
            var positiveThreshold = samplesForSecond.Where(x => x >= 0).Average();
            var negativeThreshold = samplesForSecond.Where(x => x <= 0).Average();

            foreach (var tmp in samplesForSecond)
            {
                if (tmp >= 0)
                {
                    if (tmp >= positiveThreshold)
                    {
                        yield return true;
                    }
                    else
                    {
                        yield return false;
                    }
                }
                else
                {
                    if (tmp >= negativeThreshold)
                    {
                        yield return false;
                    }
                    else
                    {
                        yield return true;
                    }
                }
            }
        }

        private static IEnumerable<(Components component, int freq, int duration)> GetCodeElements(IEnumerable<bool> samples, int sampleRate)
        {
            var lowCounter = 0;
            var highCounter = 0;
            var sampleCount = 0;

            foreach (var sample in samples)
            {
                sampleCount++;

                if (sample)
                {
                    highCounter++;

                    if (ComponentNeedsGenerating(lowCounter, highCounter, sampleRate))
                    {
                        var component = GenerateComponent(lowCounter, highCounter, sampleCount, sampleRate);

                        if (component.component != Components.Pause)
                        {
                            highCounter = 0;
                            sampleCount = 0;
                        }

                        lowCounter = 0;

                        yield return component;
                    }
                }
                else
                {
                    lowCounter++;
                }
            }
        }

        private static (Components component, int freq, int duration) GenerateComponent(int lowCounter, int highCounter, int sampleCount, int sampleRate)
        {
            if (lowCounter >= sampleRate / Tempo)
            {
                return (Components.Pause, 0, lowCounter * 1000 / sampleRate);
            }

            return (Components.Beep, Math.Max(highCounter * sampleRate / (sampleCount * FreqScaling), CutOff), highCounter * 1000 / sampleRate);
        }

        private static bool ComponentNeedsGenerating(int lowCounter, int highCounter, int sampleRate)
        {
            if (lowCounter >= sampleRate / Tempo) return true;

            return highCounter >= sampleRate / Tempo;
        }
    }
}
