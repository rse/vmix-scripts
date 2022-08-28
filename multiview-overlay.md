
[Multiview-Overlay](multiview-overlay.vb)
=========================================

**Update Custom Multiview Overlays**

Allow one to update the preview/program overlays of a custom multiview
by selecting corresponding images in a "List" type input.

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
1-M and on two additional layers place two "List" type inputs. Each
Nx2 "List" type inputs then should have 1+M images: one entirely empty image
(corresponding to no preview/program selected on the camera at all) and
M images corresponding to the view/angle input to be on preview/program.
Then use this script to track changes on preview/program and select the
corresponding image on the "List" inputs.

Usage
-----

Suppose you have 4 cameras and 7 views/angles and the view/angle inputs
are named `VPTZ - CAMX-Y` where X is 1-4 and Y is "C-L" (closeup-left),
"C-C" (closeup-center), "C-R" (closeup-right), "F-L" (figure-left),
"F-C" (figure-center), "F-R" (figure-right), and "W-C" (wide-center).
Suppose that the "List" inputs are named "MULTIVIEW-OV-PREVIEW - CAMX"
and MULTIVIEW-OV-PROGRAM - CAMX".

Then configure the `multiview-overlay` script with:

```
dim prefixCamAngle    as String   = "VPTZ - CAM"
dim prefixListPreview as String   = "MULTIVIEW-OV-PREVIEW - CAM"
dim prefixListProgram as String   = "MULTIVIEW-OV-PROGRAM - CAM"
dim angles            as String() = { "C-L", "C-C", "C-R", "F-L", "F-C", "F-R", "W-C" }
dim numberOfCams      as Integer  = 4
dim timeSlice         as Integer  = 50
dim debug             as Boolean  = true
```

