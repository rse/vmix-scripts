'--
'-- multiview-overlay.vb -- vMix script for Updating Custom Multiview Overlays
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--
'-- Language: VB.NET 2.0 (vMix 4K/Pro flavor)
'-- Version:  0.9.0 (2022-08-28)
'--

'-- CONFIGURATION
dim prefixCamAngle    as String   = "VPTZ - CAM"
dim prefixListPreview as String   = "MULTIVIEW-OV-PREVIEW - CAM"
dim prefixListProgram as String   = "MULTIVIEW-OV-PROGRAM - CAM"
dim angles            as String() = { "C-L", "C-C", "C-R", "F-L", "F-C", "F-R", "W-C" }
dim numberOfCams      as Integer  = 4
dim timeSlice         as Integer  = 50
dim debug             as Boolean  = true

'-- prepare XML DOM tree and load the current API state
dim cfg as new System.Xml.XmlDocument
dim xml as String = API.XML()
cfg.LoadXml(xml)

'-- keep internal state of active preview/program
dim lastInPreview as String = ""
dim lastInProgram as String = ""

'-- keep internal state of still cleared preview/program overlay
dim clearedPreview as Boolean() = new Boolean(numberOfCams) {}
dim clearedProgram as Boolean() = new Boolean(numberOfCams) {}
for i as Integer = 0 to numberOfCams - 1
    clearedPreview(i) = false
    clearedProgram(i) = false
next

'-- endless loop
do while true
    '-- re-load the current API state
    xml = API.XML()
    cfg.LoadXml(xml)

    '-- determine what is currently in preview
    dim nowInPreview as String = cfg.SelectSingleNode("/vmix/preview").InnerText
    dim nowInProgram as String = cfg.SelectSingleNode("/vmix/active").InnerText

    '-- react if a new input was placed into preview
    if nowInPreview <> lastInPreview then
        lastInPreview = nowInPreview
        dim inputName as String = cfg.SelectSingleNode("/vmix/inputs/input[@number = '" & nowInPreview & "']/@title").Value
        if debug then
            Console.WriteLine("multiview-update: INFO: PREVIEW change detected: input=" & inputName)
        end if
        dim changed as Boolean = false
        if inputName.length > prefixCamAngle.length then
            if inputName.substring(0, prefixCamAngle.length) = prefixCamAngle then
                dim cam   as Integer = Convert.ToInt32(inputName.substring(prefixCamAngle.length, 1))
                dim angle as String  = inputName.substring(prefixCamAngle.length + 2)
                dim idx   as Integer = Array.IndexOf(angles, angle)
                if idx >= 0 then
                    if debug then
                        Console.WriteLine("multiview-update: INFO: updating multiview PREVIEW overlay of CAM" & cam)
                    end if
                    API.Function("SelectIndex", Input := prefixListPreview & cam, Value := (idx + 2).toString())
                    clearedPreview(cam - 1) = false
                    for i as Integer = 1 to numberOfCams
                        if i <> cam then
                            if clearedPreview(i - 1) = false then
                                if debug then
                                    Console.WriteLine("multiview-update: INFO: clearing multiview PREVIEW overlay of CAM" & i.toString())
                                end if
                                API.Function("SelectIndex", Input := prefixListPreview & i.toString(), Value := "1")
                                clearedPreview(i - 1) = true
                            end if
                        end if
                    next
                    changed = true
                end if
            end if
        end if
        if not changed then
            for i as Integer = 1 to numberOfCams
                if clearedPreview(i - 1) = false then
                    if debug then
                        Console.WriteLine("multiview-update: INFO: clearing multiview PREVIEW overlay of CAM" & i.toString())
                    end if
                    API.Function("SelectIndex", Input := prefixListPreview & i.toString(), Value := "1")
                    clearedPreview(i - 1) = true
                end if
            next
        end if
    end if

    '-- react if a new input was placed into program
    if nowInProgram <> lastInProgram then
        lastInProgram = nowInProgram
        dim inputName as String = cfg.SelectSingleNode("/vmix/inputs/input[@number = '" & nowInProgram & "']/@title").Value
        if debug then
            Console.WriteLine("multiview-update: INFO: PROGRAM change detected: input=" & inputName)
        end if
        dim changed as Boolean = false
        if inputName.length > prefixCamAngle.length then
            if inputName.substring(0, prefixCamAngle.length) = prefixCamAngle then
                dim cam   as Integer = Convert.ToInt32(inputName.substring(prefixCamAngle.length, 1))
                dim angle as String  = inputName.substring(prefixCamAngle.length + 2)
                dim idx   as Integer = Array.IndexOf(angles, angle)
                if idx >= 0 then
                    if debug then
                        Console.WriteLine("multiview-update: INFO: updating multiview PROGRAM overlay of CAM" & cam)
                    end if
                    API.Function("SelectIndex", Input := prefixListProgram & cam, Value := (idx + 2).toString())
                    clearedProgram(cam - 1) = false
                    for i as Integer = 1 to numberOfCams
                        if i <> cam then
                            if clearedProgram(i - 1) = false then
                                if debug then
                                    Console.WriteLine("multiview-update: INFO: clearing multiview PROGRAM overlay of CAM" & i.toString())
                                end if
                                API.Function("SelectIndex", Input := prefixListProgram & i.toString(), Value := "1")
                                clearedProgram(i - 1) = true
                            end if
                        end if
                    next
                    changed = true
                end if
            end if
        end if
        if not changed then
            for i as Integer = 1 to numberOfCams
                if clearedProgram(i - 1) = false then
                    if debug then
                        Console.WriteLine("multiview-update: INFO: clearing multiview PREVIEW overlay of CAM" & i.toString())
                    end if
                    API.Function("SelectIndex", Input := prefixListProgram & i.toString(), Value := "1")
                    clearedProgram(i - 1) = true
                end if
            next
        end if
    end if

    '-- wait a little bit before next iteration
    sleep(timeSlice)
loop

