
[RemoteShowControl-Once](remoteshowcontrol-once.vb)
===================================================

**Once Control Irisdown RemoteShowControl**

Once control a remote PowerPoint slide-deck with the help of its
[Irisdown RemoteShowControl](https://www.irisdown.co.uk/rsc.html)
plugin, based on vMix triggers or shortcuts.

Problem
-------

When ingesting the screen or HDMI captured output of a PowerPoint
slide-deck into vMix scene inputs one has to ensure that the correct
slide is selected within PowerPoint. Often, PowerPoint is running on a
different computer than vMix, too.

Additionally, when presenting slides with PowerPoint and teleprompter
information with applications like QPrompt (which has to be
interactively speed-adjusted by an operator to follow the speaker) in
parallel, PowerPoint cannot be controlled with regular input devices
like Logitech Presenter anymore, as this requires PowerPoint to
have focus (in parallel to QPrompt) and under Windows only a single
application can be focused.

Solution
--------

The PowerPoint plugin [Irisdown
RemoteShowControl](https://www.irisdown.co.uk/rsc.html) is installed
into PowerPoint and listens on a TCP port for control commands. This
script can be run to instruct PowerPoint, through its installed Irisdown
Remote Show Control plugin, to go to the previous, next or a particular
slide.

As IrisDown RemoteShowControl is a PowerPoint plugin which receives the
commands via TCP, PowerPoint isn't required to be focused, too.

Usage
-----

Configure vMix Shortcuts or Triggers with:

    <key1> SetDynamicValue1 PREV
    <key1> ScriptStart      remoteshowcontrol-once

    <key2> SetDynamicValue1 NEXT
    <key2> ScriptStart      remoteshowcontrol-once

    <key3> SetDynamicValue1 <N>
    <key3> ScriptStart      remoteshowcontrol-once

