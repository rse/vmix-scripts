
[NDI-Studio-Monitor](ndi-studio-monitor.vb)
===========================================

**Reconfigure NewTek NDI Studio Monitor**

Allow vMix to reconfigure the NDI source displayed in a (remote) NewTek
NDI Studio Monitor instance.

Problem
-------

The NewTek NDI Studio Monitor is a great way to display an NDI stream.
It even supports Picture-in-Picture (PiP). Unfortunately, it usually
runs on a different computer (usually a mini-PC attached to a TV) than
vMix and during a production might have to switch its NDI stream (in
concert with vMix scene inputs or a shortcut used by the operator).

Solution
--------

The NewTek NDI Studio Monitor can be
[remote controlled](https://github.com/bitfocus/companion-module-newtek-ndistudiomonitor/blob/master/HELP.md)
via a small HTTP interface. We call this interface directly from within vMix in
order to reconfigure the shown NDI stream.

Usage
-----

Configure a vMix Shortcut with:

    <key> SetDynamicValue1 <ip-address>:<port>
    <key> SetDynamicValue1 <ndi-source-name>
    <key> ScriptStart      ndi-studio-monitor

