'--
'-- translator-overspeak.vb -- vMix VB.NET script for translator over-speaking
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--

'-- DESCRIPTION:
'-- This is a VB.NET script for the vMix 4K/Pro scripting facility,
'-- allowing translators (sitting on vMix Call sources and mixed on the
'-- Master audio bus and monitored on Bus-C) to over-speak the program
'-- (received via NDI and mixed on the Master audio bus after being
'-- "dimming" on Bus-B)

'--  bus configuration (can be adjusted)
dim busProgram            as string  = "B"
dim busTranslators        as string  = "C"

'--  volume configuration (can be adjusted)
dim volumeFull            as integer = 100       'full      volume of output (from 0 to 100, linear scale)
dim volumeReduced         as integer = 40        'reduced   volume of output (from 0 to 100, linear scale)
dim volumeThreshold       as integer = 50        'threshold volume of input  (from 0 to 100, linear scale)

'--  time configuration (can be adjusted)
dim timeSlice             as integer = 10        'time interval between the script iterations          (ms)
dim timeAwaitOver         as integer = 20        'time over  the threshold before triggering fade down (ms)
dim timeAwaitBelow        as integer = 2000      'time below the threshold before triggering fade up   (ms)
dim timeFadeDown          as integer = 60        'time for fading down (ms)
dim timeFadeUp            as integer = 400       'time for fading up   (ms)

'-- internal state
dim mode                  as string  = "wait"    'current iteration mode
dim volumeCurrent         as integer = -1        'current volume of output (from 0 to 100, linear scale)
dim timeAwaitBelowCount   as integer = 0         'counter for time over  the threshold
dim timeAwaitOverCount    as integer = 0         'counter for time below the threshold

'-- enter endless iteration loop
do while true
    '-- fetch current vMix API status
    dim xml as string = API.XML()
    dim x as new system.xml.xmldocument
    x.loadxml(xml)

    '-- determine whether we should operate at all (indicated by muted/unmuted input bus)
    dim muted as boolean = (x.SelectSingleNode("//audio/bus" + busTranslators + "/@muted").Value)
    if muted
        '-- ensure we reset current volume knowledge once we become unmuted again
        if not volumeCurrent = -1
            volumeCurrent = -1
        end if
        continue do
    end if

    '-- initialize output volume
    if volumeCurrent = -1
        volumeCurrent = volumeFull
        API.Function("SetBus" + busProgram + "Volume", Value := volumeCurrent.tostring)
    end if

    '-- determine input volume (in linear volume scale)
    dim meter1 as double = (x.SelectSingleNode("//audio/bus" + busTranslators + "/@meterF1").Value)
    dim meter2 as double = (x.SelectSingleNode("//audio/bus" + busTranslators + "/@meterF2").Value)
    if meter1 < meter2
        meter1 = meter2
    end if
    meter1 = cint((meter1 ^ 0.25) * 100)

    '-- track whether input volume is continuously over or below volume threshold
    if meter1 > volumeThreshold
        timeAwaitOverCount  += 1
        timeAwaitBelowCount  = 0
    else
        timeAwaitBelowCount += 1
        timeAwaitOverCount   = 0
    end if

    '-- decide current operation mode
    if timeAwaitBelowCount >= cint(timeAwaitBelow / timeSlice) and volumeCurrent < volumeFull
        mode = "fade-up"
    elseif timeAwaitOverCount >= cint(timeAwaitOver / timeSlice) and volumeCurrent > volumeReduced
        mode = "fade-down"
    else
        mode = "wait"
    end if

    '-- fade output volume down/up
    if mode = "fade-down" or mode = "fade-up"
        dim k as integer = cint(((volumeFull - volumeReduced) / timeFadeDown) * timeSlice)
        if k = 0
            k = 1
        end if
        if mode = "fade-down"
            volumeCurrent -= k
        elseif mode = "fade-up"
            volumeCurrent += k
        end if
        API.Function("SetBus" + busProgram + "Volume", Value := volumeCurrent.tostring)
    end if

    '-- wait until next iteration
    sleep(timeSlice)
loop

