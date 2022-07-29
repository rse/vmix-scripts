
[Input-Bridge](input-bridge.vb)
===============================

**Bridge Inputs between vMix instances**

Allow one to bridge/tunnel an arbitrary number of inputs between two
vMix instances with the help of two intermediate NDI-type input(s) in
order to perform load offloading between two vMix instances.

Problem
-------

Sometimes a single vMix instance is overloaded, even on a very powerful
computer. One solution is to offload a bunch of inputs, e.g., camera
inputs and their chroma-key filtering and virtual PTZ positioning, to
a second vMix instance. For instance, when you have 4 cameras and 7
virtual PTZ, you would want to offload the corresponding 28 inputs.
Unfortunately, on a 1 Gbps network link you can transmit a maximum
of eight 1080p30, or six 1080p60, or four 2160p30 or two 2160p60 NDI
streams. You would need at least a 10 Gbps network link and use not more
than 2160p30.

Solution
--------

Usually, even if you could bridge all necessary inputs, the primary
vMix instance usually uses just a single one in the output/program and
another one in the preview. So, it is sufficient to bridge the arbitrary
number of inputs over just two NDI inputs, as long as all bridged inputs
are all singletons and to not occur as a pair in any scene.

Usage
-----

Suppose you you want to offload inputs named `VPTZ - XXX` from the vMix
instance on COMPUTER2 (10.0.0.12) to a pre-processing vMix instance
on COMPUTER1 (10.0.0.11). Then configure the `VPTZ - XXX` inputs on
COMPUTER1 as usual, and then setup COMPUTER2 as follows:

- add the `input-bridge` script to COMPUTER2 and adjust its configuration to:

```
dim remoteAPI              as String  = "http://10.0.0.11:8088/API/"
dim remoteOutputNumber1    as String  = "3"
dim remoteOutputNumber2    as String  = "4"
dim remoteNDIStreamName1   as String  = "COMPUTER1 (vMix - Output 3)"
dim remoteNDIStreamName2   as String  = "COMPUTER1 (vMix - Output 4)"
dim localNDIInputName1     as String  = "BRIDGE1"
dim localNDIInputName2     as String  = "BRIDGE2"
dim localInputNamePrefix   as String  = "VPTZ - "
```

- add two NDI streams named `BRIDGE1` and `BRIDGE2`. Their NDI streams
  will be automatically selected and hence can be arbitrary streams
  initially.

- for all to be bridged inputs `VPTZ - XXX` add inputs of type "Blank"
  and set their layer 1 to `BRIDGE1` or `BRIDGE2`. The actually
  used bridge is overridden afterwards.

