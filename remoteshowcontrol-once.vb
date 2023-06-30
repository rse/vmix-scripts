'--
'-- remoteshowcontrol-once.vb -- vMix script for RemoteShowControl one-time commands
'-- Copyright (c) 2022-2023 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--
'-- Language: VB.NET 2.0 (vMix 4K/Pro flavor)
'-- Version:  1.0.0 (2022-03-13)
'--

'-- CONFIGURATION
dim clientIP   as String = "10.1.0.15"
dim clientPort as String = "61001"

'-- load the current API state
dim xml as string = API.XML()
dim cfg as new System.Xml.XmlDocument
cfg.LoadXml(xml)

'-- determine command parameter
dim cmd as string = cfg.SelectSingleNode("//dynamic/value1").InnerText
if not (cmd = "PREV" or cmd = "NEXT") then
    cmd = "GO " & cmd
end if

'-- connect to RemoteShowControl and send command
dim client as new System.Net.Sockets.TcpClient(clientIP, clientPort)
client.SendTimeout    = 1000
client.ReceiveTimeout = 1000
dim data as [Byte]() = System.Text.Encoding.ASCII.GetBytes(cmd)
dim stream as System.Net.Sockets.NetworkStream = client.GetStream()
stream.Write(data, 0, data.Length)
stream.Close()
client.Close()

