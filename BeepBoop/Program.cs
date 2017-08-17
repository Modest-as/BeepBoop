using System;
using BeepBoop.Core;

namespace BeepBoop
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var sampler = new GoertzelSampler();

            Play(sampler, @"Samples\Test.mp3");
        }

        private static void Play(ISampler sampler, string fileName)
        {
            Console.WriteLine("Sampling...");
            var song = sampler.Sample(fileName);

            var player = new Player();

            Console.WriteLine("Playing...");

            player.Play(song);
        }
    }
}
