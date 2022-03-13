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
'--            busMonitor        = "B"   (Notice: callers and media)
'--            busAdjust         = "A"   (Notice: microphones)
'--            busAdjustInputs   = true  (Notice: adjust the inputs)
'--            volumeFullDB      = 0
'--            volumeReducedDB   = -55   (Notice: pull down volume to about 20%)
'--            volumeThresholdDB = -32
'--            timeSlice         = 10
'--            timeAwaitOver     = 20
'--            timeAwaitBelow    = 200
'--            timeFadeDown      = 50
'--            timeFadeUp        = 150
'--
'--     2. TRANSLATOR OVER-SPEAKING:
'--        Allow one or more translators (usually sitting on vMix Call
'--        inputs and mixed on the Master audio bus and additionally
'--        monitored on Bus-C) to "over-speak" the program (usually
'--        received via NDI and mixed on the Master audio bus after
'--        being "dimmed" on Bus-B). The recommended configuration is:
'--
'--            busMonitor        = "C"   (Notice: translators)
'--            busAdjust         = "B"   (Notice: program)
'--            busAdjustInputs   = false (Notice: adjust the bus)
'--            volumeFullDB      = 0
'--            volumeReducedDB   = -24   (Notice: pull down volume to about 50%)
'--            volumeThresholdDB = -32
'--            timeSlice         = 10
'--            timeAwaitOver     = 20
'--            timeAwaitBelow    = 1500  (Notice: allow translators to breathe)
'--            timeFadeDown      = 50
'--            timeFadeUp        = 500   (Notice: fade in program slowly)
'--
'-- BACKROUND:
'-- The audio volume science is a little bit hard to understand and vMix
'-- in addition also makes it even more complicated by using difference
'-- scales. Here is some background on the above Decibel (dB) based
'-- scales, the formulars how the scales can be converted and some
'-- examples:
'--
'--     Scales:
'--         Volume:      0 to 100    (used for UI volume bars, SetVolumeFade)
'--         Amplitude:   0 to 1      (used for API audio bus meter, @meterF1)
'--         Amplitude2:  0 to 100    (used for API input volume input, @volume)
'--         Decibels:    -oo to 0    (used in audio science)
'--
'--     Formulas:
'--         Amplitude    = Amplitude2 / 100
'--         Amplitude2   = Amplitude * 100
'--         Volume       = (Amplitude ^ 0.25) * 100
'--         Amplitude    = (Volume / 100) ^ 4
'--         Decibels     = 20 * Math.Log10(Amplitude)
'--         Amplitude    = 10 ^ (Decibels / 20)
'--
'--     Examples:
'--         Volume: 100, Amplitude: 1.0000, Decibel:  0
'--         Volume:  97, Amplitude: 0.8913, Decibel: -1  (clipping border)
'--         Volume:  92, Amplitude: 0.7079, Decibel: -3
'--         Volume:  84, Amplitude: 0.5012, Decibel: -6
'--         Volume:  77, Amplitude: 0.3548, Decibel: -9
'--         Volume:  71, Amplitude: 0.2512, Decibel: -12
'--         Volume:  63, Amplitude: 0.1585, Decibel: -16 (broadcast standard)
'--         Volume:  50, Amplitude: 0.0631, Decibel: -23 (film standard)
'--         Volume:  40, Amplitude: 0.0224, Decibel: -32 (voice gate border)
'--         Volume:  27, Amplitude: 0.0056, Decibel: -45
'--         Volume:  21, Amplitude: 0.0020, Decibel: -54 (very much dimmed)
'--         Volume:  15, Amplitude: 0.0006, Decibel: -65 (silence border)
'--         Volume:  10, Amplitude: 0.0001, Decibel: -80
'--         Volume:   0, Amplitude: 0.0000, Decibel: -Infinity

'-- ==== CONFIGURATION (please adjust) ====

'--  bus configuration
dim busMonitor            as string  = "B"       'id of audio bus to input/monitor volume
dim busAdjust             as string  = "A"       'id of audio bus to output/adjust volume
dim busAdjustInputs       as boolean = true      'whether inputs attached to bus are adjusted instead of just bus itself

