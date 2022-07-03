
[Event-Title-Control](event-title-control.vb)
=============================================

**Control Layer-Embedded Titles**

Control the in/out transitioning of lower-third titles which are
embedded layers of scene inputs (where vMix only performs `TransitionIn`
and never a `TransitionOut`).

Problem
-------

Titles in vMix are usually controlled via the Overlay 1-4 facility. This
can be done manually by pressing the overlay buttons by the operator, or
automatically with three triggers on a scene input:

    Trigger       Function         Input   Delay
    TransitionIn  OverlayInput1In  <title> 2000
    TransitionIn  OverlayInput1Out         8000
    TransitionOut OverlayInput1Off

Alternatively, you can also embed the title input as a layer on the
scene input. But then vMix triggers only a `TransitionIn` on the title
and never a `TransitionOut`). Finally, when reusing scenes, the title of
a person should be raised only once within a certain time range.

Solution
--------

A [special reusable title](event-title-control.gtzip) is used for
embedding on layers of scene inputs. The title contains an invisible
`LastTransition` field for tracking time and makes all its elements
`Hidden` on `TransitionIn` and `TransitionOut`, and performs the in/out
transitioning on `Page1`/`Page2` instead.

Additionally, this script ensures that independent of arbitrary scene
input changes, the titles are shown just for `durationVisible` seconds
and at maximum every `durationLocked` seconds (see configuration section
at the top of the script).

Usage
-----

Place the title input(s) onto layers of the scene input and configure
triggers on the scene input:

    OnTransitionIn SetDynamicValue1 <title-input-name>,...
    OnTransitionIn ScriptStart      event-title-control

