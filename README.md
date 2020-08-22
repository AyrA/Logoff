# Logoff

This is a replacement and improvement for the logoff command, which is absent in Windows 10 Home

## How to use

Command line: `logoff.exe [timeout [/d]] [/hide] [/fake]`

### `timeout`

This is how long to wait for the logoff.
**Windows does not support this feature so it's emulated.**

To emulate the timeout, this application implements its own timer.

This value is treated as a number of seconds,
unless `/d` is specified (see below).

If the argument is not provided, the user is logged out immediately.
If the number is negative, it's replaced with zero.