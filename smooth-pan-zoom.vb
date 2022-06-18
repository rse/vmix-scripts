'--
'-- smooth-pan-zoom.vb -- vMix script for smooth adjusting pan/zoom of input
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--

'-- DESCRIPTION:
'-- This is a VB.NET 2.0 script for the vMix 4K/Pro scripting facility,
'-- which allows one to smoothly adjust the pan/zoom of an input. This
'-- is usually used for a rough emulation of the Virtual PTZ feature,
'-- which cannot be used on layered inputs like Virtual Sets.

'-- USAGE: configure a vMix Shortcut with:
'-- <keyX> SetDynamicInput4 <input-name>
'-- <keyX> SetDynamicValue4 {pan:{up-left|up|up-right|left|reset|right|down-left|down|down-right}|zoom:{increase|reset|decrease}}
'-- <keyX> ScriptStart      smooth-pan-zoom

'-- USAGE: alternatively, for splitting between input selection and operation, configure vMix Shortcuts with:
'-- <keyA> SetDynamicInput4 <input-name>
'-- <keyX> SetDynamicValue4 {pan:{up-left|up|up-right|left|reset|right|down-left|down|down-right}|zoom:{increase|reset|decrease}}
'-- <keyX> ScriptStart      smooth-pan-zoom

'-- script configuration
dim timeSlice  as integer = 33    '-- (= 1000ms/30fps)
dim duration   as integer = 660   '-- (= multiple of timeSlice)
dim deltaPan   as double  = 0.10  '-- (= 10%)
dim deltaZoom  as double  = 0.10  '-- (= 10%)

'-- load the current API state
dim xml as string = API.XML()
dim cfg as new System.Xml.XmlDocument
cfg.LoadXml(xml)

'-- determine parameters
dim inputName as string = cfg.SelectSingleNode("//dynamic/input4").InnerText
dim params()  as string = cfg.SelectSingleNode("//dynamic/value4").InnerText.Split(":")
dim opName    as string = params(0)
dim dirName   as string = params(1)

'-- determine operation function(s)
dim func1  as string = ""
dim func2  as string = ""
dim value1 as string = ""
dim value2 as string = ""
dim delta  as double = 0
if opName = "pan" then
    delta = deltaPan
    if dirName = "up-left" then
        func1  = "SetPanY"
        func2  = "SetPanX"
        value1 = "-="
        value2 = "+="
    else if dirName = "up" then
        func1  = "SetPanY"
        value1 = "-="
    else if dirName = "up-right" then
        func1  = "SetPanY"
        func2  = "SetPanX"
        value1 = "-="
        value2 = "-="
    else if dirName = "left" then
        func1  = "SetPanX"
        value1 = "+="
    else if dirName = "reset" then
        func1  = "SetPanY"
        func2  = "SetPanX"
    else if dirName = "right" then
        func1  = "SetPanX"
        value1 = "-="
    else if dirName = "down-left" then
        func1  = "SetPanY"
        func2  = "SetPanX"
        value1 = "+="
        value2 = "+="
    else if dirName = "down" then
        func1  = "SetPanY"
        value1 = "+="
    else if dirName = "down-right" then
        func1  = "SetPanY"
        func2  = "SetPanX"
        value1 = "+="
        value2 = "-="
    end if
else if opName = "zoom" then
    delta = deltaZoom
    func1 = "SetZoom"
    if dirName = "decrease" then
        value1 = "-="
    else if dirName = "increase" then
        value1 = "+="
    end if
end if

'-- determine operation value(s)
dim timeSteps  as integer = duration / timeSlice
dim valueSlice as double  = delta    / timeSteps
if value1 <> "" then
    value1 = value1 & valueSlice
else
    value1 = "0"
end if
if func2 <> "" then
    if value2 <> "" then
        value2 = value2 & valueSlice
    else
        value2 = "0"
    end if
end if

'-- apply operation in a smooth way
do while timeSteps > 0
    timeSteps = timeSteps - 1

    '-- perform single operation step
    API.Function(func1, Input := inputName, Value := value1)
    if func2 <> "" then
        API.Function(func2, Input := inputName, Value := value2)
    end if

    '-- wait until next iteration
    sleep(timeSlice)
loop

