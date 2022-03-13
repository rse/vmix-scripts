'--
'-- event-reconfiguration.vb -- vMix script for updating event configuration
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--

'-- DESCRIPTION:
'-- This is a VB.NET 2.0 script for the vMix 4K/Pro scripting facility,
'-- allowing one to step forward/backward through an event configuration
'-- by re-configuring four NDI input sources (for shared content, one
'-- moderator P1 and two presenters P2 and P3). The crux is the use
'-- of a full-screen GT title input which holds the four cells of the
'-- underlying Excel data source.

'-- USAGE:
'-- configure two vMix Shortcuts with:
'-- <key1> SetDynamicValue1 PREV
'-- <key1> ScriptStart event-reconfiguration
'-- <key2> SetDynamicValue1 NEXT
'-- <key2> ScriptStart event-reconfiguration

'-- CONFIGURATION
dim dataSource  = "Exchange.Mapping"
dim titleSource = "DATASOURCE"
dim fieldInputMapping as new Dictionary(of String, String)
fieldInputMapping.Add("C-NDI",  "CONTENT")
fieldInputMapping.Add("P1-NDI", "PRESENTER-1")
fieldInputMapping.Add("P2-NDI", "PRESENTER-2")
fieldInputMapping.Add("P3-NDI", "PRESENTER-3")

'-- load the current API state
dim xml as string = API.XML()
dim cfg as new System.Xml.XmlDocument
cfg.LoadXml(xml)

'-- determine operation parameter
dim op as string = cfg.SelectSingleNode("//dynamic/value1").InnerText

'-- change row in data source
if op = "PREV" then
    API.Function("DataSourcePreviousRow", Value := dataSource)
elseif op = "NEXT" then
    API.Function("DataSourceNextRow", Value := dataSource)
end if

'-- update the NDI inputs
for each titleField as String in fieldInputMapping.Keys
    dim vmixInput as String = fieldInputMapping(titleField)
    dim ndiStream as string = Input.Find(titleSource).Text(titleField & ".Text")
    API.Function("NDISelectSourceByName", Input := vmixInput, Value := ndiStream)
next

