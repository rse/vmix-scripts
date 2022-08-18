'--
'-- audio-sidechain.vb -- vMix VB.NET script for audio side-chain compression
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--
'-- Language: VB.NET 2.0 (vMix 4K/Pro flavor)
'-- Version:  1.3.0 (2022-07-24)
'--

'-- ==== CONFIGURATION (please adjust) ====

'--  bus configuration
dim busMonitor            as string  = "B"       'id of audio bus to input/monitor volume
dim busAdjust             as string  = "A"       'id of audio bus to output/adjust volume
dim busAdjustInputs       as boolean = true      'whether inputs attached to bus are adjusted instead of just bus itself
dim busAdjustInputsExcl   as string  = ""        'comma-separated list of inputs to exclude from adjustment
dim busAdjustUnmutedOnly  as boolean = false     'whether only unmuted bus/inputs should be adjusted

'--  volume configuration
dim volumeThreshold       as integer = -42       'threshold of input  in dB FS   (-oo to   0)
dim volumeFull            as integer = 100       'full         output in percent (0   to 100)
dim volumeReduced         as integer = 20        'reduced      output in percent (0   to 100)

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
dim volumeCurrent         as double  = -1        'current volume of output in percent (from 0 to 100)
dim timeAwaitBelowCount   as integer = 0         'counter for time over  the threshold
dim timeAwaitOverCount    as integer = 0         'counter for time below the threshold

'-- pre-convert values
dim volumeThresholdAmp    as double  = 10 ^ (volumeThreshold / 20)

'-- prepare XML DOM tree
dim cfg as new System.Xml.XmlDocument

'-- prepare list of excluded inputs
dim busAdjustInputsExclA() as string = busAdjustInputsExcl.Split(",")

'-- use a fixed locale for parsing floating point numbers
dim cultureInfo as System.Globalization.CultureInfo = System.Globalization.CultureInfo.CreateSpecificCulture("en-US")

'-- enter endless iteration loop
do while true
    '-- fetch current vMix API status
    dim xml as string = API.XML()
    cfg.LoadXml(xml)

    '-- determine whether we should operate at all (indicated by muted/unmuted input bus)
    dim muted as boolean = Convert.ToBoolean(cfg.SelectSingleNode("/vmix/audio/bus" & busMonitor & "/@muted").Value)
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
            dim isMuted as boolean = Convert.ToBoolean(cfg.SelectSingleNode("/vmix/audio/bus" & busAdjust & "/@muted").Value)
            if not isMuted or not busAdjustUnmutedOnly then
                API.Function("SetBus" & busAdjust & "Volume", Value := cint(volumeCurrent).ToString())
            end if
        else
            '-- adjust the inputs attached to the audio bus
            dim busInputs as XmlNodeList = cfg.SelectNodes("/vmix/inputs/input[@audiobusses]")
            for each busInput as XmlNode in busInputs
                dim onBusses() as string = busInput.Attributes("audiobusses").Value.Split(",")
                dim title      as string = busInput.Attributes("title").Value
                if Array.IndexOf(onBusses, busAdjust) >= 0 and Array.IndexOf(busAdjustInputsExclA, title) < 0 then
                    dim isMuted as boolean = Convert.ToBoolean(busInput.Attributes("muted").Value)
                    if not isMuted or not busAdjustUnmutedOnly then
                        dim num as integer = Convert.ToInt32(busInput.Attributes("number").Value)
                        Input.Find(num).Function("SetVolume", Value := cint(volumeCurrent).ToString())
                    end if
                end if
            next
        end if
    end if

    '-- determine input volume (in linear volume scale)
    dim meter1 as double = Convert.ToDouble(cfg.SelectSingleNode("/vmix/audio/bus" & busMonitor & "/@meterF1").Value, cultureInfo)
    dim meter2 as double = Convert.ToDouble(cfg.SelectSingleNode("/vmix/audio/bus" & busMonitor & "/@meterF2").Value, cultureInfo)
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
            dim isMuted as boolean = Convert.ToBoolean(cfg.SelectSingleNode("/vmix/audio/bus" & busAdjust & "/@muted").Value)
            if not isMuted or not busAdjustUnmutedOnly then
                API.Function("SetBus" & busAdjust & "Volume", Value := cint(volumeCurrent).ToString())
            end if
        else
            '-- adjust the inputs attached to the audio bus
            dim busInputs as XmlNodeList = cfg.SelectNodes("/vmix/inputs/input[@audiobusses]")
            for each busInput as XmlNode in busInputs
                dim onBusses() as string = busInput.Attributes("audiobusses").Value.Split(",")
                dim title      as string = busInput.Attributes("title").Value
                if Array.IndexOf(onBusses, busAdjust) >= 0 and Array.IndexOf(busAdjustInputsExclA, title) < 0 then
                    dim isMuted as boolean = Convert.ToBoolean(busInput.Attributes("muted").Value)
                    if not isMuted or not busAdjustUnmutedOnly then
                        dim num as integer = Convert.ToInt32(busInput.Attributes("number").Value)
                        Input.Find(num).Function("SetVolume", Value := cint(volumeCurrent).ToString())
                    end if
                end if
            next
        end if
    end if

    '-- wait until next iteration
    sleep(timeSlice)
loop

