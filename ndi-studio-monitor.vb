'--
'-- ndi-studio-monitor.vb -- vMix script for NDI Studio Monitor stream configuration
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--

'-- DESCRIPTION:
'-- This is a VB.NET 2.0 script for the vMix 4K/Pro scripting facility,
'-- allowing vMix to re-configure the NDI source displayed in an NDI
'-- Studio Monitor instance.

'-- USAGE: configure a vMix Shortcut with:
'-- <key> SetDynamicValue1 <ip-address>:<port>
'-- <key> SetDynamicValue1 <ndi-source-name>
'-- <key> ScriptStart ndi-studio-monitor

'-- fetch current vMix API status
dim xml as string = API.XML()
dim cfg as new System.Xml.XmlDocument
cfg.LoadXml(xml)

'-- determine parameters
dim monitorIP      as String = cfg.selectSingleNode("//dynamic/value1").InnerText
dim monitorSource  as String = cfg.selectSingleNode("//dynamic/value2").InnerText

'-- prepare re-configuration URL
dim monitorURL     as String = "http://" + monitorIP + "/v1/configuration"

'-- prepare re-configuration JSON payload
dim utf8WithoutBOM as new System.Text.UTF8Encoding(false)
dim payloadJSON    as String = "{""version"":1,""NDI_source"":""" + monitorSource + """}"
dim payloadBytes   as Byte() = utf8WithoutBOM.GetBytes(payloadJSON)

'-- initiate the HTTP POST request to NDI Studio Monitor
dim request as HttpWebRequest = HttpWebRequest.Create(monitorURL)
request.Method        = "POST"
request.ContentType   = "application/x-www-form-urlencoded"
request.ContentLength = payloadBytes.Length
dim stream as Stream = request.GetRequestStream()
stream.Write(payloadBytes, 0, payloadBytes.Length)
stream.Close()

