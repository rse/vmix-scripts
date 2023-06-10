
[Input-Bridge](input-bridge.vb)
===============================

**Bridge Inputs between vMix instances**

Allow one to bridge/tunnel an arbitrary number of inputs between two
vMix instances with the help of two NDI streams in order to perform load
offloading between two vMix instances.

See the corresponding [demonstration video](https://youtu.be/Y6MHAtpMYG8) for details.

Problem
-------

Sometimes a single vMix instance is overloaded, even on a very powerful
computer. One solution is to offload a bunch of inputs, e.g., camera
inputs and their chroma-key filtering and virtual PTZ positioning, to
a second vMix instance. For instance, when you have 4 cameras and 7
virtual PTZ, you would want to offload the corresponding 28 inputs.
Unfortunately, on a 1 Gbps network link you can transmit just a maximum
of eight 1080p30, or six 1080p60, or four 2160p30, or two 2160p60, NDI
streams between the two vMix instances. You would need at least a 10
Gbps network link and use not more than 2160p30 if all the 28 inputs
should be used on both vMix instances in parallel.

Solution
--------

Usually, even if you could bridge all 28 inputs, the primary vMix
instance uses just a single one in its output/program and potentially
another single one in its preview. So, in practice it is usually
sufficient to bridge the arbitrary number of inputs over just two NDI
streams, as long as all bridged inputs are singletons and do not occur
as a pair in any scene and at any time.

Usage
-----

Suppose you you want to offload inputs named `VPTZ - XXX` from the vMix
instance on COMPUTER2 (10.0.0.12) to a pre-processing vMix instance on
COMPUTER1 (10.0.0.11). Then setup COMPUTER1 as following:

- add the `VPTZ - XXX` inputs as usual (perhaps as the leaves of
  a complex input hierarchy like Camera / Virtual Input / VirtualSet).

- add two inputs of type *Mix* named `BRIDGE1` and `BRIDGE2` and
  under their *Settings / Outputs* enable NDI for the outputs #3 and #4 and
  set these two inputs.

Then setup COMPUTER2 as following:

- add the `input-bridge` script to COMPUTER2 and adjust its configuration to:

```
dim peerAPI          as String  = "http://10.0.0.11:8088/API/"
dim bridge1MixNum    as String  = "1"
dim bridge2MixNum    as String  = "2"
dim bridge1InputName as String  = "BRIDGE1"
dim bridge2InputName as String  = "BRIDGE2"
dim timeSlice        as Integer = 50
dim debug            as Boolean = true
```

- add two inputs of type *NDI* named `BRIDGE1` and `BRIDGE2`. Their NDI
  streams should be set to `COMPUTER1 (vMix - Output 3)` and `COMPUTER1
  (vMix - Output 4)`.

- for all the to be bridged inputs `VPTZ - XXX` (from COMPUTER1) add inputs of type Blank
  and set their layer 1 initially to just `BRIDGE1`. The actually used
  bridge input (`BRIDGE1` or `BRIDGE2`) is automatically selected
  afterwards and hence you can use any bridge input initially.

Finally, on COMPUTER2, start the script `input-bridge` and bring any of
the `VPTZ - XXX` inputs into preview (and then potentially cut it to
output/program) and observe that the script automatically adjusts the
layer 1 on the `VPTZ - XXX` inputs (it switches between `BRIDGE1` and
`BRIDGE2` when necessary) and it automatically adjusts the Mix inputs
`BRIDGE1` and `BRIDGE2` on COMPUTER1 to have the particular `VPTZ - XXX`
inputs as their active input.

