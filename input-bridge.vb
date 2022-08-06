'--
'-- input-bridge.vb -- vMix script for Bridging Inputs between vMix instances
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--
'-- Language: VB.NET 2.0 (vMix 4K/Pro flavor)
'-- Version:  0.9.3 (2022-08-06)
'--

'-- CONFIGURATION
dim remoteAPI        as String  = "http://10.0.0.11:8088/API/"
dim remoteMixNum1    as String  = "1"
dim remoteMixNum2    as String  = "2"
dim localInputName1  as String  = "BRIDGE1"
dim localInputName2  as String  = "BRIDGE2"
dim timeSlice        as Integer = 50
dim debug            as Boolean = true

'-- prepare XML DOM tree and load the current API state
dim cfg as new System.Xml.XmlDocument
dim xml as String = API.XML()
cfg.LoadXml(xml)

'-- determine information of local NDI bridge inputs
dim localInputNum1 as String = cfg.SelectSingleNode("/vmix/inputs/input[@title = '" & localInputName1 & "']/@number").Value
dim localInputKey1 as String = cfg.SelectSingleNode("/vmix/inputs/input[@title = '" & localInputName1 & "']/@key").Value
dim localInputNum2 as String = cfg.SelectSingleNode("/vmix/inputs/input[@title = '" & localInputName2 & "']/@number").Value
dim localInputKey2 as String = cfg.SelectSingleNode("/vmix/inputs/input[@title = '" & localInputName2 & "']/@key").Value

'-- keep internal state
dim lastInPreview as String = ""
dim lastInProgram as String = ""

'-- track remote inputs
dim remoteInputName1 as String = ""
dim remoteInputName2 as String = ""

