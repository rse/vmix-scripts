'--
'-- input-bridge.vb -- vMix script for Bridging Inputs between vMix instances
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--
'-- Language: VB.NET 2.0 (vMix 4K/Pro flavor)
'-- Version:  1.0.0 (2022-10-13)
'--

'-- ==== CONFIGURATION ====

dim peerAPI          as String  = "http://10.0.0.11:8088/API/" '-- peer vMix HTTP API endpoint       (remote only)
dim bridge1MixNum    as String  = "1"                          '-- mix number of bridge #1           (remote only)
dim bridge2MixNum    as String  = "2"                          '-- mix number of bridge #2           (remote only)
dim bridge1InputName as String  = "BRIDGE1"                    '-- input name of bridge #1           (local and remote)
dim bridge2InputName as String  = "BRIDGE2"                    '-- input name of bridge #2           (local and remote)
dim timeSlice        as Integer = 50                           '-- time slice of processing interval (local only)
dim debug            as Boolean = false                        '-- wether to output debug messages   (local only)

'-- ==== STATE ====

'-- prepare XML DOM tree and load the current API state
dim cfg as System.Xml.XmlDocument = new System.Xml.XmlDocument()
dim xml as String = API.XML()
cfg.LoadXml(xml)

'-- pre-determine information of bridge inputs
dim bridge1InputNum as String = cfg.SelectSingleNode("/vmix/inputs/input[@title = '" & bridge1InputName & "']/@number").Value
dim bridge1InputKey as String = cfg.SelectSingleNode("/vmix/inputs/input[@title = '" & bridge1InputName & "']/@key").Value
dim bridge2InputNum as String = cfg.SelectSingleNode("/vmix/inputs/input[@title = '" & bridge2InputName & "']/@number").Value
dim bridge2InputKey as String = cfg.SelectSingleNode("/vmix/inputs/input[@title = '" & bridge2InputName & "']/@key").Value

'-- track which source input is in the bridges (on the peer side)
dim bridge1SourceInputName as String = ""
dim bridge2SourceInputName as String = ""

'-- track which bridge is currently used (directly or indirectly) in preview and program
dim bridgeInPreview as Integer = 0
dim bridgeInProgram as Integer = 0

'-- track last preview/program state
dim inputInPreviewLast as String = ""
dim inputInProgramLast as String = ""

'-- ==== PROCESSING ====

