'--
'-- audio-sidechain.vb -- vMix VB.NET script for audio side-chain compression
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--

'-- DESCRIPTION:
'-- This is a VB.NET script for the vMix 4K/Pro scripting facility,
'-- allowing audio output volumes to be automatically and temporarily
'-- reduced, based on audio input volumes (when temporarily above a
'-- threshold) -- similar to an audio side-chain compression. There are
'-- two main use-cases for this functionality:
'--
'--     1. STAGE GATE:
'--        Allow stage input devices (microphones, attached to the
'--        Master bus plus the "marker" bus Bus-A, but individually
'--        controlled) to be temporarily "dimmed" (volume reduced) as
'--        long as stage output devices (callers and media, monitored
'--        on Bus-B) are active. This prevents nasty echos or even full
'--        loops on a stage. The recommended configuration is:
'--
'--            busMonitor      = "B"   (Notice: callers and media)
'--            busAdjust       = "A"   (Notice: microphones)
'--            busAdjustInputs = true  (Notice: adjust the inputs)
'--            volumeFull      = 0
'--            volumeReduced   = -54   (Notice: pull down very much)
'--            volumeThreshold = -32
'--            timeSlice       = 10
'--            timeAwaitOver   = 20
'--            timeAwaitBelow  = 200
'--            timeFadeDown    = 60
'--            timeFadeUp      = 200
'--
'--     2. TRANSLATOR OVER-SPEAKING:
'--        Allow one or more translators (usually sitting on vMix Call
'--        inputs and mixed on the Master audio bus and additionally
'--        monitored on Bus-C) to "over-speak" the program (usually
'--        received via NDI and mixed on the Master audio bus after
'--        being "dimmed" on Bus-B). The recommended configuration is:
'--
'--            busMonitor      = "C"   (Notice: translators)
'--            busAdjust       = "B"   (Notice: program)
'--            busAdjustInputs = false (Notice: adjust the bus)
'--            volumeFull      = 0
'--            volumeReduced   = -32   (Notice: pull down not too much)
'--            volumeThreshold = -32
'--            timeSlice       = 10
'--            timeAwaitOver   = 20
'--            timeAwaitBelow  = 2000  (Notice: allow translators to breathe and have gaps)
'--            timeFadeDown    = 60
'--            timeFadeUp      = 400   (Notice: fade in program slowly)

'-- ==== CONFIGURATION (please adjust) ====

'--  bus configuration
dim busMonitor            as string  = "B"       'id of audio bus to input/monitor volume
dim busAdjust             as string  = "A"       'id of audio bus to output/adjust volume
dim busAdjustInputs       as boolean = true      'whether inputs attached to bus are adjusted instead of just bus itself

'--  volume configuration
dim volumeFull            as integer = 0         'full      volume of output (dB)
dim volumeReduced         as integer = -54       'reduced   volume of output (dB)
dim volumeThreshold       as integer = -32       'threshold volume of input  (dB)

'--  time configuration
dim timeSlice             as integer = 10        'time interval between the script iterations          (ms)
dim timeAwaitOver         as integer = 20        'time over  the threshold before triggering fade down (ms)
dim timeAwaitBelow        as integer = 200       'time below the threshold before triggering fade up   (ms)
dim timeFadeDown          as integer = 60        'time for fading down (ms)
dim timeFadeUp            as integer = 200       'time for fading up   (ms)

'-- ==== INTERNAL STATE ====

'-- internal state
dim mode                  as string  = "wait"    'current iteration mode
dim volumeCurrent         as integer = -1        'current volume of output (from 0 to 100, linear scale)
dim timeAwaitBelowCount   as integer = 0         'counter for time over  the threshold
dim timeAwaitOverCount    as integer = 0         'counter for time below the threshold

'-- pre-convert volumes from decibel to audio fader level
dim volumeFullAmp         as double  = 10 ^ (volumeFullDB / 20)
dim volumeFull            as integer = cint((volumeFullAmp ^ 0.25) * 100)
dim volumeReducedAmp      as double  = 10 ^ (volumeReducedDB / 20)
dim volumeReduced         as integer = cint((volumeReducedAmp ^ 0.25) * 100)
dim volumeThresholdAmp    as double  = 10 ^ (volumeThresholdDB / 20)
dim volumeThreshold       as integer = cint((volumeThresholdAmp ^ 0.25) * 100)

'-- prepare XML DOM tree
dim cfg as new System.Xml.XmlDocument

