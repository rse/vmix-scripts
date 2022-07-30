'--
'-- input-bridge.vb -- vMix script for Bridging Inputs between vMix instances
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--
'-- Language: VB.NET 2.0 (vMix 4K/Pro flavor)
'-- Version:  0.9.0 (2022-07-27)
'--

'-- CONFIGURATION
dim remoteAPI              as String  = "http://10.0.0.11:8088/API/"
dim remoteOutputNumber1    as String  = "3"
dim remoteOutputNumber2    as String  = "4"
dim remoteNDIStreamName1   as String  = "COMPUTER1 (vMix - Output 3)"
dim remoteNDIStreamName2   as String  = "COMPUTER1 (vMix - Output 4)"
dim localNDIInputName1     as String  = "BRIDGE1"
dim localNDIInputName2     as String  = "BRIDGE2"
dim localInputNamePrefix   as String  = "VPTZ - "
dim timeSlice              as Integer = 100
dim debug                  as Boolean = true

'-- prepare XML DOM tree and load the current API state
dim cfg as new System.Xml.XmlDocument
dim xml as String = API.XML()
cfg.LoadXml(xml)

'-- determine information of local NDI inputs
dim localNDIInputNum1 as String = cfg.SelectSingleNode("/vmix/inputs/input[@title = '" & localNDIInputName1 & "']/@number").Value
dim localNDIInputKey1 as String = cfg.SelectSingleNode("/vmix/inputs/input[@title = '" & localNDIInputName1 & "']/@key").Value
dim localNDIInputNum2 as String = cfg.SelectSingleNode("/vmix/inputs/input[@title = '" & localNDIInputName2 & "']/@number").Value
dim localNDIInputKey2 as String = cfg.SelectSingleNode("/vmix/inputs/input[@title = '" & localNDIInputName2 & "']/@key").Value

'-- keep internal state
dim lastInPreview as String = ""
dim lastInProgram as String = ""

'-- enforce NDI streams on bridge NDI inputs
API.Function("NDISelectSourceByName", Input := localNDIInputName1, Value := remoteNDIStreamName1)
API.Function("NDISelectSourceByName", Input := localNDIInputName2, Value := remoteNDIStreamName2)

