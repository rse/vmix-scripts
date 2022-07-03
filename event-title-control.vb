'--
'-- event-title-control.vb -- vMix script for animating title
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--
'-- Language: VB.NET 2.0 (vMix 4K/Pro flavor)
'-- Version:  1.0.0 (2022-03-23)
'--

'-- CONFIGURATION
dim durationVisible as Integer = 10  '-- seconds to wait between title in and out
dim durationLocked  as Integer = 900 '-- seconds to wait until title can be shown again

'-- load the current API state
dim cfg as new System.Xml.XmlDocument
dim xml as String = API.XML()
cfg.LoadXml(xml)

'-- determine parameter
dim titleNames() as String = cfg.SelectSingleNode("//dynamic/value1").InnerText.Split(",")

'-- determine current time
dim now as Integer = (DateTime.Now - #1/1/1970#).TotalSeconds

'-- iterate over all titles
dim titleTransitioned as String = ""
for each titleName as String in titleNames
    '-- determine currently selected transition
    dim selectedIndex as Integer = Convert.ToInt32(cfg.SelectSingleNode("//inputs/input[@title = '" & titleName & "']/@selectedIndex").Value)
    if selectedIndex <> 1 then
        '-- if transition IN was still not done, we might do it...
        dim state as String = Input.Find(titleName).Text("LastTransition.Text")
        dim time as Integer = 0
        if state <> "" then
            time = Convert.ToInt32(state)
        end if
        if (time + (durationVisible + durationLocked)) <= now then
            '-- ...but only at maximum every durationLocked seconds
            Input.Find(titleName).Function("SetText", SelectedName := "LastTransition.Text", Value := now.ToString())
            Input.Find(titleName).Function("SelectIndex", Value := 1)

            '-- remember to transition OUT the title again afterwards
            if titleTransitioned <> "" then
                titleTransitioned = titleTransitioned & ","
            end if
            titleTransitioned = titleTransitioned & titleName
        end if
    end if
next

'-- in case of any transitions IN, wait and transition OUT again
if titleTransitioned <> "" then
    '-- first let the title be visible for the configured amount of time
    Sleep(durationVisible * 1000)

    '-- reload the current API state
    xml = API.XML()
    cfg.LoadXml(xml)

    '-- iterate over all remembered titles again
    dim titleNames2() as String = titleTransitioned.Split(",")
    for each titleName2 as String in titleNames2
        '-- determine currently selected transition once again
        dim selectedIndex2 as Integer = Convert.ToInt32(cfg.SelectSingleNode("//inputs/input[@title = '" & titleName2 & "']/@selectedIndex").Value)
        if selectedIndex2 = 1 then
            '-- if transition IN was really done (and not something changed
            '-- between our operations as a side-effect), we transition OUT again
            Input.Find(titleName2).Function("SelectIndex", Value := 2)
        end if
    next
end if

