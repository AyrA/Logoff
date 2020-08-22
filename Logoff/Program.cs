using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;

namespace Logoff
{
    class Program
    {
        private struct EXIT
        {
            public const int SUCCESS = 0;
            public const int ARG = 1;
            public const int DATE = 2;
        }

        private const string DUPLICATE_ERR = "Duplicate argument: {0}\r\nUse /? for help";
        private const string DATE_FORMATS = "yyyy-MM-dd HH:mm:ss|yyyy-MM-dd HH:mm";
        private const DateTimeStyles DateStyle = DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal;

        static int Main(string[] args)
        {
#if DEBUG
            args = "5|/hide".Split('|');
#endif

            var LowerArgs = args.Select(m => m.ToLower()).ToArray();
            var IsDate = false;
            var HideTimer = false;
            var DryRun = false;
            var TimeoutUnparsed = (string)null;

            DateTime LogoffTime = DateTime.UtcNow;

            if (args.Contains("/?"))
            {
                Help();
                return EXIT.SUCCESS;
            }

            foreach (var Arg in LowerArgs)
            {
                switch (Arg)
                {
                    case "/d":
                        if (IsDate)
                        {
                            Console.Error.WriteLine(DUPLICATE_ERR, Arg);
                            return EXIT.ARG;
                        }
                        IsDate = true;
                        break;
                    case "/fake":
                        if (DryRun)
                        {
                            Console.Error.WriteLine(DUPLICATE_ERR, Arg);
                            return EXIT.ARG;
                        }
                        DryRun = true;
                        break;
                    case "/hide":
                        if (HideTimer)
                        {
                            Console.Error.WriteLine(DUPLICATE_ERR, Arg);
                            return EXIT.ARG;
                        }
                        HideTimer = true;
                        break;
                    default:
                        if (TimeoutUnparsed != null)
                        {
                            Console.Error.WriteLine($"Unknown argument: {Arg}");
                            Console.Error.WriteLine("Use /? for help");
                            return EXIT.ARG;
                        }
                        TimeoutUnparsed = Arg;
                        break;
                }
            }
            //If we are still here, all arguments are OK.
            if (IsDate)
            {
                if (string.IsNullOrEmpty(TimeoutUnparsed))
                {
                    TimeoutUnparsed = "0";
                    LogoffTime = DateTime.Now;
                }
                else if (!DateTime.TryParseExact(TimeoutUnparsed, DATE_FORMATS.Split('|'), CultureInfo.CurrentCulture, DateStyle, out LogoffTime))
                {
                    if (!DateTime.TryParse(TimeoutUnparsed, CultureInfo.CurrentCulture, DateStyle, out LogoffTime))
                    {
                        Console.Error.WriteLine("Failed to parse {0} as date.", TimeoutUnparsed);
                        return EXIT.DATE;
                    }
                }
            }
            else
            {
                //Apply default timeout
                if (string.IsNullOrEmpty(TimeoutUnparsed))
                {
                    TimeoutUnparsed = "0";
                }
                if (long.TryParse(TimeoutUnparsed, out long temp))
                {
                    LogoffTime = DateTime.Now.AddSeconds(temp);
                }
                else
                {
                    Console.Error.WriteLine("Failed to parse {0} as a number of seconds.", TimeoutUnparsed);
                    return EXIT.DATE;
                }
            }
#if DEBUG
            //Don't actually log off in debug mode
            DryRun = true;
#endif
            if (HideTimer)
            {
                Tools.DetachFromConsole();
            }
            else
            {
                Console.WriteLine("This user is about to be logged out.\r\n" +
                    "Please save all unsaved work before the timer expires.");
            }
            var Pos = new
            {
                X = HideTimer ? 0 : Console.CursorLeft,
                Y = HideTimer ? 0 : Console.CursorTop
            };
            while (LogoffTime > DateTime.Now)
            {
                if (!HideTimer)
                {
                    var Timeout = LogoffTime.Subtract(DateTime.Now);
                    if (Timeout.TotalSeconds >= 0)
                    {
                        Console.SetCursorPosition(Pos.X, Pos.Y);
                        var text = string.Empty;
                        var timeformat = @"hh\:mm\:ss";
                        if (Timeout.Days > 0)
                        {
                            if (Timeout.Days == 1)
                            {
                                timeformat = @"\1\ \d\a\y\,\ " + timeformat;
                            }
                            else
                            {
                                timeformat = @"d\ \d\a\y\s\,\ " + timeformat;
                            }
                        }
                        text += string.Format("Time remaining: {0}", Timeout.ToString(timeformat)).PadRight(Console.BufferWidth);
                        text += string.Format("Time of logoff: {0} {1}",
                            LogoffTime.ToLongDateString(),
                            LogoffTime.ToLongTimeString()).PadRight(Console.BufferWidth);
                        Console.Write(text);
                    }
                }
                //Delay compensated second timer
                Thread.Sleep(1000 - DateTime.Now.Millisecond);
            }
            if (!HideTimer)
            {
                Console.WriteLine("LOGOFF");
            }
            Tools.Logoff(DryRun);
#if DEBUG
            if (!HideTimer)
            {
                Console.ReadKey(true);
            }
#endif
            return EXIT.SUCCESS;
        }

        private static void Help()
        {
            Console.WriteLine(@"Logoff.exe [timeout [/D] [/hide]] [/fake]
Logs the user out of the current session

timeout  - Timeout until the user is logged off.
           Windows does not supports this natively.
           The timeout is simulated in the application.
           Terminating the application prior to the timeout
           will not log off the user.
           If the number is negative, it's treated as being zero.
           If no timeout is given, the user is logged out immediately.
/D       - Timeout specifies an absolute date. See below for the format.
/hide    - Hides the timer. Only valid if a timeout is specified.
           This releases the console handle.
           To abort a pending logoff, the process has to be killed.
           If this is not specified, the logoff can be aborted using
           CTRL+C or by closing the console window.
/fake    - Will not actually perform the logoff call.

Format for timeout without /D
=============================
Digits 0-9 only
This is interpreted as a value in seconds.
3600 is an hour and 86400 is a day.

Digits with at least one colon ':'
This is treated as a time in [hh:]mm:ss format.
The hour is optional.

Format for timeout with /D
==========================
With /D, the timeout represents a date in the future.
Be sure to encapsulate the date in double quotes.
The application tries to parse the date in two forms:

First, it tries a local ISO date (yyyy-MM-dd HH:mm:ss).
If this fails, it tries to parse the date using the current locale.
If this also fails, the application aborts.

If the parsed date is in the past, the logoff is performed immediately.
");
        }
    }
}
