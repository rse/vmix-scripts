
vMix Scripts
============

About
-----

This is a small collection of
[VB.NET](https://en.wikipedia.org/wiki/Visual_Basic_.NET) 2.0 scripts
for automating certain tasks and extending the functionality in the
video/audio stream mixing software [vMix](https://www.vmix.com/).

The individual scripts are:

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
  allowing one to step forward/backward through an event configuration
  by re-configuring four NDI input sources (for shared content, one
  moderator P1 and two presenters P1 and P3).

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

- [stage-gate.vb](stage-gate.vb):<br/>
  This is a VB.NET 2.0 script for the vMix 4K/Pro scripting facility,
  allowing the stage inputs (monitored on Bus-A) to be temporarily
  "dimmed" as long as stage outputs (monitored on Bus-B) happen. This
  is somewhat similar to a sidechain-based noise-gate audio filter.
  This way one can prevent nasty echos or even full loops on stage.

- [translator-overspeak.vb](translator-overspeak.vb):<br/>
  This is a VB.NET script for the vMix 4K/Pro scripting facility,
  allowing translators (sitting on vMix Call sources and mixed on the
  Master audio bus and monitored on Bus-C) to over-speak the program
  (received via NDI and mixed on the Master audio bus after being
  "dimming" on Bus-B)

- [audio-sidechain.vb](audio-sidechain.vb):<br/>
  This is a VB.NET script for the vMix 4K/Pro scripting facility,
  allowing audio output volumes to be automatically and temporarily
  reduced, based on audio input volumes (when temporarily above a
  threshold) -- similar to an audio side-chain compression. There are
  two main use-cases for this functionality: stage gate and translator
  over-speaking.

- [ndi-studio-monitor.vb](ndi-studio-monitor.vb):<br/>
  This is a VB.NET 2.0 script for the vMix 4K/Pro scripting facility,
  allowing vMix to re-configure the NDI source displayed in an NDI
  Studio Monitor instance.

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

