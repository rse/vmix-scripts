
RemoteShowControl-Loop
======================

**Continuously Control Irisdown RemoteShowControl**

Automatically and continuously control a remote
PowerPoint slide-deck with the help of its [Irisdown
RemoteShowControl][https://www.irisdown.co.uk/rsc.html] plugin,
based on vMix input name information.

Problem
-------

When ingesting the screen or HDMI captured output of a PowerPoint
slide-deck into vMix scene inputs one has to ensure that the correct
slide is selected within PowerPoint. Often, PowerPoint is running on a
different computer than vMix, too.

Solution
--------

The PowerPoint plugin [Irisdown
RemoteShowControl][https://www.irisdown.co.uk/rsc.html] is installed
into PowerPoint and listens on a TCP port for control commands.

This script continuously observes which vMix input is in PREVIEW and if
its name contains the string `[rsc:N]`, this script remotely instructs
PowerPoint, through its installed Irisdown Remote Show Control plugin,
to go to the particular slide number `N`.

Usage
-----

On a scene input embed the screen or HDMI captured slide content as a
layer and add to the end of the input name e.g. `[rsc:42]`. Whenever
this scene input comes into PREVIEW, PowerPoint switches to slide number
42 and as a result, the input scene in vMix shows the correct slide
before being cut into PROGRAM.

