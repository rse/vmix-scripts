'--
'-- input-mirror.vb -- vMix script for Mirroring Input Selection on vMix Slave Instance
'-- Copyright (c) 2023-2023 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--
'-- Language: VB.NET 2.0 (vMix 4K/Pro flavor)
'-- Version:  0.9.1 (2023-06-30)
'--

'-- ==== CONFIGURATION ====

dim peerAPI     as String  = "http://127.0.0.1:8188/API/" '-- peer vMix HTTP API endpoint
dim timeSlice   as Integer = 50                           '-- time slice of processing interval
dim debug       as Boolean = false                        '-- whether to output debug messages

'-- ==== STATE ====

'-- prepare XML DOM tree and load the current API state
dim cfg as System.Xml.XmlDocument = new System.Xml.XmlDocument()
dim xml as String = API.XML()
cfg.LoadXml(xml)

'-- track last preview/program state (locally)
dim inputInPreviewLast as String = ""
dim inputInProgramLast as String = ""

'-- track last preview/program state (remotely)
dim inputInPreviewRemoteLast as String = ""
dim inputInProgramRemoteLast as String = ""

'-- ==== PROCESSING ====

'-- endless processing loop
do while true
    '-- re-load the current API state
    xml = API.XML()
    cfg.LoadXml(xml)

    '-- determine what input is currently in preview and in program
    dim inputInPreviewNowNum as String = cfg.SelectSingleNode("/vmix/preview").InnerText
    dim inputInProgramNowNum as String = cfg.SelectSingleNode("/vmix/active").InnerText
    dim inputInPreviewNow as String = cfg.SelectSingleNode("/vmix/inputs/input[@number = '" & inputInPreviewNowNum & "']/@title").Value
    dim inputInProgramNow as String = cfg.SelectSingleNode("/vmix/inputs/input[@number = '" & inputInProgramNowNum & "']/@title").Value

    '-- detect if a new input was placed into program
    dim changeProgramToInput as String = ""
    if inputInProgramNow <> inputInProgramLast then
        '-- print detected change
        if debug then
            Console.WriteLine("input-mirror: INFO: PROGRAM change detected: input=" & inputInProgramNow)
        end if
        changeProgramToInput = inputInProgramNow
    end if

    '-- detect if a new input was placed into preview
    dim changePreviewToInput as String = ""
    if inputInPreviewNow <> inputInPreviewLast then
        '-- print detected change
        if debug then
            Console.WriteLine("input-mirror: INFO: PREVIEW change detected: input=" & inputInPreviewNow)
        end if
        changePreviewToInput = inputInPreviewNow
    end if

    '-- update remote preview and program if a local change was done
    if inputInPreviewRemoteLast = changeProgramToInput and inputInProgramRemoteLast = changePreviewToInput then
        '-- optimized all-in-one special case operation
        if debug then
            Console.WriteLine("input-mirror: INFO: remote: swapping preview '" & inputInPreviewRemoteLast & "' with program '" & inputInProgramRemoteLast & "'")
        end if
        dim url as String = peerAPI & "?Function=Cut"
        dim webClient as System.Net.WebClient = new System.Net.WebClient()
        webClient.DownloadString(url)
        inputInPreviewRemoteLast = changePreviewToInput
        inputInProgramRemoteLast = changeProgramToInput
    else
        if changeProgramToInput <> "" then
            '-- individual update operation
            if debug then
                Console.WriteLine("input-mirror: INFO: remote: switching program to '" & changeProgramToInput & "'")
            end if
            dim url as String = peerAPI & "?Function=CutDirect&Input=" & changeProgramToInput
            dim webClient as System.Net.WebClient = new System.Net.WebClient()
            webClient.DownloadString(url)
            inputInProgramRemoteLast = changeProgramToInput
        end if
        if changePreviewToInput <> "" then
            '-- individual update operation
            if debug then
                Console.WriteLine("input-mirror: INFO: remote: switching preview to '" & changePreviewToInput & "'")
            end if
            dim url as String = peerAPI & "?Function=PreviewInput&Input=" & changePreviewToInput
            dim webClient as System.Net.WebClient = new System.Net.WebClient()
            webClient.DownloadString(url)
            inputInPreviewRemoteLast = changePreviewToInput
        end if
    end if

    '-- finally remember new states
    if inputInProgramNow <> inputInProgramLast then
        inputInProgramLast = inputInProgramNow
    end if
    if inputInPreviewNow <> inputInPreviewLast then
        inputInPreviewLast = inputInPreviewNow
    end if

    '-- wait a little bit before next iteration
    sleep(timeSlice)
loop

