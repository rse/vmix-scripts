
vMix Scripts
============

About
-----

This is a small collection of
[VB.NET](https://en.wikipedia.org/wiki/Visual_Basic_.NET) 2.0 scripts
for automating certain tasks and extending the functionality in the
video/audio stream mixing software [vMix](https://www.vmix.com/) (4K and Pro editions only).

The individual scripts are:

- [audio-sidechain.vb](audio-sidechain.vb):<br/>
  **Audio Sidechain Compression**<br/>
  Allow audio output volumes to be automatically and temporarily
  reduced, based on audio input volumes (when temporarily above a
  threshold) -- similar to an audio side-chain compression.

- [auto-pre-mix.vb](auto-pre-mix.vb):<br/>
  **Automatically Pre-Mixing Inputs**<br/>
  Allow one to auto-pre-mix (aka pre-render or flattening) source
  inputs with the help of two intermediate Mix-type input(s) in order to
  further embed the result onto a layer of a target input.

- [clone-input.vb](clone-input.vb):<br/>
  **Really Cloning an Arbitrary Input**<br/>
  Allow an arbitrary input (which has to be in the preview) to be
  really cloned/duplicated.

- [event-reconfiguration.vb](event-reconfiguration.vb):<br/>
  **Reconfiguration of Event NDI Inputs (and Lower-Third Titles)**<br/>
  Allow one to step forward/backward through (or to a particular row of)
  an Excel-based conference event configuration by re-configuring four
  reusable NDI input sources (for shared content, one moderator P1 and
  two presenters P2 and P3).

- [event-title-control.vb](event-title-control.vb):<br/>
  **Control Layer-Embedded Titles**<br/>
  Control the in/out transitioning of lower-third titles which are
  embedded layers of scene inputs (where vMix only performs `TransitionIn`
  and never a `TransitionOut`).

- [remoteshowcontrol-loop.vb](remoteshowcontrol-loop.vb):<br/>
  **Continuously Control Irisdown RemoteShowControl**<br/>
  Automatically and continuously control a remote
  PowerPoint slide-deck with the help of its
  [Irisdown RemoteShowControl][https://www.irisdown.co.uk/rsc.html] plugin,
  based on vMix input name information.

- [remoteshowcontrol-once.vb](remoteshowcontrol-once.vb):<br/>
  **Once Control Irisdown RemoteShowControl**<br/>
  Once control a remote PowerPoint slide-deck with the help of its
  [Irisdown RemoteShowControl][https://www.irisdown.co.uk/rsc.html]
  plugin, based on vMix triggers or shortcuts.

- [ndi-studio-monitor.vb](ndi-studio-monitor.vb):<br/>
  **Reconfigure NewTek NDI Studio Monitor**<br/>
  Allow vMix to reconfigure the NDI source displayed in a (remote) NewTek
  NDI Studio Monitor instance.

- [smooth-pan-zoom.vb](smooth-pan-zoom.vb):<br/>
  **Smooth Virtual Pan/Zoom in Virtual Sets**<br/>
  Smoothly adjust the pan/zoom of an input, for a rough emulation of the
  vMix Virtual PTZ feature, which cannot be used on layered inputs like
  Virtual Sets.

Installation
------------

1. Clone this repository:<br/>
   `git clone https://github.com/rse/vmix-scripts`

2. Add the individual scripts to vMix with<br/>
   **Settings** &rarr; **Scripting** &rarr; **Add** &rarr; **Import**

License
-------

Copyright &copy; 2022 Dr. Ralf S. Engelschall (http://engelschall.com/)

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be included
in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

