
[Smooth-Pan-Zoom](smooth-pan-zoom.vb)
=====================================

**Smooth Virtual Pan/Zoom in Virtual Sets**

Smoothly adjust the pan/zoom of an input, for a rough emulation of the
vMix Virtual PTZ feature, which cannot be used on layered inputs like
Virtual Sets.

Problem
-------

In a greenscreen-baded production you have to place a chroma-keyed
camera input over a pre-rendered background image input. In order to
PTZ into such a scene, a virtual PTZ is required. Unfortunately,
the standard "Virtual PTZ" functionality (under input "Settings" / "PTZ")
cannot be used, as it works only on the main content and not on the
layers of an input.

Solution
--------

A "Virtual Set" type input (usually the "Blank" or "Blank 10" one) has
to be used, as only this input allows you to use "Position" for its
main content and its layers in parallel. This script then smoothly
adjusts the pan/zoom of this input for a rough emulation of the original
"Virtual PTZ" feature.

Usage
-----

Configure vMix Shortcuts with:

    <keyX> SetDynamicInput4 <input-name>
    <keyX> SetDynamicValue4 {pan:{up-left|up|up-right|left|reset|right|down-left|down|down-right}|zoom:{increase|reset|decrease}}
    <keyX> ScriptStart      smooth-pan-zoom

Alternatively, for splitting between input selection and operation, configure vMix Shortcuts with:

    <keyA> SetDynamicInput4 <input-name>
    <keyX> SetDynamicValue4 {pan:{up-left|up|up-right|left|reset|right|down-left|down|down-right}|zoom:{increase|reset|decrease}}
    <keyX> ScriptStart      smooth-pan-zoom

