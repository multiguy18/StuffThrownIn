using System;
using System.Collections.Generic;
using System.Text;

namespace MidiFlood
{
    public static class Extensions
    {
        public static object PeekRandom(this Array array)
        {
            Random random = new Random();
            int randomIndex = random.Next(array.Length - 1);

            return array.GetValue(randomIndex);
        }
    }
}
