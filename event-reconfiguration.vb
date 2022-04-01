'--
'-- event-reconfiguration.vb -- vMix script for updating event configuration
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--

'-- DESCRIPTION:
'-- This is a VB.NET 2.0 script for the vMix 4K/Pro scripting facility,
'-- allowing one to step forward/backward through (or to a particular
'-- row of) an event configuration by re-configuring four NDI input
'-- sources (for shared content, one moderator P1 and two presenters P2
'-- and P3). The crux is the use of a full-screen GT title input which
'-- holds the four cells of the underlying Excel data source.

'-- USAGE:
'-- configure two vMix Shortcuts with:
'-- <key1> SetDynamicValue1 PREV
'-- <key1> SetDynamicValue2 <path-to-xml-file>
'-- <key1> ScriptStart event-reconfiguration
'-- <key2> SetDynamicValue1 NEXT
'-- <key2> SetDynamicValue2 <path-to-xml-file>
'-- <key2> ScriptStart event-reconfiguration
'-- <key3> SetDynamicValue1 42
'-- <key3> SetDynamicValue2 <path-to-xml-file>
'-- <key3> ScriptStart event-reconfiguration

'-- load the current API state
dim xml as string = API.XML()
dim cfg as new System.Xml.XmlDocument
cfg.LoadXml(xml)

'-- determine parameter
dim opName      as string = cfg.SelectSingleNode("//dynamic/value1").InnerText
dim settingFile as string = cfg.SelectSingleNode("//dynamic/value2").InnerText

'-- read configuration file
dim utf8WithoutBOM as new System.Text.UTF8Encoding(false)
xml = System.IO.File.ReadAllText(settingFile, utf8WithoutBOM)
dim setting as new System.Xml.XmlDocument
setting.LoadXml(xml)

'-- parse configuration information
dim dataSource  as String      = setting.SelectSingleNode("/event/data-source/@name").Value
dim titleSource as String      = setting.SelectSingleNode("/event/title-source/@name").Value
dim ndiMappings as XmlNodeList = setting.SelectNodes("/event/ndi-mapping")

'-- change row in data source
if opName = "PREV" then
    API.Function("DataSourcePreviousRow", Value := dataSource)
elseif opName = "NEXT" then
    API.Function("DataSourceNextRow", Value := dataSource)
else
    API.Function("DataSourceSelectRow", Value := dataSource & "," & opName)
end if

'-- reset the title control states
dim titleNodes as XmlNodeList = cfg.SelectNodes("//inputs/input[@key and @type = 'GT' and text[@name = 'LastTransition.Text']]")
for each titleNode as XmlNode in titleNodes
    dim title as String = titleNode.Attributes("title").Value
    Input.Find(title).Function("SetText", SelectedName := "LastTransition.Text", Value := "0")
next

'-- give vMix some time to update the title input
sleep(100)

'-- update the NDI inputs
for each ndiMapping as XmlNode in ndiMappings
    dim fieldName as String = ndiMapping.Attributes("field-name").Value
    dim inputName as String = ndiMapping.Attributes("input-name").Value
    dim ndiStream as string = Input.Find(titleSource).Text(fieldName & ".Text")
    API.Function("NDISelectSourceByName", Input := inputName, Value := ndiStream)
next

