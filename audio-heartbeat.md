
[Audio-Heartbeat](audio-heartbeat.vb)
=====================================

**Detect Unexpected Silence**

Notify operator in case unexpected silence, i.e., audio below a certain
threshold on the master bus, is detected during streaming and/or
recording.

Problem
-------

Sometimes microphones were forgotten to be enabled, the wrong
microphones were enabled or the wrong microphones were given to a particular talent.

Solution
--------

In all problem cases, during streaming/recording the master bus audio
volume drops below a certain threshold (e.g. -32 dB FS) for longer than
a certain time (e.g. 5000 ms). If this situation is detected, the vMix
operator is notified through regular system sounds (e.g. every 2000 ms).

Usage
-----

Just run the script `audio-heartbeat` in the background.

