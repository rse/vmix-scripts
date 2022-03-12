'--
'-- remoteshowcontrol-once.vb -- vMix script for RemoteShowControl one-time commands
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--

'-- DESCRIPTION:
'-- This is a VB.NET 2.0 script for the vMix 4K/Pro scripting facility,
'-- allowing one to send commands to the Irisdown Remote Show
'-- Control (https://www.irisdown.co.uk/rsc.html) plugin of PowerPoint.
'-- This allows a vMix operator to step forward/backward through an
'-- ingested (screen or HDMI capturing) PowerPoint presentation.

'-- USAGE: configure four vMix Shortcuts with:
'-- <key1> SetDynamicValue1 PREV
'-- <key1> ScriptStart RemoteShowControlOnce
'-- <key2> SetDynamicValue1 NEXT
'-- <key2> ScriptStart RemoteShowControlOnce

'-- CONFIGURATION
dim clientIP   as String = "10.1.0.15"
dim clientPort as String = "61001"

'-- load the current API state
dim xml as string = API.XML()
dim x as new System.Xml.XmlDocument
x.loadxml(xml)
dim cmd as string = (x.SelectSingleNode("//dynamic/value1").InnerText)
console.writeline(cmd)

'-- connect to RemoteShowControl and send command
dim client as new System.Net.Sockets.TcpClient(clientIP, clientPort)
client.SendTimeout    = 1000
client.ReceiveTimeout = 1000
dim data as [Byte]() = System.Text.Encoding.ASCII.GetBytes(cmd)
dim stream as System.Net.Sockets.NetworkStream = client.GetStream()
stream.Write(data, 0, data.Length)
stream.Close()
client.Close()

