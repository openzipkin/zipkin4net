using System;
using System.Text;

namespace zipkin4net.Tracers.Zipkin
{
    internal static class BinaryAnnotationValueEncoder
    {

        public static byte[] Encode(string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        public static byte[] Encode(bool value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] Encode(short value)
        {
            return ConvertBigEndian(BitConverter.GetBytes(value));
        }

        public static byte[] Encode(int value)
        {
            return ConvertBigEndian(BitConverter.GetBytes(value));
        }

        public static byte[] Encode(long value)
        {
            return ConvertBigEndian(BitConverter.GetBytes(value));
        }

        public static byte[] Encode(double value)
        {
            return ConvertBigEndian(BitConverter.GetBytes(value));
        }

        private static byte[] ConvertBigEndian(byte[] input)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(input);
            }
            return input;
        }

    }
}
