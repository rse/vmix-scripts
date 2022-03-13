'--
'-- stage-gate.vb -- vMix script for gating stage audio
'-- Copyright (c) 2022 Dr. Ralf S. Engelschall <rse@engelschall.com>
'-- Distributed under MIT license <https://spdx.org/licenses/MIT.html>
'--

'-- DESCRIPTION:
'-- This is a VB.NET 2.0 script for the vMix 4K/Pro scripting facility,
'-- allowing the stage inputs (monitored on Bus-A) to be temporarily
'-- "dimmed" as long as stage outputs (monitored on Bus-B) happen. This
'-- is somewhat similar to a sidechain-based noise-gate audio filter.
'-- This way one can prevent nasty echos or even full loops on stage.

'-- CONFIGURATION (can be adjusted)

dim thresholdDB       as integer = -30   'The volume (dB) Stage speakers must be above to trigger
dim reducedDB         as integer = -54   'The volume (dB) Stage microphones are temporarily reduced to
dim fadeDown          as integer = 60    'The time (ms) for fading down Stage microphones (when above threshold)
dim fadeUp            as integer = 400   'The time (ms) for fading up   Stage microphones (when below threshold)

dim checkingIter      as integer = 10    'The number of iterations of checking the silence before triggering
dim checkingIterTime  as integer = 20    'The interval (milliseconds) between the checking iterations
dim checkingCount     as integer = 0

'-- pre-convert decibel to audio fader volume
dim reducedAmp as double  = 10 ^ (reducedDB / 20)
dim reducedVol as integer = cint((reducedAmp ^ 0.25) * 100)

'-- prepare XML DOM tree
dim cfg as new System.Xml.XmlDocument

'-- enter endless iteration loop
do while true
    '-- fetch current vMix API status
    dim xml as string = API.XML()
    cfg.LoadXml(xml)

    '-- determine metering of all inputs on Bus-B (Stage-OUT)
    dim muted  as boolean = (cfg.SelectSingleNode("//audio/busB/@muted").Value)
    dim meter1 as double  = (cfg.SelectSingleNode("//audio/busB/@meterF1").Value)
    dim meter2 as double  = (cfg.SelectSingleNode("//audio/busB/@meterF2").Value)

    '-- convert meter from amplitude to decibel
    if meter1 < meter2 then
        meter1 = meter2
    end if
    dim meterDB as integer = cint(20 * Math.log10(meter1))

    '-- dispatch according to current meter level and mute state
    if meterDB > thresholdDB and not muted then
        '-- Stage speakers are above threshold
        if checkingCount >= checkingIter then
            '-- ducking of all inputs marked with Bus-A (Stage-IN)
            dim busInputs as XmlNodeList = cfg.SelectNodes("//inputs/input[@audiobusses = 'M,A']")
            for each busInput as XmlNode in busInputs
                dim num     as integer = Convert.ToInt32(busInput.Attributes("number").InnerText)
                dim isMuted as boolean = Convert.ToBoolean(busInput.Attributes("muted").InnerText)
                if not isMuted then
                    '-- fade to reduced volume
                    Input.Find(num).Function("SetVolumeFade", reducedVol.tostring & "," & fadeDown.tostring)
                end if
            next busInput
            checkingCount = 0
        end if
    else
        '-- Stage speakers are below threshold
        if checkingCount < checkingIter then
            checkingCount += 1
        elseif checkingCount = checkingIter then
            '-- unducking of all inputs marked with Bus-A (Stage-IN)
            dim busInputs as XmlNodeList = cfg.SelectNodes("//inputs/input[@audiobusses = 'M,A']")
            for each busInput as XmlNode in busInputs
                dim num     as integer = Convert.ToInt32(busInput.Attributes("number").InnerText)
                dim isMuted as boolean = Convert.ToBoolean(busInput.Attributes("muted").InnerText)
                if not isMuted then
                    '-- fade back to original volume
                    Input.Find(num).Function("SetVolumeFade", "100" & "," & fadeUp.tostring)
                end if
            next busInput
            checkingCount += 1
        end if
    end if

    '-- wait until next check
    sleep(checkingIterTime)
loop

