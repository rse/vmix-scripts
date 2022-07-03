
Auto-Pre-Mix
============

**Automatically Pre-Mixing Inputs**

Allow one to auto-pre-mix (aka pre-render or flattening) source
inputs with the help of two intermediate Mix-type input(s) in order to
further embed the result onto a layer of a target input.

Problem
-------

vMix is an awesome video mixing software, but it is still rather limited
(especially in contrast to OBS Studio) when it comes to its input
layering: when creating complex input trees the maximum layering depth
is 2, and when applying effects like positioning there is only the
single, inherited, original positioning. The reason is the design
weakness of vMix that no pre-rendering/flattening of layers actually
happens.

In practice the following scenario of a greenscreen-based and
virtual-PTZ-based scenario illustrates the problem: The *N* physical
cameras are on inputs `PCAM-`*N*. For applying a chroma-key filter
(without changing the original input), *N* virtual inputs `VCAM-`*N*
are created from the `PCAM-`*N* inputs. For replacing the greenscreen
with *N* pre-rendered background images (inputs `BG-`*N*) and performing
*K* virtual pan/tilt/zoom (PTZ), *N*x*K* `VPTZ-`*N*`-`*xxx* inputs (of
type Virtual Set / Blank) are created with each having the background
image `BG-`*N* on layer 1, the `VCAM-`*N* on layer 2 and an individual
Position for the PTZ. *X* scene inputs (*X* &lt;= *N*x*K*) `SCENE-`*X*
can now be created with each having `VPTZ-`*N*`-`*xxx* on layer 1,
and e.g. slides and titles on additional layers.

Unfortunately, when a slide is too unreadable and you want to exchange
the virtual camera `VPTZ-`*N*`-`*xxx* and the slide by creating
a special scene `SCENE-PIP` with the slide on layer 1 and the virtual camera
`VPTZ-`*N*`-`*xxx* on layer 2 (as a PIP) you cannot achieve the PIP
effect because the Position of layer 2 interferes with the Position
already in the Virtual Set `VPTZ-`*N*`-`*xxx*. Even a virtual input from
`VPTZ-`*N*`-`*xxx* doesn't help here.

Solution
--------

Fortunately, vMix 4K/Pro have Mix input types which actually perform
pre-rendering/flattening of inputs -- even if they are primarily
intended for switching between inputs. Unfortunately, vMix provides only
a maximum of just 3(!) of those Mix input types. But we can re-use a Mix
input type by dynamically re-configuring it to show (and pre-render) a
particular input when it a scene containing it comes into PREVIEW. In
order to not change the PROGRAM, two Mix inputs are used in total.

For our practical scenario, you create two Mix-type inputs `PRERENDER1`
and `PRERENDER2` and in the PIP-scene `SCENE-PIP` you now place the
slide on layer 1, `PRERENDER1` on layer 2 and the virtual camera
`VPTZ-`*N*`-`*xxx* on layer 3, but disabled. The Position for the
PIP is now done on layer 2 instead of 3. When the `SCENE-PIP` comes
into PREVIEW, the `auto-pre-mix` script detects the scenario and
automatically switches `PRERENDER1` to `VPTZ-`*N*`-`*xxx*. In case
PROGRAM already uses `PRERENDER1`, the `SCENE-PIP` is re-configured to
use `PRERENDER2` and then this Mix is switched.

Usage
-----

Create two *Mix*-type inputs `PRERENDER1` and `PRERENDER2` and on any
target input, place any *Mix* input onto layer *N* and any source input
on layer *N*+1.

