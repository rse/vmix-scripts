
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
Unfortunately, on a 1 Gbps network link you can transmit just a maximum
of eight 1080p30, or six 1080p60, or four 2160p30 or two 2160p60 NDI
streams between the two vMix instances. You would need at least a 10
Gbps network link and use not more than 2160p30 if all the 28 inputs
should be available on both vMix instances again.

Solution
--------

Usually, even if you could bridge all 28 inputs, the primary vMix
instance uses just a single one in its output/program and potentially
another single one in its preview. So, in practice it is usually
sufficient to bridge the arbitrary number of inputs over just two NDI
inputs, as long as all bridged inputs are singletons and do not occur as
a pair in any scene and and any time.

Usage
-----

Suppose you you want to offload inputs named `VPTZ - XXX` from the vMix
instance on COMPUTER2 (10.0.0.12) to a pre-processing vMix instance on
COMPUTER1 (10.0.0.11). Then setup COMPUTER1 as following:

- configure the `VPTZ - XXX` inputs as usual (perhaps the leaves of
  a complex input hierarchy like Camera / Virtual Input / VirtualSet).

- under Settings / Outputs enable NDI for the outputs #3 and #4 and
  set their input to arbitrary initial inputs. The particular inputs
  will be automatically adjusted afterwards by `input-bridge` on
  COMPUTER2.

Then setup COMPUTER2 as follows:

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

- add two NDI inputs named `BRIDGE1` and `BRIDGE2`. Their NDI streams
  will be automatically selected afterwards and hence can be arbitrary
  streams initially.

- for all the be bridged inputs `VPTZ - XXX` (from COMPUTER1) add inputs of type "Blank"
  and set their layer 1 initially to `BRIDGE1`. The actually used bridge
  input is automatically selected afterwards and hence can be any bridge
  input initially.

Finally, on COMPUTER2, start the script `input-bridge` and bring any
of the `VPTZ - XXX` inputs into preview (and then potentially cut it
to output/program) and observe that the script automatically adjust
the layer 1 on the `VPTZ - XXX` inputs (it switches between `BRIDGE1`
and `BRIDGE2` when necessary) and it automatically adjusts the NDI
streams on `BRIDGE1` and `BRIDGE2` (it switches between `COMPUTER1
(vMix - Output3)` and `COMPUTER1 (vMix - Output 4)`) and also observe
on COMPUTER1 that the outputs #3 and #4 (open Settings / Output to see)
are set to the particular `VPTZ - XXX` inputs which should are currently
bridged.

