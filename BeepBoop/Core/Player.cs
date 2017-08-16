using System;
using System.Collections.Generic;
using System.Threading;

namespace BeepBoop.Core
{
    internal class Player
    {
        public void Play(IEnumerable<(Components component, int freq, int duration)> song)
        {
            foreach (var songComponent in song)
            {
                if (songComponent.component == Components.Pause)
                {
                    Thread.Sleep(songComponent.duration);
                }
                else
                {
                    Console.Beep(songComponent.freq, songComponent.duration);
                }
            }
        }
    }
}
