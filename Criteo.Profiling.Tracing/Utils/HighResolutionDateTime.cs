using System;
using System.Runtime.InteropServices;

namespace Criteo.Profiling.Tracing.Utils
{
    internal static class HighResolutionDateTime
    {
        public static bool IsAvailable { get; private set; }

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern void GetSystemTimePreciseAsFileTime(out long filetime);

        public static DateTime UtcNow
        {
            get
            {
                if (!IsAvailable)
                {
                    throw new InvalidOperationException("High resolution clock isn't available.");
                }

                long filetime;
                GetSystemTimePreciseAsFileTime(out filetime);

                return DateTime.FromFileTimeUtc(filetime);
            }
        }

        static HighResolutionDateTime()
        {
            if (!HasSufficientWindowsVersion(Environment.OSVersion.Platform, Environment.OSVersion.Version))
            {
                IsAvailable = false;
                return;
            }

            // Make sure the API is actually available
            try
            {
                long filetime;
                GetSystemTimePreciseAsFileTime(out filetime);
                IsAvailable = true;
            }
            catch (EntryPointNotFoundException)
            {
                IsAvailable = false;
            }
        }

        /// <summary>
        /// Check whether the given OS is Windows 8, Windows Server 2012 or above.
        /// </summary>
        /// <param name="platformId">PlatformId on which the application is currently running</param>
        /// <param name="windowsVersion">Windows version on which the application is currently running</param>
        /// <returns></returns>
        private static bool HasSufficientWindowsVersion(PlatformID platformId, Version windowsVersion)
        {
            var minimumRequiredVersion = new Version(6, 2, 9200, 0); // Windows 8, Windows Server 2012

            return (platformId == PlatformID.Win32NT) && (windowsVersion >= minimumRequiredVersion);
        }
    }
}