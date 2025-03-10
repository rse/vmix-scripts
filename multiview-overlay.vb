'--
'-- multiview-overlay.vb -- vMix script for Updating Custom Multiview Overlays
'-- Copyright (c) 2022-2023 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--
'-- Language: VB.NET 2.0 (vMix 4K/Pro flavor)
'-- Version:  0.9.4 (2025-02-17)
'--

'-- CONFIGURATION
dim numberOfCams            as Integer  = 4
dim angleInputPrefix        as String   = "VPTZ - CAM"
dim angleInputPrefixPHYS    as String   = "PTZ - CAM"
dim angleInputPostfixes     as String() = { "C-L", "C-C", "C-R", "F-L", "F-C", "F-R", "W-C" }
dim titlePreviewInputPrefix as String   = "MULTIVIEW-OV-PREVIEW - CAM"
dim titleProgramInputPrefix as String   = "MULTIVIEW-OV-PROGRAM - CAM"
dim multiviewInputPrefix    as String   = "MULTIVIEW - CAM"
dim multiviewInputPHYS      as String   = "MULTIVIEW - CAMx"
dim multiviewInputNOCAM     as String   = "MULTIVIEW - NOCAM"
dim multiviewOutputId       as String   = "3"
dim timeSlice               as Integer  = 50
dim debug                   as Boolean  = false

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

'-- keep internal state of current preview camera
dim lastPreviewCam as Integer = 0

'-- keep internal state of current mode
dim lastMode as String = ""

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
        if inputName.length > angleInputPrefixPHYS.length then
            if inputName.substring(0, angleInputPrefixPHYS.length) = angleInputPrefixPHYS then
                if lastMode <> "PHYS" then
                    API.Function("PreviewInput", Input := multiviewInputPHYS, Mix := multiviewOutputId)
                    API.Function("ActiveInput",  Input := multiviewInputPHYS, Mix := multiviewOutputId)
                    lastPreviewCam = 0
                    lastMode = "PHYS"
                end if
            end if
        end if
        if inputName.length > angleInputPrefix.length then
            if inputName.substring(0, angleInputPrefix.length) = angleInputPrefix then
                lastMode = "VIRT"
                dim cam   as Integer = Convert.ToInt32(inputName.substring(angleInputPrefix.length, 1))
                dim angle as String  = inputName.substring(angleInputPrefix.length + 2)
                dim idx   as Integer = Array.IndexOf(angleInputPostfixes, angle)
                if idx >= 0 then
                    if debug then
                        Console.WriteLine("multiview-update: INFO: updating multiview PREVIEW overlay of CAM" & cam)
                    end if
                    API.Function("TitleBeginAnimation", Input := titlePreviewInputPrefix & cam, Value := "Page" & (idx + 1).toString())
                    clearedPreview(cam - 1) = false
                    for i as Integer = 1 to numberOfCams
                        if i <> cam then
                            if clearedPreview(i - 1) = false then
                                if debug then
                                    Console.WriteLine("multiview-update: INFO: clearing multiview PREVIEW overlay of CAM" & i.toString())
                                end if
                                API.Function("TitleBeginAnimation", Input := titlePreviewInputPrefix & i.toString(), Value := "TransitionOut")
                                clearedPreview(i - 1) = true
                            end if
                        end if
                    next
                    if lastPreviewCam <> cam then
                        lastPreviewCam = cam
                        if debug then
                            Console.WriteLine("multiview-update: INFO: switching MULTIVIEW to CAM" & cam.toString())
                        end if
                        API.Function("PreviewInput", Input := multiviewInputPrefix & cam.toString(), Mix := multiviewOutputId)
                        API.Function("ActiveInput",  Input := multiviewInputPrefix & cam.toString(), Mix := multiviewOutputId)
                    end if
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
                    API.Function("TitleBeginAnimation", Input := titlePreviewInputPrefix & i.toString(), Value := "TransitionOut")
                    clearedPreview(i - 1) = true
                end if
            next
            API.Function("PreviewInput", Input := multiviewInputNOCAM, Mix := multiviewOutputId)
            API.Function("ActiveInput",  Input := multiviewInputNOCAM, Mix := multiviewOutputId)
            lastPreviewCam = 0
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
        if inputName.length > angleInputPrefix.length then
            if inputName.substring(0, angleInputPrefix.length) = angleInputPrefix then
                dim cam   as Integer = Convert.ToInt32(inputName.substring(angleInputPrefix.length, 1))
                dim angle as String  = inputName.substring(angleInputPrefix.length + 2)
                dim idx   as Integer = Array.IndexOf(angleInputPostfixes, angle)
                if idx >= 0 then
                    if debug then
                        Console.WriteLine("multiview-update: INFO: updating multiview PROGRAM overlay of CAM" & cam)
                    end if
                    API.Function("TitleBeginAnimation", Input := titleProgramInputPrefix & cam, Value := "Page" & (idx + 1).toString())
                    clearedProgram(cam - 1) = false
                    for i as Integer = 1 to numberOfCams
                        if i <> cam then
                            if clearedProgram(i - 1) = false then
                                if debug then
                                    Console.WriteLine("multiview-update: INFO: clearing multiview PROGRAM overlay of CAM" & i.toString())
                                end if
                                API.Function("TitleBeginAnimation", Input := titleProgramInputPrefix & i.toString(), Value := "TransitionOut")
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
                    API.Function("TitleBeginAnimation", Input := titleProgramInputPrefix & i.toString(), Value := "TransitionOut")
                    clearedProgram(i - 1) = true
                end if
            next
        end if
    end if

    '-- wait a little bit before next iteration
    sleep(timeSlice)
loop

