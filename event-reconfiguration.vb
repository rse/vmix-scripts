'--
'-- event-reconfiguration.vb -- vMix script for updating event configuration
'-- Copyright (c) 2022-2023 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--
'-- Language: VB.NET 2.0 (vMix 4K/Pro flavor)
'-- Version:  1.0.1 (2022-04-01)
'--

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

