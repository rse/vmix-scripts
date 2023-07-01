
<img src="https://raw.githubusercontent.com/rse/vmix-scripts/master/vmix-scripts.png" width="300" align="right" alt=""/>

vMix Scripts
============

About
-----

This is a collection of
[VB.NET](https://en.wikipedia.org/wiki/Visual_Basic_.NET) 2.0 scripts
for automating certain tasks and extending the functionality in the
video/audio stream mixing software [vMix](https://www.vmix.com/) (4K and Pro editions only).
All scripts were created in the professional context of a company
filmstudio and its live event productions. The individual scripts are:

- [audio-sidechain.vb](audio-sidechain.vb):<br/>
  [**Audio Sidechain Compression**](audio-sidechain.md)<br/>
  Allow audio output volumes to be automatically and temporarily
  reduced, based on audio input volumes (when temporarily above a
  threshold) -- similar to an audio side-chain compression.
  <br/>
  Use Cases: Microphone Ducking, Translator Voice-Over

- [audio-heartbeat.vb](audio-heartbeat.vb):<br/>
  [**Detect Unexpected Silence**](audio-heartbeat.md)<br/>
  Notify operator in case unexpected silence, i.e., audio below a
  certain threshold on the master bus, is detected during streaming
  and/or recording.
  <br/>
  Use Cases: Playout Operator Hints, Playout Scene Switching

- [auto-pre-mix.vb](auto-pre-mix.vb):<br/>
  [**Automatically Pre-Mixing Inputs**](auto-pre-mix.md)<br/>
  Allow one to auto-pre-mix (aka pre-render or flattening) source
  inputs with the help of two intermediate Mix-type input(s) in order to
  further embed the result onto a layer of a target input.
  <br/>
  Use Cases: Layer Re-Position/Re-Cropping

- [input-bridge.vb](input-bridge.vb):<br/>
  [**Bridge Inputs between vMix instances**](input-bridge.md)<br/>
  Allow one to bridge/tunnel an arbitrary number of inputs between two
  vMix instances with the help of two NDI streams in
  order to perform load offloading between two vMix instances.
  (See the corresponding [demonstration video](https://youtu.be/Y6MHAtpMYG8) for details)
  <br/>
  Use Cases: Separated Ingest/Mixing

- [input-mirror.vb](input-mirror.vb):<br/>
  [**Mirror Input Selection on vMix Slave Instance**](input-mirror.md)<br/>
  Allow one to mirror the current input preview/program selection
  on vMix slave instances in order to closely follow the vMix master instance.
  <br/>
  Use Cases: Separated Playouts

- [multiview-overlay.vb](multiview-overlay.vb):<br/>
  [**Update Custom Multiview Overlays**](multiview-overlay.md)<br/>
  Allow one to update the preview/program overlays of a custom multiview
  by selecting corresponding images in a "List" type input.
  <br/>
  Use Cases: Virtual PTZ Overview

- [smooth-pan-zoom.vb](smooth-pan-zoom.vb):<br/>
  [**Smooth Virtual Pan/Zoom in Virtual Sets**](smooth-pan-zoom.md)<br/>
  Smoothly adjust the pan/zoom of an input, for a rough emulation of the
  vMix Virtual PTZ feature, which cannot be used on layered inputs like
  Virtual Sets.
  <br/>
  Use Cases: Virtual PTZ Adjustment

- [event-reconfiguration.vb](event-reconfiguration.vb):<br/>
  [**Reconfiguration of Event NDI Inputs (and Lower-Third Titles)**](event-reconfiguration.md)<br/>
  Allow one to step forward/backward through (or to a particular row of)
  an Excel-based conference event configuration by re-configuring four
  reusable NDI input sources (for shared content, one moderator and
  two presenters).
  <br/>
  Use Cases: Conference Guest Ingest

- [event-title-control.vb](event-title-control.vb):<br/>
  [**Control Layer-Embedded Titles**](event-title-control.md)<br/>
  Control the in/out transitioning of lower-third titles which are
  embedded layers of scene inputs (where vMix only performs `TransitionIn`
  and never a `TransitionOut`).
  <br/>
  Use Cases: Conference Guest Title Mangement

- [clone-input.vb](clone-input.vb):<br/>
  [**Really Cloning an Arbitrary Input**](clone-input.md)<br/>
  Allow an arbitrary input (which has to be in the preview) to be
  really cloned/duplicated.
  <br/>
  Use Cases: Event Configuration

- [recording-log.vb](recording-log.vb):<br/>
  [**Logging Recording States**](recording-log.md)<br/>
  Logs the start and stop states of Recording and MultiCorder and can
  add a special marking log entry for bookkeeping special points of
  interest during a recording.
  <br/>
  Use Cases: Point of Interest Tracking

- [remoteshowcontrol-loop.vb](remoteshowcontrol-loop.vb):<br/>
  [**Continuously Control Irisdown RemoteShowControl**](remoteshowcontrol-loop.md)<br/>
  Automatically and continuously control a remote
  PowerPoint slide-deck with the help of its
  [Irisdown RemoteShowControl](https://www.irisdown.co.uk/rsc.html) plugin,
  based on vMix input name information.
  <br/>
  Use Cases: Automatic PowerPoint Slide Control

- [remoteshowcontrol-once.vb](remoteshowcontrol-once.vb):<br/>
  [**Once Control Irisdown RemoteShowControl**](remoteshowcontrol-once.md)<br/>
  Once control a remote PowerPoint slide-deck with the help of its
  [Irisdown RemoteShowControl](https://www.irisdown.co.uk/rsc.html)
  plugin, based on vMix triggers or shortcuts.
  <br/>
  Use Cases: Manual PowerPoint Slide Control

- [ndi-studio-monitor.vb](ndi-studio-monitor.vb):<br/>
  [**Reconfigure NewTek NDI Studio Monitor**](ndi-studio-monitor.md)<br/>
  Allow vMix to reconfigure the NDI source displayed in a (remote) NewTek
  NDI Studio Monitor instance.
  <br/>
  Use Cases: Manual NDI Tools Studio Monitor Source Control

Installation
------------

1. Clone this [repository](https://github.com/rse/vmix-scripts)
   or [download a ZIP archive](https://github.com/rse/vmix-scripts/archive/refs/heads/master.zip):<br/>
   `git clone https://github.com/rse/vmix-scripts`<br/>

2. Add the individual scripts to vMix with<br/>
   **Settings** &rarr; **Scripting** &rarr; **Add** &rarr; **Import**

3. For some of the scripts, do not forget to adjust their configuration section!

Background
----------

All these vMix scripts where created in the professional context of a
company filmstudio, where multiple vMix instances (connected through
NDI) are used to drive the live event productions. The particularly used
vMix instances and their job (partially driven by the scripts and manual
Bitfocus Companion control informations) are:

- **Ingest 1**: 2160p30 mode, 5x physical camera ingest, 5x chroma-keying, 5x game-engine based
  background overlaying (indirectly using content 1+2 from **Ingest
  2**), 5x8 physical PTZ management, 5x8x7 virtual PTZ management, 5x7
  virtual PTZ emit (to **Mixing 1**).
  <br/>
  Rationale: 4K cameras have to be still chroma-keyed in 4K, flexible virtual PTZ management.
  <br/>
  Scripts: multiview-overlay, smooth-pan-zoom

- **Ingest 2**: 1080p30 mode, content 1+2 ingest (from **Mixing 1**),
  content 1+2 emit (via virtual camera into the game engines inside **Ingest 1**).
  <br/>
  Rationale: content from Mixing has to be ingested back into game-engine instances.
  <br/>
  Scripts: none

- **Mixing 1**: 1080p30 mode, 5x7 virtual PTZ ingest (from **Ingest 1**), 8x remote guests ingest, 1x slide ingest,
  12x microphone ingest, 4x4 title overlays, content 1+2 emit (to **Ingest 2**), primary programm emit (to **Playout 1**).
  <br/>
  Rationale: primary scene mixing (usually with german audio)
  <br/>
  Scripts: input-bridge, audio-sidechain, event-reconfiguration, event-title-control, recording-log,
  remoteshowcontrol-once, remoteshowcontrol-loop, ndi-studio-monitor, clone-input, auto-pre-mix

- **Mixing 2**: 1080p30 mode, primary program ingest (from **Mixing 1**), 2x real-time translator ingest,
  translator audio export/re-import, secondary programm emit (to **Playout 2**), studio multiview emit.
  <br/>
  Rationale: real-time translation (usually german to englisch audio), studio multiview production.
  <br/>
  Scripts: audio-sidechain

- **Playout 1**: 1080p30 mode, primary program ingest (from **Mixing 1**), event program management,
  primary program broadcasting.
  <br/>
  Rationale: event program mangement of first stream (usually german audio)
  <br/>
  Scripts: input-mirror, audio-heartbeat

- **Playout 2**: 1080p30 mode, secondary program ingest (from **Mixing 2**), event program management,
  secondary program broadcasting.
  <br/>
  Rationale: event program mangement of second stream (usually english audio)
  <br/>
  Scripts: none

License
-------

Copyright &copy; 2022-2023 Dr. Ralf S. Engelschall (http://engelschall.com/)

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

