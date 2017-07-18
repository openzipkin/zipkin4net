using System;

namespace Criteo.Profiling.Tracing.Utils
{
    public static class Guard
    {
        public static T IsNotNull<T>(T value, string argumentName)
        {
            if (value == null)
            {
                throw new ArgumentNullException($"{argumentName} cannot be null");
            }
            return value;
        }
    }
}
