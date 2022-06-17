
vMix Scripts
============

About
-----

This is a small collection of
[VB.NET](https://en.wikipedia.org/wiki/Visual_Basic_.NET) 2.0 scripts
for automating certain tasks and extending the functionality in the
video/audio stream mixing software [vMix](https://www.vmix.com/).

The individual scripts are:

- [audio-sidechain.vb](audio-sidechain.vb):<br/>
  This is a VB.NET script for the vMix 4K/Pro scripting facility,
  allowing audio output volumes to be automatically and temporarily
  reduced, based on audio input volumes (when temporarily above a
  threshold) -- similar to an audio side-chain compression. There are
  two main use-cases for this functionality: stage gate and translator
  over-speaking.

- [clone-input.vb](clone-input.vb):<br/>
  This is a VB.NET 2.0 script for the vMix 4K/Pro scripting facility,
  allowing an arbitrary input (which has to be in the preview) to
  be cloned/duplicated. This is somewhat similar to the "Settings"
  / "Copy from..." functionality of an input (but which copies
  not everything) or the "Settings" / "General" / "Create Virtual
  Input" functionality of an input (but which still attaches to the
  original). Instead, this script performs a real clone of an input by
  directly operating on the underlying vMix preset XML file.

- [event-reconfiguration.vb](event-reconfiguration.vb):<br/>
  This is a VB.NET 2.0 script for the vMix 4K/Pro scripting facility,
  allowing one to step forward/backward through (or to a particular row
  of) an event configuration by re-configuring four NDI input sources
  (for shared content, one moderator P1 and two presenters P1 and P3).

- [event-title-control.vb](event-title-control.vb):<br/>
  This is a VB.NET 2.0 script for the vMix 4K/Pro scripting facility,
  allowing one to control the in/out transitioning of titles which
  are embedded layers of scene inputs (where vMix only performs
  TransitionIn and never a TransitionOut). Instead, the title has to
  make all elements Hidden on TransitionIn and TransitionOut and do
  the in/out transitioning on Page1/Page2 instead. Additionally, this
  script ensures that independent of arbitrary scene input changes, the
  titles are shown just for durationVisible seconds and at maximum every
  durationLocked seconds.

- [remoteshowcontrol-once.vb](remoteshowcontrol-once.vb):<br/>
  This is a VB.NET 2.0 script for the vMix 4K/Pro scripting facility,
  allowing one to send commands to the [Irisdown Remote Show
  Control](https://www.irisdown.co.uk/rsc.html) plugin of PowerPoint.
  This allows a vMix operator to step forward/backward through an
  ingested (screen or HDMI capturing) PowerPoint presentation.

- [remoteshowcontrol-loop.vb](remoteshowcontrol-loop.vb):<br/>
  This is a VB.NET 2.0 script for the vMix 4K/Pro scripting facility,
  allowing one to attach particular slides an ingested (screen or
  HDMI capturing) PowerPoint presentation to a vMix input. This works
  by observing which input is in preview and if its title contains
  "[rsc:N]" this script instructs PowerPoint, through the [Irisdown
  Remote Show Control](https://www.irisdown.co.uk/rsc.html) plugin, to
  go to the particular slide N.

- [ndi-studio-monitor.vb](ndi-studio-monitor.vb):<br/>
  This is a VB.NET 2.0 script for the vMix 4K/Pro scripting facility,
  allowing vMix to re-configure the NDI source displayed in an NDI
  Studio Monitor instance.

- [smooth-pan-zoom.vb](smooth-pan-zoom.vb):<br/>
  This is a VB.NET 2.0 script for the vMix 4K/Pro scripting facility,
  which allows one to smoothly adjust the pan/zoom of an input. This is
  usually used for a rough emulation of the Virtual PTZ feature, which
  cannot be used on layered inputs like Virtual Sets.

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

