using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Logoff
{
    public static class Tools
    {
        private static void LogExitCall(ExitFlags uFlags, ReasonCodes dwReason)
        {
            Console.Error.WriteLine(
                "Shutdown call: Flags={0}; Reason={1}",
                Tools.FlagsToString(Tools.FlagsToArray(uFlags)),
                Tools.FlagsToString(Tools.FlagsToArray(dwReason)));
        }
        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        /// <summary>
        /// Shutds down the system or logs off the user
        /// </summary>
        /// <param name="uFlags">Type of system shutdown</param>
        /// <param name="dwReason">
        /// The reason for initiating the shutdown. This parameter must be one of the system shutdown reason codes.
        /// If this parameter is zero,
        /// the SHTDN_REASON_FLAG_PLANNED reason code will not be set and therefore the default action is
        /// an undefined shutdown that is logged as "No title for this reason could be found".
        /// By default, it is also an unplanned shutdown. Depending on how the system is configured,
        /// an unplanned shutdown triggers the creation of a file that contains the system state information,
        /// which can delay shutdown. Therefore, do not use zero for this parameter.
        /// Zero is only to be used when <paramref name="uFlags"/> is <see cref="ExitFlags.LOGOFF"/>
        /// </param>
        /// <returns>
        /// true, if the shutdown was initiated.
        /// This does not indicate shutdown success as other applications, the system and the user can abort the shutdown.
        /// </returns>
        [DllImport("User32.dll")]
        private static extern bool ExitWindowsEx(ExitFlags uFlags, ReasonCodes dwReason);

        [Flags]
        public enum ExitFlags : uint
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
            /// This flag has no effect if terminal services is enabled.
            /// Otherwise, the system does not send the WM_QUERYENDSESSION message.
            /// This can cause applications to lose data.
            /// Therefore, you should only use this flag in an emergency.
            /// </summary>
            FORCE = 0x4,
            /// <summary>
            /// Turns off the power after shutdown.
            /// Combined with <see cref="SHUTDOWN"/>
            /// </summary>
            POWEROFF = 0x8,
            /// <summary>
            /// Forces processes to terminate if they do not respond to the
            /// WM_QUERYENDSESSION or WM_ENDSESSION message within the timeout interval.
            /// </summary>
            FORCEIFHUNG = 0x10,
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

        [Flags]
        public enum ReasonCodes : uint
        {
            /// <summary>
            /// No reason given.
            /// <remarks>This is the only valid value for <see cref="ExitFlags.LOGOFF"/></remarks>
            /// </summary>
            NO_REASON = 0
        }

        public static bool DetachFromConsole()
        {
            return FreeConsole();
        }

        public static bool Logoff(bool DryRun = false)
        {
            const ExitFlags exit = ExitFlags.LOGOFF;
            const ReasonCodes reason = ReasonCodes.NO_REASON;
            LogExitCall(exit, reason);
            if (!DryRun)
            {
                return ExitWindowsEx(exit, reason);
            }
            return true;
        }

        public static T[] FlagsToArray<T>(T EnumValues) where T : Enum
        {
            List<T> Values = new List<T>();
            //Make sure the Flags attribute is set
            var Attr = typeof(T).GetCustomAttributes(typeof(FlagsAttribute), false);
            if (Attr == null || Attr.Length == 0)
            {
                throw new Exception($"The enumeration {typeof(T).FullName} is not marked using the 'Flags' attribute");
            }
            //Get the numeric value of this enumeration
            var RawValue = ForceLong(Enum.Format(typeof(T), EnumValues, "d"));
            foreach (var Value in Enum.GetValues(typeof(T)).OfType<T>())
            {
                var FlagValue = long.Parse(Enum.Format(typeof(T), Value, "d"));
                if (RawValue == FlagValue)
                {
                    //No need to evaluate further if the remainder is exactly the current flag value
                    Values.Add(Value);
                    return Values.ToArray();
                }
                if ((RawValue & FlagValue) != 0)
                {
                    RawValue ^= FlagValue;
                    Values.Add(Value);
                }
            }
            if (RawValue != 0)
            {
                Values.Add((T)Enum.ToObject(typeof(T), RawValue));
            }
            return Values.ToArray();
        }

        public static string FlagsToString<T>(IEnumerable<T> Values) where T : Enum
        {
            return string.Join("|", Values.Select(m => m.ToString()).ToArray());
        }

        private static long ForceLong(string Value)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return 0L;
            }
            try
            {
                //Try to parse it as long directly
                return long.Parse(Value);
            }
            catch
            {
                try
                {
                    //Try to parse as unsigned long
                    return (long)ulong.Parse(Value);
                }
                catch
                {
                    try
                    {
                        //Try to parse as double and convert to long
                        return (long)Math.Floor(double.Parse(Value));
                    }
                    catch (Exception ex)
                    {
                        //Give up
                        throw new Exception($"Unable to convert {Value} into type 'long'", ex);
                    }
                }
            }
        }
    }
}
