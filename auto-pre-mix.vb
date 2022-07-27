'--
'-- auto-pre-mix.vb -- vMix script for Automatically Pre-Mixing Inputs
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--
'-- Language: VB.NET 2.0 (vMix 4K/Pro flavor)
'-- Version:  0.9.0 (2022-07-03)
'--

'-- CONFIGURATION
dim mix1MixNumber  as String  = "1"
dim mix1InputName  as String  = "PRERENDER1"
dim mix2MixNumber  as String  = "2"
dim mix2InputName  as String  = "PRERENDER2"
dim timeSlice      as Integer = 250
dim debug          as Boolean = true

'-- prepare XML DOM tree and load the current API state
dim cfg as new System.Xml.XmlDocument
dim xml as String = API.XML()
cfg.LoadXml(xml)

'-- determine UUIDs of mix inputs
dim mix1InputNum as String = cfg.SelectSingleNode("/vmix/inputs/input[@title = '" & mix1InputName & "']/@number").Value
dim mix1InputKey as String = cfg.SelectSingleNode("/vmix/inputs/input[@title = '" & mix1InputName & "']/@key").Value
dim mix2InputNum as String = cfg.SelectSingleNode("/vmix/inputs/input[@title = '" & mix2InputName & "']/@number").Value
dim mix2InputKey as String = cfg.SelectSingleNode("/vmix/inputs/input[@title = '" & mix2InputName & "']/@key").Value

'-- keep internal state
dim lastInPreview as String = ""
dim lastInProgram as String = ""

