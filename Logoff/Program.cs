using System;
using System.Runtime.InteropServices;

namespace Logoff
{
    class Program
    {
        [DllImport("User32.dll")]
        private static extern bool ExitWindowsEx(ExitFlags uFlags, int Y);

        [Flags]
        private enum ExitFlags : uint
        {
            /// <summary>
            /// Logs off the user.
            /// Can't be combined with different flags
            /// </summary>
            LOGOFF = 0x0,
            /// <summary>
            /// Performs a regular shutdown.
            /// Optional flags: <see cref="POWEROFF"/>, <see cref="HYBRID_SHUTDOWN"/>
            /// </summary>
            SHUTDOWN = 0x1,
            /// <summary>
            /// Shuts down the system and restarts it
            /// </summary>
            REBOOT = 0x2,
            /// <summary>
            /// Turns off the power after shutdown.
            /// Combined with <see cref="SHUTDOWN"/>
            /// </summary>
            POWEROFF = 0x8,
            /// <summary>
            /// Similar to <see cref="REBOOT"/> but restarts currently running applications
            /// </summary>
            RESTART_APPS = 0x40,
            /// <summary>
            /// Performs a hybrid shutdown which allows Windows to start faster.
            /// Combined with <see cref="SHUTDOWN"/>
            /// </summary>
            HYBRID_SHUTDOWN = 0x400000
        }

        static void Main(string[] args)
        {
        }
    }
}
