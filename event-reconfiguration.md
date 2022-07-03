
Event-Reconfiguration
=====================

**Reconfiguration of Event NDI Inputs (and Lower-Third Titles)**

This is a VB.NET 2.0 script for the vMix 4K/Pro scripting
facility, allowing one to step forward/backward through (or
to a particular row of) an [Excel-based conference event
configuration](event-reconfiguration.xlsx) by re-configuring four
reusable NDI input sources (for shared content, one moderator P1 and two
presenters P2 and P3).

See also the [Event-Title-Control](event-title-control.md) for the
companion script to control the Lower-Third Titles of the people.

Problem
-------

For recorded, remote-only, online-only conference events, you want to
chronologically step through the event phases, where in each phase you
have to ingest a moderator camera, up to two presenters cameras and a
shared screen of one of the presenters.

Unfortunately, although vMix Call is very convenient for ingesting
people, it is too limited. It supports only up to 720p (which can be
acceptable) and especially does not directly and easily support that
a presenter camera and screen can be shared in parallel (which is not
acceptable for presenters).

Additionally, even if presenters would accept having to fiddle around
with two independent vMix Call browser tabs (one for the camera, one for
the screen sharing), this also effectively limits the solution to just
5 parallel people being online (as vMix supports only up to 8 vMix Call
instances, even in vMix Pro, and a moderator and its screen, plus two
presenters and their screen, AND at least the next two presenters and
their screen, already requires all the available 8 vMix Call instances).

Solution
--------

The alternative [VDON Call](https://github.com/rse/vdon-call/) is
a WebRTC/NDI-based remote caller ingest solution for live video
productions, based on the two swiss army knifes in their field:
the awesome, low-latency, P2P-based video streaming facility
[VDO.Ninja](https://vdo.ninja), and the awesome, ultra-flexible video
mixing software [OBS Studio](https://obsproject.org).

VDON Call allows you to ingest up to 16 streams which means 8 cameras
plus up to 8 screen sharings -- all in parallel and in advance. This
especially means that usually all people and their screens can be online
in prepared in advance to the event. When ingesting the VDO.Ninja-based
VDON Call streams via OBS Studio, you get 16 NDI A/V-streams on your
network.

In an [Excel-based event configuration](event-reconfiguration.xlsx) you
pre-configure your conference event by splitting it into phases 1-N and
in each phase you configure the moderator, the up to two presenters and
their screen sharing (plus the names and titles of the people).

The **Event-Reconfiguration** facility then allows you to step
forward/backward through (or to a particular row of) this event
configuration and re-configures four reusable NDI input sources (for
shared content, one moderator P1 and two presenters P2 and P3). The crux
is the use of a [full-screen GT title](event-reconfiguration.gtzip)
input which holds the four cell information of the underlying [Excel
data source](event-reconfiguration.xlsx) inside vMix between calls to
this script.

Usage
-----

Create four NDI inputs and map their input name to the Excel
field names in the [XML mapping file](event-reconfiguration.xml).
Also create a input based on the information holding [GT
title](event-reconfiguration.gtzip). Finally, configure the following
vMix Shortcuts for reconfiguring the NDI inputs during the event:

    # switch to previous event configuration row
    <key1> SetDynamicValue1 PREV
    <key1> SetDynamicValue2 <path-to-xml-file>
    <key1> ScriptStart      event-reconfiguration

    # switch to next event configuration row
    <key2> SetDynamicValue1 NEXT
    <key2> SetDynamicValue2 <path-to-xml-file>
    <key2> ScriptStart      event-reconfiguration

    # switch to particular event configuration row)
    <key3> SetDynamicValue1 42
    <key3> SetDynamicValue2 <path-to-xml-file>
    <key3> ScriptStart      event-reconfiguration

