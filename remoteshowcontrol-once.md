
RemoteShowControl-Once
======================

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

Solution
--------

The PowerPoint plugin [Irisdown
RemoteShowControl](https://www.irisdown.co.uk/rsc.html) is installed
into PowerPoint and listens on a TCP port for control commands. This
script can be run to instruct PowerPoint, through its installed Irisdown
Remote Show Control plugin, to go to the previous, next or a particular
slide.

Usage
-----

Configure vMix Shortcuts or Triggers with:

    <key1> SetDynamicValue1 PREV
    <key1> ScriptStart      remoteshowcontrol-once

    <key2> SetDynamicValue1 NEXT
    <key2> ScriptStart      remoteshowcontrol-once

    <key3> SetDynamicValue1 <N>
    <key3> ScriptStart      remoteshowcontrol-once

