using System.Collections.Generic;

namespace BeepBoop.Core
{
    internal interface ISampler
    {
        IEnumerable<(Components component, int freq, int duration)> Sample(string fileName);
    }
}