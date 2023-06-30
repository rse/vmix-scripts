'--
'-- audio-heartbeat.vb -- vMix VB.NET script for detecting unexpected silence
'-- Copyright (c) 2022-2023 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--
'-- Language: VB.NET 2.0 (vMix 4K/Pro flavor)
'-- Version:  0.9.2 (2023-06-30)
'--

'-- ==== CONFIGURATION (please adjust) ====

dim heartbeatMonitorBus      as string  = "master"         'id of audio bus to monitor volume
dim heartbeatMonitorInput    as string  = ""               'id of input     to monitor volume
dim heartbeatThresholdVolume as integer = -32              'threshold below which volume to react (dB FS)
dim heartbeatThresholdTime   as integer = 5000             'threshold after which time   to react (ms)
dim heartbeatWarningEvery    as integer = 2000             'time between warning indicators (ms)
dim heartbeatTimeSlice       as integer = 10               'time interval between the script iterations (ms)
dim inputOK                  as string  = "STREAM1"        'optional input to switch to for OK situation
dim inputWARNING             as string  = "SCREEN-FAILURE" 'optional input to switch to for warning situaton
dim debug                    as boolean = false            'whether to output debug information to the console

'-- ==== INTERNAL STATE ====

'-- internal state
dim mode                  as string  = "OK"      'current iteration mode
dim timeAwaitBelowCount   as integer = 0         'counter for time over  the threshold
dim timeAwaitOverCount    as integer = 0         'counter for time below the threshold
dim timeWarningCount      as integer = 0         'counter for time of warning

'-- pre-convert decibel to amplitude
dim heartbeatThresholdAmp as double  = 10 ^ (heartbeatThresholdVolume / 20)

'-- prepare XML DOM tree
dim cfg as new System.Xml.XmlDocument

'-- use a fixed locale for parsing floating point numbers
dim cultureInfo as System.Globalization.CultureInfo = System.Globalization.CultureInfo.CreateSpecificCulture("en-US")

'-- enter endless iteration loop
do while true
    '-- fetch current vMix API status
    dim xml as string = API.XML()
    cfg.LoadXml(xml)

    '-- determine whether we should operate at all (indicated by streaming/recording enabled)
    dim isStreaming    as boolean = Convert.ToBoolean(cfg.SelectSingleNode("/vmix/streaming").InnerText)
    dim isRecording    as boolean = Convert.ToBoolean(cfg.SelectSingleNode("/vmix/recording").InnerText)
	dim isMultiCording as boolean = Convert.ToBoolean(cfg.SelectSingleNode("/vmix/multiCorder").InnerText)
    if not (isStreaming or isRecording or isMultiCording) then
        continue do
    end if

    '-- determine input volume (in linear volume scale)
    dim meter1 as double = 0.0
    dim meter2 as double = 0.0
    if heartbeatMonitorBus <> "" and heartbeatMonitorInput = "" then
        meter1 = Convert.ToDouble(cfg.SelectSingleNode("/vmix/audio/bus" & heartbeatMonitorBus & "/@meterF1").Value, cultureInfo)
        meter2 = Convert.ToDouble(cfg.SelectSingleNode("/vmix/audio/bus" & heartbeatMonitorBus & "/@meterF2").Value, cultureInfo)
    elseif heartbeatMonitorBus = "" and heartbeatMonitorInput <> "" then
        meter1 = Convert.ToDouble(cfg.SelectSingleNode("/vmix/inputs/input[@title = '" & heartbeatMonitorInput & "']/@meterF1").Value, cultureInfo)
        meter2 = Convert.ToDouble(cfg.SelectSingleNode("/vmix/inputs/input[@title = '" & heartbeatMonitorInput & "']/@meterF2").Value, cultureInfo)
    end if
    if meter1 < meter2 then
        meter1 = meter2
    end if

    '-- track whether input volume is continuously over or below volume threshold
    if meter1 > heartbeatThresholdAmp then
        timeAwaitOverCount  += 1
        timeAwaitBelowCount  = 0
    else
        timeAwaitBelowCount += 1
        timeAwaitOverCount   = 0
    end if

    '-- decide current operation mode
    dim modeNew as String = "OK"
    if timeAwaitBelowCount >= cint(heartbeatThresholdTime / heartbeatTimeSlice) then
        modeNew = "WARNING"
    end if
	if modeNew = "WARNING" and mode <> "WARNING" then
	    '-- enforce an initial warning immediately
	    timeWarningCount = cint(heartbeatWarningEvery / heartBeatTimeSlice)
	end if
    if mode <> modeNew then
        if debug then
            Console.WriteLine("audio-heartbeat: INFO: switching to mode: " & modeNew)
        end if
        if mode = "OK" and modeNew = "WARNING" then
            '-- enter WARNING mode: optionally switch to WARNING input
            if inputOK <> "" and inputWARNING <> "" then
                API.Function("CutDirect", Input := inputWARNING)
            end if
        elseif mode = "WARNING" and modeNew = "OK" then
            '-- leave WARNING mode: optionally switch to regular input
            if inputOK <> "" and inputWARNING <> "" then
                API.Function("CutDirect", Input := inputOK)
            end if
        end if
        mode = modeNew
    end if

    '-- warn operator
    if mode = "WARNING" then
        timeWarningCount += 1
		if timeWarningCount >= cint(heartbeatWarningEvery / heartBeatTimeSlice) then
		    timeWarningCount = 0
			Console.WriteLine("audio-heartbeat: DEBUG: notifying operator about warning situation")
            My.Computer.Audio.PlaySystemSound(Media.SystemSounds.Exclamation)
        end if
    end if

    '-- wait until next iteration
    sleep(heartbeatTimeSlice)
loop

