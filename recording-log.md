
[Recording-Log](recording-log.vb)
=================================

**Logging Recording States**

Logs the start and stop states of Recording and MultiCorder and can add
a special marking log entry for bookkeeping special points of interest
during a recording.

Problem
-------

vMix has the `WriteDurationToRecordingLog` which can be used for adding
a marker to the regular vMix logfile with the help of a shortcut.
Unfortunately, this works only for regular Recording and not for
MultiCorder.

Solution
--------

An own logging facility detects the start and stop states of Recording
and MultiCorder in parallel and in addition provides a way to add
a special marking log entry (through a shortcut) for bookkeeping
special points of interest during a recording.

The following log is the result of starting Recording, pressing the
marker shortcut two times in sequence, then starting MultiCorder,
pressing the marker shortcut two times in sequence once again, then
stopping MultiCorder, pressing the marker shortcut two times in sequence
once again, and finally also stopping Recording:

```txt
[2022-07-04 23:59:39.184] RECORDING   started
[2022-07-04 23:59:41.130] RECORDING   marked  (position: 00:00:01.946)
[2022-07-04 23:59:42.555] RECORDING   marked  (position: 00:00:03.370)
[2022-07-04 23:59:44.514] MULTICORDER started
[2022-07-04 23:59:47.101] RECORDING   marked  (position: 00:00:07.916)
[2022-07-04 23:59:47.101] MULTICORDER marked  (position: 00:00:02.586)
[2022-07-04 23:59:48.558] RECORDING   marked  (position: 00:00:09.373)
[2022-07-04 23:59:48.558] MULTICORDER marked  (position: 00:00:04.043)
[2022-07-04 23:59:52.456] MULTICORDER ended   (duration: 00:00:07.942)
[2022-07-04 23:59:53.518] RECORDING   marked  (position: 00:00:14.333)
[2022-07-04 23:59:54.282] RECORDING   marked  (position: 00:00:15.098)
[2022-07-04 23:59:56.820] RECORDING   ended   (duration: 00:00:17.636)
```

Usage
-----

Configure vMix Shortcuts with:

    <key> SetDynamicValue3 trigger-marker

