using System;

namespace FridgeSimulator.Utils
{
    public class RandomNoise
    {
        private const double BOOL_THRESHOLD = Initial.NORMAL_THRESHOLD;

        public static double NoisyReading(double value)
        {
            return value + value * ((new Random().NextDouble() - 0.5) / 5);
        }

        public static bool NoisyReading(bool value)
        {
            // InRange ^ true value = false
            // NotInRange ^ true value = false
            // InRange ^ false value = true
            // NotInRange ^ false value = false 
            //var uniform = GetUniformUnsigned();
            var uniform = new Random().NextDouble();
            return value ^ (uniform < BOOL_THRESHOLD || uniform > (1.0 - BOOL_THRESHOLD));
        }
    }
}