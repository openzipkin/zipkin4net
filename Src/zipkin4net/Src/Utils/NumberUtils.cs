using System;

namespace zipkin4net.Utils
{
    public static class NumberUtils
    {
        public static Guid LongToGuid(long value)
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 8);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes, 8, 8);
            }
            return new Guid(bytes);
        }

        public static long GuidToLong(Guid value)
        {
            var b = value.ToByteArray();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(b, 8, 8);
            }
            var blong = BitConverter.ToInt64(b, 8);
            return blong;
        }

        public static string EncodeLongToLowerHexString(long value)
        {
            return value.ToString("x16");
        }

        public static long DecodeHexString(string longAsHexString)
        {
            return Convert.ToInt64(longAsHexString, 16);
        }

    }
}
