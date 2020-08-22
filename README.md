# Logoff

This is a replacement and improvement for the logoff command, which is absent in Windows 10 Home.

## How to use

Command line: `logoff.exe [timeout [/d]] [/hide] [/fake]`

A short command line help is available by supplying `/?` as argument.
This will ignore any other arguments present and just display the help.

### `timeout`

This is how long to wait for the logoff.
**Windows does not support this feature so it's emulated.**

To emulate the timeout, this application implements its own timer.
This means you must not exit the application or a scheduled logoff will not happen.

This value is treated as a number of seconds,
unless `/d` is specified (see below).

- No value: replaced with zero
- Negative value: replaced with zero
- Invalid value: application exits with failure code

### `/d`

Treats the timeout as a full date instead of as a number of seconds.
Because most date formats on earth contain spaces,
you should enclose the date in quotes.

The application will first try to parse the date in this order:

1. `yyyy-MM-dd HH:mm:ss` (year-month-day 24h-min-sec)
2. `yyyy-MM-dd HH:mm` (year-month-day 24h-min)
3. Local region specific date and time format (fallback)

It's recommended that you always use one of the first two formats.
The third format acts as a fallback.
It uses [`DateTime.Parse`](https://docs.microsoft.com/en-us/dotnet/api/system.datetime.parse) which parses various different date formats,
including just a date or just the time, which is likely not what you want.
Unless you are very sure about what you do, always pass the date using one of the first two formats.

Treatment of special `timeout` parameter values if `/d` is specified:

- No value: application exits with failure code
- Negative value (date in past): Logoff happens immediately
- Invalid value: application exits with failure code

### `/hide`

Detaches the process from the console window.

This has several effects:

- Command line scripts will continue immediately and not wait for the process to exit
- The exit code can't be evaluated by scripts
- The console is closed if no other process or script is using it
- The logoff action cannot be aborted using `[CTRL]`+`[C]` anymore

### `/fake`

This will not perform the actuall logoff action.
Useful for testing purposes.

## Application exit codes

- **0**: The operation completed successfully
- **1**: General error with arguments (duplicates, missing arguments, unknown argument)
- **2**: Problem parsing the `timeout` value

Note:
Code zero doesn't mean that the logoff will actually happen.
The user, other applications and the system can cancel a pending logoff operation.

## Logoff considerations

Logging out users is much more limited in Windows than the shutdown operation is.

You can only log out interactive users.
This means you can't run this application from a service.
It also means you can't log out other users.

To log out another user, you have to run logoff.exe in that users session.

An easy way to do this is to set up a task in the task scheduler.
Use the user you want to log out as the user account for the task
and set it to only run if the user is logged in.
Then run the task manually and delete it afterwards.