'-- endless loop
do while true
    '-- re-load the current API state
    xml = API.XML()
    cfg.LoadXml(xml)

    '-- determine what is currently in preview
    dim nowInPreview as String = cfg.SelectSingleNode("/vmix/preview").InnerText

    '-- only react if a new input was placed into preview
    if nowInPreview <> lastInPreview then
        lastInPreview = nowInPreview
        dim inputName as String = ""
        if debug then
            inputName = cfg.SelectSingleNode("/vmix/inputs/input[@number = '" & nowInPreview & "']/@title").Value
            Console.WriteLine("auto-pre-mix: INFO: PREVIEW change detected: input=" & inputName)
        end if

        '-- determine what is currently in program
        dim nowInProgram as String = cfg.SelectSingleNode("/vmix/active").InnerText

        '-- transitively iterate through the program input tree
        '-- and find out whether a pre-rendering Mix is used at all
        dim mix1Found as Boolean = false
        dim mix2Found as Boolean = false
        dim inputKey as String = cfg.SelectSingleNode("/vmix/inputs/input[@number = '" & nowInProgram & "']/@key").Value
        dim stack as System.Collections.Stack = new System.Collections.Stack()
        stack.Push(inputKey)
        do while stack.Count > 0 and (not mix1Found or not mix2Found)
            inputKey = stack.Pop()
            if debug then
                inputName = cfg.SelectSingleNode("/vmix/inputs/input[@key = '" & inputKey & "']/@title").Value
                Console.WriteLine("auto-pre-mix: DEBUG: crawl PROGRAM input tree: input=" & inputName)
            end if
            if inputKey = mix1InputKey then
                mix1Found = true
                exit do
            elseif inputKey = mix2InputKey then
                  mix2Found = true
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
            if mix1Found then
                Console.WriteLine("auto-pre-mix: INFO: found mix 1 usage in PROGRAM input tree: input=" & mix1InputName)
            end if
            if mix2Found then
                Console.WriteLine("auto-pre-mix: INFO: found mix 2 usage in PROGRAM input tree: input=" & mix2InputName)
            end if
        end if

        '-- transitively iterate through the Preview input tree
        '-- in order to re-configure the pre-rendering Mix which should be used
        '-- (i.e. the one which is not used in Program)
        inputKey = cfg.SelectSingleNode("/vmix/inputs/input[@number = '" & nowInPreview & "']/@key").Value
        stack = new System.Collections.Stack()
        stack.Push(inputKey)
        do while stack.Count > 0
            inputKey = stack.Pop()
            if debug then
                inputName = cfg.SelectSingleNode("/vmix/inputs/input[@key = '" & inputKey & "']/@title").Value
                Console.WriteLine("auto-pre-mix: DEBUG: crawl PREVIEW input tree: input=" & inputName)
            end if

            '-- determine input details
            dim targetNum      as String      = cfg.SelectSingleNode("/vmix/inputs/input[@key = '" & inputKey & "']/@number").Value
            dim targetOverlays as XmlNodeList = cfg.SelectNodes("/vmix/inputs/input[@key = '" & inputKey & "']/overlay")

            '-- check for the special input configuration we act on
            dim mixNumber      as String  = ""    '-- the mix number to use
            dim mixChange      as Boolean = False '-- whether to change the mix
            dim overlay1Number as Integer = -1    '-- the overlay of potential mix    input (layer number)
            dim overlay2Number as Integer = -1    '-- the overlay of potential source input (layer number)
            for i as Integer = 0 to targetOverlays.Count - 1
                dim ovKey as String = targetOverlays.Item(i).Attributes("key").InnerText
                if ovKey = mix1InputKey then
                    overlay1Number = i
                    if not mix1Found then
                        mixNumber = mix1MixNumber
                    else
                        mixNumber = mix2MixNumber
                        mixChange = true
                    end if
                elseif ovKey = mix2InputKey then
                    overlay1Number = i
                    if not mix2Found then
                        mixNumber = mix2MixNumber
                    else
                        mixNumber = mix1MixNumber
                        mixChange = true
                    end if
                elseif overlay1Number <> -1 and overlay2Number = -1 then
                    overlay2Number = i
                end if
                stack.Push(ovKey)
            next

            '-- react on special input configuration, if found
            if mixNumber <> "" and overlay1Number <> -1 and overlay2Number <> -1 then
                if debug then
                    dim overlay1Name as String = cfg.SelectSingleNode("/vmix/inputs/input[@key = '" & targetOverlays.Item(overlay1Number).Attributes("key").InnerText & "']/@title").Value
                    dim overlay2Name as String = cfg.SelectSingleNode("/vmix/inputs/input[@key = '" & targetOverlays.Item(overlay2Number).Attributes("key").InnerText & "']/@title").Value
                    Console.WriteLine("auto-pre-mix: INFO: target input " & inputName & ": found setup: layer-" & (overlay1Number + 1) & "=" & overlay1Name & " layer-" & (overlay2Number + 1) & "=" & overlay2Name)
                end if

                '-- reconfigure the pre-rendering Mix input
                dim overlayKey  as String = targetOverlays.Item(overlay2Number).Attributes("key").InnerText
                dim overlayName as String = cfg.SelectSingleNode("/vmix/inputs/input[@key = '" & targetOverlays.Item(overlay2Number).Attributes("key").InnerText & "']/@title").Value
                dim overlayNum  as String = cfg.SelectSingleNode("/vmix/inputs/input[@key = '" & targetOverlays.Item(overlay2Number).Attributes("key").InnerText & "']/@number").Value
                if debug then
                    if mixNumber = mix1MixNumber then
                        Console.WriteLine("auto-pre-mix: INFO: target input " & inputName & ": switch: mix=" & mix1InputName & " input=" & overlayName)
                    else
                        Console.WriteLine("auto-pre-mix: INFO: target input " & inputName & ": switch: mix=" & mix2InputName & " input=" & overlayName)
                    end if
                end if
                API.Function("PreviewInput", Input := overlayNum, Mix := mixNumber)
                API.Function("ActiveInput",  Input := overlayNum, Mix := mixNumber)

                '-- optionally re-configure mix layer
                if mixChange then
                    if mixNumber = mix1MixNumber then
                        if debug then
                            Console.WriteLine("auto-pre-mix: INFO: target input " & inputName & ": reconfigure: layer-" & (overlay1Number + 1) & "=" & mix1InputName)
                        end if
                        API.Function("SetMultiViewOverlay", Input := targetNum, Value := (overlay1Number + 1).toString() & "," & mix1InputNum)
                    else
                        if debug then
                            Console.WriteLine("auto-pre-mix: INFO: target input " & inputName & ": reconfigure: layer-" & (overlay1Number + 1) & "=" & mix2InputName)
                        end if
                        API.Function("SetMultiViewOverlay", Input := targetNum, Value := (overlay1Number + 1).toString() & "," & mix2InputNum)
                    end if
                end if

                '-- disable the marker overlay of the source input on the target input
                if debug then
                    Console.WriteLine("auto-pre-mix: INFO: target input " & inputName & ": reconfigure: layer-" & (overlay2Number + 1) & "=" & overlayName & " (disabled)")
                end if
                API.Function("MultiViewOverlayOff", Input := targetNum, Value := (overlay2Number + 1).toString())
             end if
        loop
    end if

    '-- wait a little bit before next iteration
    sleep(timeSlice)
loop