'--  volume configuration
dim volumeFullDB          as integer = 0         'full      volume of output (dB)
dim volumeReducedDB       as integer = -55       'reduced   volume of output (dB)
dim volumeThresholdDB     as integer = -32       'threshold volume of input  (dB)

'--  time configuration
dim timeSlice             as integer = 10        'time interval between the script iterations          (ms)
dim timeAwaitOver         as integer = 20        'time over  the threshold before triggering fade down (ms)
dim timeAwaitBelow        as integer = 200       'time below the threshold before triggering fade up   (ms)
dim timeFadeDown          as integer = 50        'time for fading down (ms)
dim timeFadeUp            as integer = 150       'time for fading up   (ms)

'--  debug configuration
dim debug                 as boolean = false     'whether to output debug information to the console

'-- ==== INTERNAL STATE ====

'-- internal state
dim mode                  as string  = "wait"    'current iteration mode
dim volumeCurrent         as double  = -1        'current volume of output (from 0 to 100, linear scale)
dim timeAwaitBelowCount   as integer = 0         'counter for time over  the threshold
dim timeAwaitOverCount    as integer = 0         'counter for time below the threshold

'-- pre-convert volumes from decibel to audio fader level
dim volumeFullAmp         as double  = 10 ^ (volumeFullDB / 20)
dim volumeFull            as double  = (volumeFullAmp ^ 0.25) * 100
dim volumeReducedAmp      as double  = 10 ^ (volumeReducedDB / 20)
dim volumeReduced         as double  = (volumeReducedAmp ^ 0.25) * 100
dim volumeThresholdAmp    as double  = 10 ^ (volumeThresholdDB / 20)
dim volumeThreshold       as double  = (volumeThresholdAmp ^ 0.25) * 100

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
        if volumeCurrent >= 0 then
            volumeCurrent = -1
        end if
        continue do
    end if

    '-- initialize output volume
    if volumeCurrent < 0 then
        volumeCurrent = volumeFull
        if not busAdjustInputs then
            '-- adjust the audio bus directly
            dim isMuted as boolean = cfg.SelectSingleNode("//audio/bus" & busAdjust & "/@muted").Value
            if not isMuted then
                API.Function("SetBus" & busAdjust & "Volume", Value := cint(volumeCurrent).ToString())
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
                        Input.Find(num).Function("SetVolume", Value := cint(volumeCurrent).ToString())
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
    dim modeNew as String = ""
    if timeAwaitBelowCount >= cint(timeAwaitBelow / timeSlice) and volumeCurrent < volumeFull then
        modeNew = "fade-up"
    elseif timeAwaitOverCount >= cint(timeAwaitOver / timeSlice) and volumeCurrent > volumeReduced then
        modeNew = "fade-down"
    else
        modeNew = "wait"
    end if
    if mode <> modeNew then
        if debug then
            console.writeline("switching to mode: " & modeNew)
        end if
        mode = modeNew
    end if

    '-- fade output volume down/up
    if mode = "fade-down" or mode = "fade-up" then
        if mode = "fade-down" then
            volumeCurrent -= ((volumeFull - volumeReduced) / timeFadeDown) * timeSlice
        elseif mode = "fade-up" then
            volumeCurrent += ((volumeFull - volumeReduced) / timeFadeUp  ) * timeSlice
        end if
        if not busAdjustInputs then
            '-- adjust the audio bus directly
            dim isMuted as boolean = cfg.SelectSingleNode("//audio/bus" & busAdjust & "/@muted").Value
            if not isMuted then
                API.Function("SetBus" & busAdjust & "Volume", Value := cint(volumeCurrent).ToString())
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
                        Input.Find(num).Function("SetVolume", Value := cint(volumeCurrent).ToString())
                    end if
                end if
            next busInput
        end if
    end if

    '-- wait until next iteration
    sleep(timeSlice)
loop

