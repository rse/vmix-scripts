
[Input-Mirror](input-mirror.vb)
===============================

**Mirror Input Selection on vMix Slave**

Allow one to mirror the current input preview/program selection
on vMix slave instances in order to closely follow the vMix master instance.

Problem
-------

Sometimes a single vMix instance is not enough. For instance, when on a
"playout" machine two incoming SRT streams should each be forwarded to
multiple outgoing RTMP endpoints. The solution is to run vMix twice on
the same machine: once for the first SRT stream and its RTMP endpoints
and a once again for the second SRT stream and its RTMP endpoints.

Solution
--------

For a "playout" machine, one has to be able to switch between the
incoming SRT streams and various "placeholder" inputs ("before-event",
"after-event", "failure", "emergency", etc). As both vMix instances
have the same set of inputs, one can simply mirror the inputs in
preview/program in the vMix master instance 1:1 on the vMix slave
instance.

Usage
-----

Suppose you you want to mirror inputs from the vMix
master instance to a vMix slave instance whose
HTTP API is under port 8188 (instead of the regular
port 8088 of the vMix master instance).
Then setup the vMix master instance as following:

- add the `input-mirror` script and adjust its configuration to:

```
dim peerAPI          as String  = "http://127.0.0.1:8188/API/"
dim timeSlice        as Integer = 50
dim debug            as Boolean = true
```

Finally, start the script `input-mirror` and bring any of the inputs
into preview/program.

