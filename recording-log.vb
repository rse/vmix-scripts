'--
'-- recording-log.vb -- vMix script for logging recording states
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--
'-- Language: VB.NET 2.0 (vMix 4K/Pro flavor)
'-- Version:  1.0.1 (2022-07-05)
'--

'-- CONFIGURATION
dim markerDynamicVariable as String  = "3"
dim markerStandardText    as String  = "slip of the tongue"
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

    '-- log recording state changes
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

    '-- log multicorder state changes
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

    '-- log marker trigger
    dim nowVariableState as String = cfg.selectSingleNode("/vmix/dynamic/value" & markerDynamicVariable).InnerText
    if nowVariableState = "recording-marker-simple" or nowVariableState = "recording-marker-custom" then
        API.Function("SetDynamicValue" & markerDynamicVariable, Value := "")

        '-- determine duration(s) (in advance because of potentially interactive dialog)
        dim durationRecording    as String = ""
        dim durationMulticording as String = ""
        if recordingSince <> nothing and isRecording then
            dim diff as System.TimeSpan = DateTime.Now.Subtract(recordingSince)
            dim duration as DateTime = (new DateTime(0)).Add(diff)
            durationRecording = duration.ToString("HH:mm:ss.fff")
        end if
        if multicordingSince <> nothing and isMulticording then
            dim diff as System.TimeSpan = DateTime.Now.Subtract(multicordingSince)
            dim duration as DateTime = (new DateTime(0)).Add(diff)
            durationMulticording = duration.ToString("HH:mm:ss.fff")
        end if

        '-- create log entry(s)
        if durationRecording <> "" or durationMulticording <> "" then
            dim msg as String = markerStandardText

            '-- if a custom marker message is requestd, interactively ask the user for it
            '-- (NOTICE: we have to use WSH, as we cannot open an input dialog directly from within vMix VB.Net)
            if nowVariableState = "recording-marker-custom" then

                '-- determine two temporary file paths
                dim tempfile1 as String = System.IO.Path.GetTempFileName()
                dim tempfile2 as String = System.IO.Path.GetTempFileName()

                '-- create companion WSH script
                dim crlf as String = Environment.NewLine
                dim script as String = ""
                script = script & "file   = WScript.Arguments.Item(0)" & crlf
                script = script & "text   = WScript.Arguments.Item(1)" & crlf
                script = script & "text   = InputBox(""What is your textual annotation message to use for the recording marker?"", ""vMix: Recording-Log: Marker"", text)" & crlf
                script = script & "if text <> """" then" & crlf
                script = script & "    set fso = CreateObject(""Scripting.FileSystemObject"")" & crlf
                script = script & "    set out = fso.OpenTextFile(file, 8, true, -1)" & crlf
                script = script & "    out.Write(text)" & crlf
                script = script & "    out.Close()" & crlf
                script = script & "end if" & crlf
                System.IO.File.WriteAllText(tempfile1, script)

                '-- execute WSH in own process
                dim app as new ProcessStartInfo()
                app.FileName        = "wscript.exe"
                app.Arguments       = "/e:vbscript """ & tempfile1 & """ """ & tempfile2 & """ """ & msg & """"
                app.UseShellExecute = true
                app.CreateNoWindow  = true
                app.WindowStyle     = ProcessWindowStyle.Normal
                dim proc as Process = Process.Start(app)
                proc.WaitForExit()

                '-- read results form process
                '-- (NOTICE: we cannot use stdout here as WScript doesn't support stdout and CScript always opens a Terminal)
                dim utf8 as System.Text.Encoding = new System.Text.UTF8Encoding(true)
                msg = System.IO.File.ReadAllText(tempfile2, utf8)

                '-- cleanup temporary files
                System.IO.File.Delete(tempfile1)
                System.IO.File.Delete(tempfile2)
            end if

            '-- create log entries
            if durationRecording <> "" then
                log.Add("RECORDING   marked  (position: " & durationRecording & "): " & msg)
            end if
            if multicordingSince <> nothing and isMulticording then
                log.Add("MULTICORDER marked  (position: " & durationMulticording & "): " & msg)
            end if
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

        '-- signal new log entries with a sound
        My.Computer.Audio.PlaySystemSound(Media.SystemSounds.Exclamation)
    end if

    '-- wait a little bit before next iteration
    sleep(timeSlice)
loop

