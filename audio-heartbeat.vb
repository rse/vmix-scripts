'--
'-- audio-heartbeat.vb -- vMix VB.NET script for detecting unexpected silence
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--
'-- Language: VB.NET 2.0 (vMix 4K/Pro flavor)
'-- Version:  0.9.0 (2022-07-30)
'--

'-- ==== CONFIGURATION (please adjust) ====

dim heartbeatThresholdVolume    as integer = -32     'threshold below which volume to react (dB FS)
dim heartbeatThresholdTime      as integer = 5000    'threshold after which time   to react (ms)
dim heartbeatWarningEvery       as integer = 2000    'time between warning indicators (ms)
dim heartbeatTimeSlice          as integer = 10      'time interval between the script iterations (ms)
dim debug                       as boolean = true    'whether to output debug information to the console

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

'-- enter endless iteration loop
do while true
    '-- fetch current vMix API status
    dim xml as string = API.XML()
    cfg.LoadXml(xml)

    '-- determine whether we should operate at all (indicated by streaming/recording enabled)
    dim isStreaming    as boolean = (cfg.SelectSingleNode("/vmix/streaming").InnerText)
    dim isRecording    as boolean = (cfg.SelectSingleNode("/vmix/recording").InnerText)
	dim isMultiCording as boolean = (cfg.SelectSingleNode("/vmix/multiCorder").InnerText)
    if not (isStreaming or isRecording or isMultiCording) then
        continue do
    end if

    '-- determine input volume (in linear volume scale)
    dim meter1 as double = Double.Parse(cfg.SelectSingleNode("/vmix/audio/master/@meterF1").Value, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.CreateSpecificCulture("en-US"))
    dim meter2 as double = Double.Parse(cfg.SelectSingleNode("/vmix/audio/master/@meterF2").Value, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.CreateSpecificCulture("en-US"))
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