'-- endless processing loop
do while true
    '-- re-load the current API state
    xml = API.XML()
    cfg.LoadXml(xml)

    '-- determine what input is currently in preview and in program
    dim inputInPreviewNow as String = cfg.SelectSingleNode("/vmix/preview").InnerText
    dim inputInProgramNow as String = cfg.SelectSingleNode("/vmix/active").InnerText

    '-- react if a new input was placed into program
    if inputInProgramNow <> inputInProgramLast then
        '-- print detected change
        if debug then
            dim inputName as String = cfg.SelectSingleNode("/vmix/inputs/input[@number = '" & inputInProgramNow & "']/@title").Value
            Console.WriteLine("input-bridge: INFO: PROGRAM change detected: input=" & inputName)
        end if

        '-- transitively iterate through the program input tree
        '-- and find out whether a bridge input is used at all
        bridgeInProgram = 0
        dim inputKey as String = cfg.SelectSingleNode("/vmix/inputs/input[@number = '" & inputInProgramNow & "']/@key").Value
        dim stack as System.Collections.Stack = new System.Collections.Stack()
        stack.Push(inputKey)
        do while stack.Count > 0
            inputKey = stack.Pop()
            if inputKey = bridge1InputKey then
                bridgeInProgram = 1
                exit do
            elseif inputKey = bridge2InputKey then
                bridgeInProgram = 2
                exit do
            else
                dim layers as XmlNodeList = cfg.SelectNodes("/vmix/inputs/input[@key = '" & inputKey & "']/overlay")
                for each layer as XmlNode in layers
                    dim layerKey as String = layer.Attributes("key").Value
                    stack.Push(layerKey)
                next
            end if
        loop
        if bridgeInProgram > 0 and debug then
            Console.WriteLine("input-bridge: INFO: found bridge #" & bridgeInProgram & " usage in PROGRAM input tree")
        end if
    end if

    '-- react if a new input was placed into preview
    if inputInPreviewNow <> inputInPreviewLast then
        '-- print detected change
        if debug then
            dim inputName as String = cfg.SelectSingleNode("/vmix/inputs/input[@number = '" & inputInPreviewNow & "']/@title").Value
            Console.WriteLine("input-bridge: INFO: PREVIEW change detected: input=" & inputName)
        end if

        '-- transitively iterate through the Preview input tree
        '-- in order to re-configure the bridge input which should be used
        '-- (i.e. the one which is not already used in the Program input tree)
        bridgeInPreview = 0
        dim inputKey as String = cfg.SelectSingleNode("/vmix/inputs/input[@number = '" & inputInPreviewNow & "']/@key").Value
        dim stack as System.Collections.Stack = new System.Collections.Stack()
        stack.Push(inputKey)
        do while stack.Count > 0
            inputKey = stack.Pop()

            '-- consistency check to ensure input (still or at all) exists
            '-- (notice: vMix sometimes has inconsistent states)
            dim inputs as XmlNodeList = cfg.SelectNodes("/vmix/inputs/input[@key = '" & inputKey & "']")
            if inputs.Count <> 1 then
                Console.WriteLine("input-bridge: WARNING: found inconsistent vMix API state: input key '" & inputKey & "' not existing (skipping)")
                continue do
            end if

            '-- determine input details
            dim inputName as String = cfg.SelectSingleNode("/vmix/inputs/input[@key = '" & inputKey & "']/@title").Value
            dim inputNum  as String = cfg.SelectSingleNode("/vmix/inputs/input[@key = '" & inputKey & "']/@number").Value
            dim inputOverlays as XmlNodeList = cfg.SelectNodes("/vmix/inputs/input[@key = '" & inputKey & "']/overlay")

            '-- check for the special input configuration we act on
            dim inputChange  as Boolean = False '-- whether to change the bridge input on remote side
            dim inputOverlay as Integer = -1    '-- the overlay of bridge input (layer number)
            for i as Integer = 0 to inputOverlays.Count - 1
                dim ovKey as String = inputOverlays.Item(i).Attributes("key").Value
                if ovKey = bridge1InputKey then
                    inputOverlay = i
                    if bridgeInProgram <> 1 or (bridgeInProgram = 1 and bridge1SourceInputName = inputName) then
                        bridgeInPreview = 1
                    else
                        bridgeInPreview = 2
                        inputChange = true
                    end if
                elseif ovKey = bridge2InputKey then
                    inputOverlay = i
                    if bridgeInProgram <> 2 or (bridgeInProgram = 2 and bridge2SourceInputName = inputName) then
                        bridgeInPreview = 2
                    else
                        bridgeInPreview = 1
                        inputChange = true
                    end if
                end if
                stack.Push(ovKey)
            next

            '-- react on special input configuration, if found
            if bridgeInPreview <> 0 and inputOverlay <> -1 then
                if debug then
                    dim overlayName as String = cfg.SelectSingleNode("/vmix/inputs/input[@key = '" & inputOverlays.Item(inputOverlay).Attributes("key").Value & "']/@title").Value
                    Console.WriteLine("input-bridge: INFO: target input '" & inputName & "': found setup: layer-" & (inputOverlay + 1).toString() & "=" & overlayName)
                end if

                '-- reconfigure the remote vMix instance to send input
                '-- (but do not re-configure if not necessary to not let program flash)
                if (bridgeInPreview = 1 and bridge1SourceInputName <> inputName) or (bridgeInPreview = 2 and bridge2SourceInputName <> inputName) then
                    dim url as String = peerAPI & "?Function=ActiveInput&Mix="
                    if bridgeInPreview = 1 then
                        if debug then
                            Console.WriteLine("input-bridge: INFO: target input '" & inputName & "': route: remote-mix=" & bridge1MixNum & " local-bridge=" & bridge1InputName)
                        end if
                        url = url & bridge1MixNum & "&Input=" & inputName
                        bridge1SourceInputName = inputName
                    else
                        if debug then
                            Console.WriteLine("input-bridge: INFO: target input '" & inputName & "': route: remote-mix=" & bridge2MixNum & " local-bridge=" & bridge2InputName)
                        end if
                        url = url & bridge2MixNum & "&Input=" & inputName
                        bridge2SourceInputName = inputName
                    end if
                    dim webClient as System.Net.WebClient = new System.Net.WebClient()
                    webClient.DownloadString(url)
                end if

                '-- optionally re-configure local input layer to receive input
                if inputChange then
                    if bridgeInPreview = 1 then
                        if debug then
                            Console.WriteLine("input-bridge: INFO: target input '" & inputName & "': reconfigure: layer-" & (inputOverlay + 1).toString() & "=" & bridge1InputName)
                        end if
                        API.Function("SetMultiViewOverlay", Input := inputNum, Value := (inputOverlay + 1).toString() & "," & bridge1InputNum)
                    else
                        if debug then
                            Console.WriteLine("input-bridge: INFO: target input '" & inputName & "': reconfigure: layer-" & (inputOverlay + 1).toString() & "=" & bridge2InputName)
                        end if
                        API.Function("SetMultiViewOverlay", Input := inputNum, Value := (inputOverlay + 1).toString() & "," & bridge2InputNum)
                    end if
                end if
            end if
        loop
    end if

    '-- update remote preview if a local change was done
    if inputInPreviewNow <> inputInPreviewLast and bridgeInPreview <> 0 then
        dim url as String = peerAPI & "?Function=PreviewInput&Input="
        if bridgeInPreview = 1 then
            url = url & bridge1SourceInputName
        elseif bridgeInPreview = 2 then
            url = url & bridge2SourceInputName
        end if
        dim webClient as System.Net.WebClient = new System.Net.WebClient()
        webClient.DownloadString(url)
    end if

    '-- update remote program if a local change was done
    if inputInProgramNow <> inputInProgramLast and bridgeInProgram <> 0 then
        dim url as String = peerAPI & "?Function=ActiveInput&Input="
        if bridgeInProgram = 1 then
            url = url & bridge1SourceInputName
        elseif bridgeInProgram = 2 then
            url = url & bridge2SourceInputName
        end if
        dim webClient as System.Net.WebClient = new System.Net.WebClient()
        webClient.DownloadString(url)
    end if

    '-- finally remember new states
    if inputInPreviewNow <> inputInPreviewLast then
        inputInPreviewLast = inputInPreviewNow
    end if
    if inputInProgramNow <> inputInProgramLast then
        inputInProgramLast = inputInProgramNow
    end if

    '-- wait a little bit before next iteration
    sleep(timeSlice)
loop

