'--
'-- audio-sidechain.vb -- vMix VB.NET script for audio side-chain compression
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--
'-- Language: VB.NET 2.0 (vMix 4K/Pro flavor)
'-- Version:  1.2.0 (2022-07-19)
'--

'-- ==== CONFIGURATION (please adjust) ====

'--  bus configuration
dim busMonitor            as string  = "B"       'id of audio bus to input/monitor volume
dim busAdjust             as string  = "A"       'id of audio bus to output/adjust volume
dim busAdjustInputs       as boolean = true      'whether inputs attached to bus are adjusted instead of just bus itself
dim busAdjustInputsExcl   as string  = ""        'comma-separated list of inputs to exclude from adjustment
dim busAdjustUnmutedOnly  as boolean = false     'whether only unmuted bus/inputs should be adjusted

'--  volume configuration
dim volumeFullDB          as integer = 0         'full      volume of output (dB)
dim volumeReducedDB       as integer = -55       'reduced   volume of output (dB)
dim volumeThresholdDB     as integer = -32       'threshold volume of input  (dB)

'--  time configuration
dim timeSlice             as integer = 10        'time interval between the script iterations          (ms)
dim timeAwaitOver         as integer = 20        'time over  the threshold before triggering fade down (ms)
dim timeAwaitBelow        as integer = 150       'time below the threshold before triggering fade up   (ms)
dim timeFadeDown          as integer = 50        'time for fading down (ms)
dim timeFadeUp            as integer = 50        'time for fading up   (ms)

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

'-- prepare list of excluded inputs
dim busAdjustInputsExclA() as string = busAdjustInputsExcl.Split(",")

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
            if not isMuted and not busAdjustUnmutedOnly then
                API.Function("SetBus" & busAdjust & "Volume", Value := cint(volumeCurrent).ToString())
            end if
        else
            '-- adjust the inputs attached to the audio bus
            dim busInputs as XmlNodeList = cfg.SelectNodes("//inputs/input[@audiobusses]")
            for each busInput as XmlNode in busInputs
                dim onBusses() as string = busInput.Attributes("audiobusses").InnerText.Split(",")
                dim title      as string = busInput.Attributes("title").InnerText
                if Array.IndexOf(onBusses, busAdjust) >= 0 and Array.IndexOf(busAdjustInputsExclA, title) < 0 then
                    dim isMuted as boolean = Convert.ToBoolean(busInput.Attributes("muted").InnerText)
                    if not isMuted and not busAdjustUnmutedOnly then
                        dim num as integer = Convert.ToInt32(busInput.Attributes("number").InnerText)
                        Input.Find(num).Function("SetVolume", Value := cint(volumeCurrent).ToString())
                    end if
                end if
            next
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
            Console.WriteLine("audio-sidechain: INFO: switching to mode: " & modeNew)
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
            if not isMuted and not busAdjustUnmutedOnly then
                API.Function("SetBus" & busAdjust & "Volume", Value := cint(volumeCurrent).ToString())
            end if
        else
            '-- adjust the inputs attached to the audio bus
            dim busInputs as XmlNodeList = cfg.SelectNodes("//inputs/input[@audiobusses]")
            for each busInput as XmlNode in busInputs
                dim onBusses() as string = busInput.Attributes("audiobusses").InnerText.Split(",")
                dim title      as string = busInput.Attributes("title").InnerText
                if Array.IndexOf(onBusses, busAdjust) >= 0 and Array.IndexOf(busAdjustInputsExclA, title) < 0 then
                    dim isMuted as boolean = Convert.ToBoolean(busInput.Attributes("muted").InnerText)
                    if not isMuted and not busAdjustUnmutedOnly then
                        dim num as integer = Convert.ToInt32(busInput.Attributes("number").InnerText)
                        Input.Find(num).Function("SetVolume", Value := cint(volumeCurrent).ToString())
                    end if
                end if
            next
        end if
    end if

    '-- wait until next iteration
    sleep(timeSlice)
loop

