
[Multiview-Overlay](multiview-overlay.vb)
=========================================

**Update Custom Multiview Overlays**

Allow one to select the corresponding page of a multi-camera multiview
and update its preview/program overlays.

Problem
-------

The vMix Multiview facility is rather limited, as it neither allows
multiple multiviews nor fully custom multiview layouts. For a usual
greenscreen-based setup with N physical cameras and M virtual
views/angles, one usually want N multiviews and each one should show M
views/angles.

Solution
--------

As as long the number of views/angles M is lower than 8, create N
multiview-like inputs, each with M views/angles on layers the layers
1-M and on two additional layers place two `Title` type inputs. Each
Nx2 `Title` type inputs then should have M transitions "PageX",
corresponding to the view/angle input to be on preview/program. Then
use this script to track changes on preview/program and select the
corresponding transition "Pagex" on the `Title` inputs.

Usage
-----

Suppose we have the following setup:

- 4 cameras and 7 views/angles and the view/angle inputs
  are named `VPTZ - CAMX-Y` where X is 1-4 and Y is `C-L` (closeup-left),
  `C-C` (closeup-center), `C-R` (closeup-right), `F-L` (figure-left),
  `F-C` (figure-center), `F-R` (figure-right), and `W-C` (wide-center).

- `Title` inputs named `MULTIVIEW-OV-PREVIEW - CAMX`
  and `MULTIVIEW-OV-PROGRAM - CAMX` per camera, each holding
  M overlays for preview/program.

- 4 custom multiview inputs named `MULTIVIEW - CAMX` corresponding
  to the 4 cameras and each showing the 7 views/angles.

- 1 `Mix` type input #3 (1-3 for the custom mixers, 0 is the
  main mixer) which is assigned for the fullscreen output under
  `Settings`/`Outputs ...`/`Fullscreen` which receives the
  4 custom multiview inputs.

Then configure the `multiview-overlay` script with:

`` `
dim numberOfCams            as Integer  = 4
dim angleInputPrefix        as String   = "VPTZ - CAM"
dim angleInputPostfixes     as String() = { "C-L", "C-C", "C-R", "F-L", "F-C", "F-R", "W-C" }
dim titlePreviewInputPrefix as String   = "MULTIVIEW-OV-PREVIEW - CAM"
dim titleProgramInputPrefix as String   = "MULTIVIEW-OV-PROGRAM - CAM"
dim multiviewInputPrefix    as String   = "MULTIVIEW - CAM"
dim multiviewOutputId       as String   = "3"
dim timeSlice               as Integer  = 50
dim debug                   as Boolean  = true
```