'-- endless loop
do while true
    '-- re-load the current API state
    xml = API.XML()
    cfg.LoadXml(xml)

    '-- determine what is currently in preview and in program
    dim nowInPreview as String = cfg.SelectSingleNode("/vmix/preview").InnerText
    dim nowInProgram as String = cfg.SelectSingleNode("/vmix/active").InnerText

    '-- only react if a new input was placed into preview
    '-- and the preview is not just the program
    if nowInPreview <> lastInPreview and nowInPreview <> nowInProgram then
        lastInPreview = nowInPreview
        dim inputName as String = ""
        if debug then
            inputName = cfg.SelectSingleNode("/vmix/inputs/input[@number = '" & nowInPreview & "']/@title").Value
            Console.WriteLine("input-bridge: INFO: PREVIEW change detected: input=" & inputName)
        end if

        '-- transitively iterate through the program input tree
        '-- and find out whether a bridge NDI input is used at all
        dim bridge1Found as Boolean = false
        dim bridge2Found as Boolean = false
        dim inputKey as String = cfg.SelectSingleNode("/vmix/inputs/input[@number = '" & nowInProgram & "']/@key").Value
        dim stack as System.Collections.Stack = new System.Collections.Stack()
        stack.Push(inputKey)
        do while stack.Count > 0
            inputKey = stack.Pop()
            if inputKey = localInputKey1 then
                bridge1Found = true
                exit do
            elseif inputKey = localInputKey2 then
                bridge2Found = true
                exit do
            else
                dim layers as XmlNodeList = cfg.SelectNodes("/vmix/inputs/input[@key = '" & inputKey & "']/overlay")
                for each layer as XmlNode in layers
                    dim layerKey as String = layer.Attributes("key").InnerText
                    stack.Push(layerKey)
                next
            end if
        loop
        if debug then
            if bridge1Found then
                Console.WriteLine("input-bridge: INFO: found bridge #1 usage in PROGRAM input tree")
            end if
            if bridge2Found then
                Console.WriteLine("input-bridge: INFO: found bridge #2 usage in PROGRAM input tree")
            end if
        end if

        '-- transitively iterate through the Preview input tree
        '-- in order to re-configure the bridge NDI input which should be used
        '-- (i.e. the one which is not used in Program)
        inputKey = cfg.SelectSingleNode("/vmix/inputs/input[@number = '" & nowInPreview & "']/@key").Value
        stack = new System.Collections.Stack()
        stack.Push(inputKey)
        do while stack.Count > 0
            inputKey = stack.Pop()

            '-- determine input details
            inputName = cfg.SelectSingleNode("/vmix/inputs/input[@key = '" & inputKey & "']/@title").Value
            dim inputNum as String = cfg.SelectSingleNode("/vmix/inputs/input[@key = '" & inputKey & "']/@number").Value
            dim inputOverlays as XmlNodeList = cfg.SelectNodes("/vmix/inputs/input[@key = '" & inputKey & "']/overlay")

            '-- check for the special input configuration we act on
            dim bridgeNumber   as Integer = 0     '-- the bridge to use
            dim bridgeChange   as Boolean = False '-- whether to change the bridge input on remote side
            dim bridgeOverlay  as Integer = -1    '-- the overlay of potential bridge input (layer number)
            for i as Integer = 0 to inputOverlays.Count - 1
                dim ovKey as String = inputOverlays.Item(i).Attributes("key").InnerText
                if ovKey = localInputKey1 then
                    bridgeOverlay = i
                    if not bridge1Found or (bridge1Found and remoteInputName1 = inputName) then
                        bridgeNumber = 1
                    else
                        bridgeNumber = 2
                        bridgeChange = true
                    end if
                elseif ovKey = localInputKey2 then
                    bridgeOverlay = i
                    if not bridge2Found or (bridge2Found and remoteInputName2 = inputName) then
                        bridgeNumber = 2
                    else
                        bridgeNumber = 1
                        bridgeChange = true
                    end if
                end if
                stack.Push(ovKey)
            next

            '-- react on special input configuration, if found
            if bridgeNumber <> 0 and bridgeOverlay <> -1 then
                if debug then
                    dim overlayName as String = cfg.SelectSingleNode("/vmix/inputs/input[@key = '" & inputOverlays.Item(bridgeOverlay).Attributes("key").InnerText & "']/@title").Value
                    Console.WriteLine("input-bridge: INFO: target input '" & inputName & "': found setup: layer-" & (bridgeOverlay + 1).toString() & "=" & overlayName)
                end if

                '-- reconfigure the remote vMix instance to send input
                '-- (but do not re-configure if not necessary to not let program flash)
                if (bridgeNumber = 1 and remoteInputName1 <> inputName) or (bridgeNumber = 2 and remoteInputName2 <> inputName) then
                    dim url as String = remoteAPI & "?Function=ActiveInput&Mix="
                    if bridgeNumber = 1 then
                        if debug then
                            Console.WriteLine("input-bridge: INFO: target input '" & inputName & "': route: remote-mix=" & remoteMixNum1 & " local-bridge=" & localInputName1)
                        end if
                        url = url & remoteMixNum1 & "&Input=" & inputName
                        remoteInputName1 = inputName
                    else
                        if debug then
                            Console.WriteLine("input-bridge: INFO: target input '" & inputName & "': route: remote-mix=" & remoteMixNum2 & " local-bridge=" & localInputName2)
                        end if
                        url = url & remoteMixNum2 & "&Input=" & inputName
                        remoteInputName2 = inputName
                    end if
                    dim request  as HttpWebRequest  = HttpWebRequest.Create(url)
                    dim response as HttpWebResponse = request.GetResponse()
                    dim stream as Stream = response.GetResponseStream()
                    dim streamReader as new StreamReader(stream)
                    while streamReader.Peek >= 0
                        dim data as String = streamReader.ReadToEnd()
                    end while
                    sleep(100)
                end if

                '-- optionally re-configure local input layer to receive input
                if bridgeChange then
                    if bridgeNumber = 1 then
                        if debug then
                            Console.WriteLine("input-bridge: INFO: target input '" & inputName & "': reconfigure: layer-" & (bridgeOverlay + 1).toString() & "=" & localInputName1)
                        end if
                        API.Function("SetMultiViewOverlay", Input := inputNum, Value := (bridgeOverlay + 1).toString() & "," & localInputNum1)
                    else
                        if debug then
                            Console.WriteLine("input-bridge: INFO: target input '" & inputName & "': reconfigure: layer-" & (bridgeOverlay + 1).toString() & "=" & localInputName2)
                        end if
                        API.Function("SetMultiViewOverlay", Input := inputNum, Value := (bridgeOverlay + 1).toString() & "," & localInputNum2)
                    end if
                end if
            end if
        loop
    end if

    '-- wait a little bit before next iteration
    sleep(timeSlice)
loop

