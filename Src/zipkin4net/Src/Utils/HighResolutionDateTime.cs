using System;
#if !NET_CORE
using System.Runtime.InteropServices;
#endif

namespace zipkin4net.Utils
{
    internal static class HighResolutionDateTime
    {
#if NET_CORE
        public static bool IsAvailable { get { return false; } }
#else
        public static bool IsAvailable { get; private set; }
#endif

#if !NET_CORE
        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern void GetSystemTimePreciseAsFileTime(out long filetime);
#endif

        public static DateTime UtcNow
        {
            get
            {
#if !NET_CORE
                if (!IsAvailable)
                {
                    throw new InvalidOperationException("High resolution clock isn't available.");
                }

                long filetime;
                GetSystemTimePreciseAsFileTime(out filetime);

                return DateTime.FromFileTimeUtc(filetime);
#else
                throw new NotImplementedException();
#endif
            }
        }

#if !NET_CORE
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
#endif
    }
}