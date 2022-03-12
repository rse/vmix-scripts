'--
'-- event-reconfiguration.vb -- vMix script for updating event configuration
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--

'-- DESCRIPTION:
'-- This is a VB.NET 2.0 script for the vMix 4K/Pro scripting facility,
'-- allowing one to step forward/backward through an event configuration
'-- by re-configuring four NDI input sources (for shared content, one
'-- moderator P1 and two presenters P2 and P3).

'-- USAGE:
'-- configure four vMix Shortcuts with:
'-- <key1> DataSourcePreviousRow <datasource>.<worksheet>
'-- <key1> ScriptStart RemoteShowControlOnce
'-- <key1> DataSourceNextRow <datasource>.<worksheet>
'-- <key2> ScriptStart RemoteShowControlOnce

'-- CONFIGURATION
dim titleSource = "DATASOURCE"
dim fieldInputMapping as new Dictionary(of String, String)
fieldInputMapping.Add("C-NDI",  "CONTENT")
fieldInputMapping.Add("P1-NDI", "PRESENTER-1")
fieldInputMapping.Add("P2-NDI", "PRESENTER-2")
fieldInputMapping.Add("P3-NDI", "PRESENTER-3")

'-- update the NDI inputs
for each titleField as String in fieldInputMapping.Keys
    dim vmixInput as String = fieldInputMapping(titleField)
    dim ndiStream as string = Input.Find(titleSource).Text(titleField & ".Text")
    API.Function("NDISelectSourceByName", Input := vmixInput, Value := ndiStream)
next