'-- track inputs received via NDI
dim remoteInputName1 = ""
dim remoteInputName2 = ""

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
            if debug then
                inputName = cfg.SelectSingleNode("/vmix/inputs/input[@key = '" & inputKey & "']/@title").Value
                Console.WriteLine("input-bridge: DEBUG: crawl PROGRAM input tree: input=" & inputName)
            end if
			if inputKey = localNDIInputKey1 then
                bridge1Found = true
                exit do
            elseif inputKey = localNDIInputKey2 then
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
                Console.WriteLine("input-bridge: INFO: found bridge 1 usage in PROGRAM input tree: input=" & localNDIInputName1)
            end if
            if bridge2Found then
                Console.WriteLine("input-bridge: INFO: found bridge 2 usage in PROGRAM input tree: input=" & localNDIInputName2)
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
            if debug then
                inputName = cfg.SelectSingleNode("/vmix/inputs/input[@key = '" & inputKey & "']/@title").Value
                Console.WriteLine("input-bridge: DEBUG: crawl PREVIEW input tree: input=" & inputName)
            end if

            '-- determine input details
            dim targetNum as String = cfg.SelectSingleNode("/vmix/inputs/input[@key = '" & inputKey & "']/@number").Value
            dim targetOverlays as XmlNodeList = cfg.SelectNodes("/vmix/inputs/input[@key = '" & inputKey & "']/overlay")

            '-- check for the special input configuration we act on
            dim bridgeNumber   as Integer = 0     '-- the bridge NDI number to use
            dim bridgeChange   as Boolean = False '-- whether to change the NDI bridge input
            dim bridgeOverlay  as Integer = -1    '-- the overlay of potential NDI bridge input (layer number)
            for i as Integer = 0 to targetOverlays.Count - 1
                dim ovKey as String = targetOverlays.Item(i).Attributes("key").InnerText
                if ovKey = localNDIInputKey1 then
                    bridgeOverlay = i
                    if not bridge1Found then
                        bridgeNumber = 1
                    else
                        bridgeNumber = 2
                        bridgeChange = true
                    end if
                elseif ovKey = localNDIInputKey2 then
                    bridgeOverlay = i
                    if not bridge2Found then
                        bridgeNumber = 2
                    else
                        bridgeNumber = 1
                        bridgeChange = true
                    end if
                end if
                stack.Push(ovKey)
            next

            '-- react on special input configuration, if found
            if inputName.Substring(0, localInputNamePrefix.Length) = localInputNamePrefix and bridgeNumber <> 0 and bridgeOverlay <> -1 then
                if debug then
                    dim overlayName as String = cfg.SelectSingleNode("/vmix/inputs/input[@key = '" & targetOverlays.Item(bridgeOverlay).Attributes("key").InnerText & "']/@title").Value
                    Console.WriteLine("input-bridge: INFO: target input '" & inputName & "': found setup: layer-" & (bridgeOverlay + 1).toString() & "=" & overlayName)
                end if

                '-- reconfigure the remote vMix instance to send input
				'-- (but do not re-configure if not necessary to not let program flash)
                if (bridgeNumber = 1 and remoteInputName1 <> inputName) or (bridgeNumber = 2 and remoteInputName2 <> inputName) then
				    dim url as String = ""
                    if bridgeNumber = 1 then
                        if debug then
                            Console.WriteLine("input-bridge: INFO: target input '" & inputName & "': route: output=" & remoteOutputNumber1 & " stream='" & remoteNDIStreamName1 & "' bridge=" & localNDIInputName1)
                        end if
                        url = remoteAPI & "?Function=SetOutput" & remoteOutputNumber1 & "&Value=Input&Input=" & inputName
						remoteInputName1 = inputName
                    else
                        if debug then
                            Console.WriteLine("input-bridge: INFO: target input '" & inputName & "': route: output=" & remoteOutputNumber2 & " stream='" & remoteNDIStreamName2 & "' bridge=" & localNDIInputName2)
                        end if
                        url = remoteAPI & "?Function=SetOutput" & remoteOutputNumber2 & "&Value=Input&Input=" & inputName
						remoteInputName2 = inputName
                    end if
                    dim request  as HttpWebRequest  = HttpWebRequest.Create(url)
                    dim response as HttpWebResponse = request.GetResponse()
                    dim stream as Stream = response.GetResponseStream()
                    dim streamReader as new StreamReader(stream)
                    while streamReader.Peek >= 0
                        dim data as String = streamReader.ReadToEnd()
                    end while
				end if

                '-- optionally re-configure local input layer to receive input
                if bridgeChange then
                    if bridgeNumber = 1 then
                        if debug then
                            Console.WriteLine("input-bridge: INFO: target input '" & inputName & "': reconfigure: layer-" & (bridgeOverlay + 1).toString() & "=" & localNDIInputName1 & "(" & localNDIInputNum1 & ")")
                        end if
                        API.Function("SetMultiViewOverlay", Input := targetNum, Value := (bridgeOverlay + 1).toString() & "," & localNDIInputNum1)
                    else
                        if debug then
                            Console.WriteLine("input-bridge: INFO: target input '" & inputName & "': reconfigure: layer-" & (bridgeOverlay + 1).toString() & "=" & localNDIInputName2 & "(" & localNDIInputNum2 & ")")
                        end if
                        API.Function("SetMultiViewOverlay", Input := targetNum, Value := (bridgeOverlay + 1).toString() & "," & localNDIInputNum2)
                    end if
                end if
            end if
        loop
    end if

    '-- wait a little bit before next iteration
    sleep(timeSlice)
loop

