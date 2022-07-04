'--
'-- recording-log.vb -- vMix script for logging recording states
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--
'-- Language: VB.NET 2.0 (vMix 4K/Pro flavor)
'-- Version:  0.9.0 (2022-07-04)
'--

'-- CONFIGURATION
dim markerDynamicVariable as String = "3"
dim timeSlice             as Integer = 33  ' = 1000ms / 30fps

'-- prepare XML DOM tree and load the current API state
dim cfg as new System.Xml.XmlDocument
dim xml as String = API.XML()
cfg.LoadXml(xml)

'-- determine logfile location
dim presetNode as System.Xml.XmlNode = cfg.SelectSingleNode("/vmix/preset")
if presetNode is Nothing then
    Console.WriteLine("recording-log: ERROR: You are running on a still UNSAVED vMix preset!")
    Console.WriteLine("recording-log: ERROR: Save your preset at least once, please.")
    return
end if
dim logFile as String = presetNode.InnerText.Replace(".vmix", ".log")

'-- keep internal state
dim recordingSince    as DateTime = nothing
dim multicordingSince as DateTime = nothing

'-- endless loop
do while true
    '-- re-load the current API state
    xml = API.XML()
    cfg.LoadXml(xml)

    '-- start fresh log
    dim log as new System.Collections.Generic.List(of String)

    '-- bookkeep recording state changes
    dim isRecording as Boolean = Boolean.parse(cfg.SelectSingleNode("/vmix/recording").InnerText)
    if recordingSince = nothing and isRecording then
        log.Add("RECORDING   started")
        recordingSince = DateTime.Now
    elseif recordingSince <> nothing and not isRecording then
        dim diff as System.TimeSpan = DateTime.Now.Subtract(recordingSince)
        dim duration as DateTime = (new DateTime(0)).Add(diff)
        log.Add("RECORDING   ended   (duration: " & duration.ToString("HH:mm:ss.fff") & ")")
        recordingSince = nothing
    end if

    '-- bookkeep multicorder state changes
    dim isMulticording as Boolean = Boolean.parse(cfg.SelectSingleNode("/vmix/multiCorder").InnerText)
    if multicordingSince = nothing and isMulticording then
        log.Add("MULTICORDER started")
        multicordingSince = DateTime.Now
    elseif multicordingSince <> nothing and not isMulticording then
        dim diff as System.TimeSpan = DateTime.Now.Subtract(multicordingSince)
        dim duration as DateTime = (new DateTime(0)).Add(diff)
        log.Add("MULTICORDER ended   (duration: " & duration.ToString("HH:mm:ss.fff") & ")")
        multicordingSince = nothing
    end if

    '-- bookkeep marker trigger
    dim nowVariableState as String = cfg.selectSingleNode("/vmix/dynamic/value" & markerDynamicVariable).InnerText
    if nowVariableState = "trigger-marker" then
        API.Function("SetDynamicValue" & markerDynamicVariable, Value := "")
        if recordingSince <> nothing and isRecording then
            dim diff as System.TimeSpan = DateTime.Now.Subtract(recordingSince)
            dim duration as DateTime = (new DateTime(0)).Add(diff)
            log.Add("RECORDING   marked  (position: " & duration.ToString("HH:mm:ss.fff") & ")")
        end if
        if multicordingSince <> nothing and isMulticording then
            dim diff as System.TimeSpan = DateTime.Now.Subtract(multicordingSince)
            dim duration as DateTime = (new DateTime(0)).Add(diff)
            log.Add("MULTICORDER marked  (position: " & duration.ToString("HH:mm:ss.fff") & ")")
        end if
    end if

    '-- write log entries
    if log.Count > 0 then
        dim timestamp as String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
        dim utf8WithoutBOM as System.Text.Encoding = new System.Text.UTF8Encoding(false)
        for each entry as String in log
            dim msg as String = "[" & timestamp & "] " & entry & Environment.NewLine
            System.IO.File.AppendAllText(logFile, msg, utf8WithoutBOM)
        next
    end if

    '-- wait a little bit before next iteration
    sleep(timeSlice)
loop

