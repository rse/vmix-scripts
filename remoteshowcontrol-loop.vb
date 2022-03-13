'--
'-- remoteshowcontrol-loop.vb -- vMix script for RemoteShowControl following inputs
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--

'-- DESCRIPTION:
'-- This is a VB.NET 2.0 script for the vMix 4K/Pro scripting facility,
'-- allowing one to attach particular slides an ingested (screen or
'-- HDMI capturing) PowerPoint presentation to a vMix input. This works
'-- by observing which input is in preview and if its title contains
'-- "[rsc:N]" this script instructs PowerPoint, through the Irisdown
'-- Remote Show Control (https://www.irisdown.co.uk/rsc.html) plugin, to
'-- go to the particular slide N.

'-- CONFIGURATION
dim clientIP   as String = "127.0.0.1"
dim clientPort as String = "61001"

'-- keep internal state
dim lastInPreview   as String = ""
dim lastSlideSelect as String = ""

'-- prepare XML DOM tree
dim cfg as new System.Xml.XmlDocument

'-- endless loop
do while true
    '-- load the current API state
    dim xml as string = API.XML()
    cfg.LoadXml(xml)

    '-- only react if a new input was placed into the preview
    dim nowInPreview as String = cfg.SelectSingleNode("//preview").InnerText
    if nowInPreview <> lastInPreview then
        lastInPreview = nowInPreview

        '-- only react if input title contains "[rsc:N]"
        dim title as string = (cfg.SelectSingleNode("//inputs/input[@number = '" & nowInPreview & "']/@title").Value)
        dim titleMatches as Boolean = (title like "*[rsc:#]*") orElse (title like "*[rsc:##]*")
        if titleMatches then
            '-- only react if slide number has to change
            dim t as String = title.substring(title.indexOf("[rsc:") + 5)
            dim nowSlideSelect as String = t.substring(0, t.indexOf("]"))
            if nowSlideSelect <> lastSlideSelect then
                lastSlideSelect = nowSlideSelect

                '-- connect to RemoteShowControl and select particular slide number
                Console.WriteLine("select slide #" & nowSlideSelect & " for input #" & nowInPreview)
                dim client as new System.Net.Sockets.TcpClient(clientIP, clientPort)
                client.SendTimeout    = 1000
                client.ReceiveTimeout = 1000
                dim data as [Byte]() = System.Text.Encoding.ASCII.GetBytes("GO " & nowSlideSelect)
                dim stream as System.Net.Sockets.NetworkStream = client.GetStream()
                stream.Write(data, 0, data.Length)
                stream.Close()
                client.Close()
            end if
        end if
    end if

    '-- wait a little bit before next iteration
    sleep(200)
loop

