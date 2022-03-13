'--
'-- clone-input.vb -- vMix script for really cloning an arbitrary input
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--

'-- DESCRIPTION:
'-- This is a VB.NET 2.0 script for the vMix 4K/Pro scripting facility,
'-- allowing an arbitrary input (which has to be in the preview) to
'-- be cloned/duplicated. This is somewhat similar to the "Settings"
'-- / "Copy from..." functionality of an input (but which copies
'-- not everything) or the "Settings" / "General" / "Create Virtual
'-- Input" functionality of an input (but which still attaches to the
'-- original). Instead, this script performs a real clone of an input by
'-- directly operating on the underlying vMix preset XML file.

'-- USAGE: configure a vMix Shortcut with:
'-- <key> ScriptStart clone-input

'-- load the current API state
dim xml as string = API.XML()
dim cfg as new System.Xml.XmlDocument
cfg.LoadXml(xml)

'-- determine input currently in preview
dim inputNum as String = cfg.SelectSingleNode("/vmix/preview").InnerText
dim inputKey as String = cfg.SelectSingleNode("/vmix/inputs/input[@number = '" & inputNum & "']/@key").InnerText
console.writeline("cloning input #" & inputNum & " (" & inputKey & ")")

'-- determine current preset
dim presetFile as String = cfg.SelectSingleNode("/vmix/preset").InnerText

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

'-- clone input
dim cloneNode as System.Xml.XmlNode = inputNode.Clone()
dim GUID As String = System.Guid.NewGuid.ToString()
cloneNode.Attributes("Key").Value = GUID
if cloneNode.Attributes("Title") is Nothing then
    dim attr as XmlAttribute = preset.CreateAttribute("Title")
    attr.Value = cloneNode.Attributes("OriginalTitle").Value & " (CLONED)"
    cloneNode.Attributes.SetNamedItem(attr)
else
    cloneNode.Attributes("Title").Value = cloneNode.Attributes("Title").Value & " (CLONED)"
end if
console.writeline("cloned input under new GUID " & GUID)

'-- insert cloned input
inputNode.ParentNode.insertAfter(cloneNode, inputNode)

'-- make backup of preset file
System.IO.File.Copy(presetFile, presetFile & ".bak", true)

'-- serialize XML and write preset file
System.IO.File.WriteAllText(presetFile, preset.OuterXml, utf8WithoutBOM)

'-- reopen the preset file (to activate our cloned input)
API.Function("OpenPreset", Value := presetFile)

