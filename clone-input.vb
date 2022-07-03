'--
'-- clone-input.vb -- vMix script for really cloning an arbitrary input
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--
'-- Language: VB.NET 2.0 (vMix 4K/Pro flavor)
'-- Version:  0.9.0 (2022-03-04)
'--

'-- load the current API state
dim xml as string = API.XML()
dim cfg as new System.Xml.XmlDocument
cfg.LoadXml(xml)

'-- determine input currently in preview
dim inputNum as String = cfg.SelectSingleNode("/vmix/preview").InnerText
dim inputKey as String = cfg.SelectSingleNode("/vmix/inputs/input[@number = '" & inputNum & "']/@key").InnerText
Console.WriteLine("INFO: Cloning input #" & inputNum & " (" & inputKey & ")")

'-- determine current preset
dim presetNode as System.Xml.XmlNode = cfg.SelectSingleNode("/vmix/preset")
if presetNode is Nothing then
    Console.WriteLine("ERROR: You are running on a still UNSAVED vMix preset!")
    Console.WriteLine("ERROR: Please save your preset at least once, please.")
    return
end if
dim presetFile as String = presetNode.InnerText

'-- save current preset (to ensure there are no modifications lost)
API.Function("SavePreset", Value := presetFile)

'-- read preset file and parse as XML
dim utf8WithoutBOM as new System.Text.UTF8Encoding(false)
xml = System.IO.File.ReadAllText(presetFile, utf8WithoutBOM)
dim preset as new System.Xml.XmlDocument
preset.PreserveWhitespace = true
preset.LoadXml(xml)

'-- find input
dim inputNode as System.Xml.XmlNode = preset.SelectSingleNode("/XML/Input[@Key = '" & inputKey & "']")
if inputNode is Nothing then
    Console.WriteLine("ERROR: Unexpected inconsistency problem detected: Failed to locate vMix")
    Console.WriteLine("ERROR: input #" & inputNum & " (" & inputKey & ") in the underlying vMix preset file!")
    return
end if

'-- clone input
dim cloneNode as System.Xml.XmlNode = inputNode.Clone()
dim GUID As String = System.Guid.NewGuid.ToString()
cloneNode.Attributes("Key").Value = GUID
if cloneNode.Attributes("Title") is Nothing then
    dim attr as System.Xml.XmlAttribute = preset.CreateAttribute("Title")
    attr.Value = cloneNode.Attributes("OriginalTitle").Value & " (CLONED)"
    cloneNode.Attributes.SetNamedItem(attr)
else
    cloneNode.Attributes("Title").Value = cloneNode.Attributes("Title").Value & " (CLONED)"
end if
Console.WriteLine("INFO: Cloned input under new GUID " & GUID)

'-- insert cloned input
inputNode.ParentNode.insertAfter(cloneNode, inputNode)

'-- make backup of preset file
System.IO.File.Copy(presetFile, presetFile & ".bak", true)

'-- serialize XML and write preset file
System.IO.File.WriteAllText(presetFile, preset.OuterXml, utf8WithoutBOM)

'-- reopen the preset file (to activate our cloned input)
API.Function("OpenPreset", Value := presetFile)

