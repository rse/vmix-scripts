'--
'-- event-reconfiguration.vb -- vMix script for updating event configuration
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--

'-- DESCRIPTION:
'-- This is a VB.NET 2.0 script for the vMix 4K/Pro scripting facility,
'-- allowing one to step forward/backward through an event configuration
'-- by re-configuring four NDI input sources (for shared content, one
'-- moderator P1 and two presenters P1 and P3).

'-- USAGE:
'-- configure four vMix Shortcuts with:
'-- <key1> DataSourcePreviousRow Exchange.Mapping
'-- <key1> ScriptStart RemoteShowControlOnce
'-- <key1> DataSourceNextRow Exchange.Mapping
'-- <key2> ScriptStart RemoteShowControlOnce

'-- update CONTENT input
dim C_NDI as string = Input.Find("DATASOURCE").Text("C-NDI.Text")
API.Function("NDISelectSourceByName", Input := "CONTENT", Value := C_NDI)

'-- update PRESENTER-1 input
dim P1_NDI as string = Input.Find("DATASOURCE").Text("P1-NDI.Text")
API.Function("NDISelectSourceByName", Input := "PRESENTER-1", Value := P1_NDI)

'-- update PRESENTER-2 input
dim P2_NDI as string = Input.Find("DATASOURCE").Text("P2-NDI.Text")
API.Function("NDISelectSourceByName", Input := "PRESENTER-2", Value := P2_NDI)

'-- update PRESENTER-3 input
dim P3_NDI as string = Input.Find("DATASOURCE").Text("P3-NDI.Text")
API.Function("NDISelectSourceByName", Input := "PRESENTER-3", Value := P3_NDI)