'-- enter endless iteration loop
do while true
    '-- fetch current vMix API status
    dim xml as string = API.XML()
    cfg.LoadXml(xml)

    '-- determine whether we should operate at all (indicated by muted/unmuted input bus)
    dim muted as boolean = cfg.SelectSingleNode("//audio/bus" & busMonitor & "/@muted").Value
    if muted then
        '-- ensure we reset current volume knowledge once we become unmuted again
        if not volumeCurrent = -1 then
            volumeCurrent = -1
        end if
        continue do
    end if

    '-- initialize output volume
    if volumeCurrent = -1 then
        volumeCurrent = volumeFull
        if not busAdjustInputs then
            '-- adjust the audio bus directly
            dim isMuted as boolean = cfg.SelectSingleNode("//audio/bus" & busAdjust & "/@muted").Value
            if not isMuted then
                API.Function("SetBus" & busAdjust & "Volume", Value := volumeCurrent.ToString())
            end if
        else
            '-- adjust the inputs attached to the audio bus
            dim busInputs as XmlNodeList = cfg.SelectNodes("//inputs/input[@audiobusses]")
            for each busInput as XmlNode in busInputs
                dim onBusses() as string = busInput.Attributes("audiobusses").InnerText.Split(",")
                if Array.IndexOf(onBusses, busAdjust) >= 0 then
                    dim isMuted as boolean = Convert.ToBoolean(busInput.Attributes("muted").InnerText)
                    if not isMuted then
                        dim num as integer = Convert.ToInt32(busInput.Attributes("number").InnerText)
                        Input.Find(num).Function("SetVolumeFade",
                            volumeCurrent.ToString() & "," & (cint(timeSlice * 0.90)).ToString())
                    end if
                end if
            next busInput
        end if
    end if

    '-- determine input volume (in linear volume scale)
    dim meter1 as double = cfg.SelectSingleNode("//audio/bus" & busMonitor & "/@meterF1").Value
    dim meter2 as double = cfg.SelectSingleNode("//audio/bus" & busMonitor & "/@meterF2").Value
    if meter1 < meter2 then
        meter1 = meter2
    end if

    '-- track whether input volume is continuously over or below volume threshold
    if meter1 > volumeThresholdAmp then
        timeAwaitOverCount  += 1
        timeAwaitBelowCount  = 0
    else
        timeAwaitBelowCount += 1
        timeAwaitOverCount   = 0
    end if

    '-- decide current operation mode
    if timeAwaitBelowCount >= cint(timeAwaitBelow / timeSlice) and volumeCurrent < volumeFull then
        mode = "fade-up"
    elseif timeAwaitOverCount >= cint(timeAwaitOver / timeSlice) and volumeCurrent > volumeReduced then
        mode = "fade-down"
    else
        mode = "wait"
    end if

    '-- fade output volume down/up
    if mode = "fade-down" or mode = "fade-up" then
        dim k as integer = cint(((volumeFull - volumeReduced) / timeFadeDown) * timeSlice)
        if k = 0 then
            k = 1
        end if
        if mode = "fade-down" then
            volumeCurrent -= k
        elseif mode = "fade-up" then
            volumeCurrent += k
        end if
        if not busAdjustInputs then
            '-- adjust the audio bus directly
            dim isMuted as boolean = cfg.SelectSingleNode("//audio/bus" & busAdjust & "/@muted").Value
            if not isMuted then
                API.Function("SetBus" & busAdjust & "Volume", Value := volumeCurrent.ToString())
            end if
        else
            '-- adjust the inputs attached to the audio bus
            dim busInputs as XmlNodeList = cfg.SelectNodes("//inputs/input[@audiobusses]")
            for each busInput as XmlNode in busInputs
                dim onBusses() as string = busInput.Attributes("audiobusses").InnerText.Split(",")
                if Array.IndexOf(onBusses, busAdjust) >= 0 then
                    dim isMuted as boolean = Convert.ToBoolean(busInput.Attributes("muted").InnerText)
                    if not isMuted then
                        dim num as integer = Convert.ToInt32(busInput.Attributes("number").InnerText)
                        Input.Find(num).Function("SetVolumeFade",
                            volumeCurrent.ToString() & "," & (cint(timeSlice * 0.90)).ToString())
                    end if
                end if
            next busInput
        end if
    end if

    '-- wait until next iteration
    sleep(timeSlice)
loop

