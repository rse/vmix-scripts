
Clone-Input
===========

**Really Cloning an Arbitrary Input**

Allow an arbitrary input (which has to be in the preview) to be
really cloned/duplicated.

Problem
-------

vMix has to real input cloning functionality. It only has the "Settings"
/ "Copy from..." functionality of an input, but this copies not
everything. Also, the "Settings" / "General" / "Create Virtual Input"
functionality of an input still attaches the resulting input to the
original one. But when creating dozens of event scenes, one usually
wants to clone existing scenes.

Solution
--------

This script performs a real clone of an input by directly operating
on the underlying vMix preset XML file. For this to work correctly,
ensure that you are running on an already saved vMix preset (which is
usually always the case in production, except when you are trying out
this script on a freshly started vMix).

Usage
-----

Configure a vMix Shortcut with:<br/>
*key* `ScriptStart` `clone-input`

