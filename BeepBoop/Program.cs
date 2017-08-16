using BeepBoop.Core;

namespace BeepBoop
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var sampler = new Sampler(@"Samples\Mortal Kombat.mp3");

            var song = sampler.Sample();

            var player = new Player();

            player.Play(song);
        }
    }
}
