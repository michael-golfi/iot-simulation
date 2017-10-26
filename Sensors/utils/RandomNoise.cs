namespace IoTEdgeFridgeSimulator.Utils
{
    public class RandomNoise
    {
        private static uint m_z;
        private static uint m_w;

        private static uint GetUint()
        {
            m_z = 36969 * (m_z & 65535) + (m_z >> 16);
            m_w = 18000 * (m_w & 65535) + (m_w >> 16);
            return (m_z << 16) + m_w;
        }

        /// From https://www.codeproject.com/Articles/25172/Simple-Random-Number-Generation
        /// Added negative ranging
        public static double GetUniform()
        {
            // 0 <= u < 2^32
            uint u = GetUint();
            // The magic number below is 1/(2^32 + 2).
            // The result is strictly between -0.1 and 0.1.
            return ((((u + 1.0) * 2.328306435454494e-10) - 0.5) / 10);
        }

        public static double NoisyReading(double value)
        {
            return value + value * GetUniform();
        }
    }
}