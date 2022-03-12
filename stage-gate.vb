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

dim reduced           as integer = 20        'The reduced volume of Stage microphones, from 0 to 100 (linear scale)
dim threshold         as double  = 0.05      'The threshold of volume of the Cloud speakers, from 0 to 1 (log scale)
                                             '0.5=50%~=-6dB, 0.1=10%~=-20dB...
dim fadeDown          as integer = 60        'The time for fading down
dim fadeUp            as integer = 400       'The time for fading up

dim checkingIter      as integer = 10        'The number of iterations of checking the interpreter silence before triggering
dim checkingIterTime  as integer = 20        'The interval (milliseconds) between the checking iterations
dim checkingCount     as integer = 0

'-- enter endless iteration loop
do while true
    '-- fetch current vMix API status
    dim xml as string = API.XML()
    dim x as new system.xml.xmldocument
    x.loadxml(xml)

    '-- determine metering of all inputs on Bus-B (Stage-OUT)
    dim muted  as boolean = (x.SelectSingleNode("//audio/busB/@muted").Value)
    dim meter1 as double  = (x.SelectSingleNode("//audio/busB/@meterF1").Value)
    dim meter2 as double  = (x.SelectSingleNode("//audio/busB/@meterF2").Value)
    if meter1 < meter2
        meter1 = meter2
    end if

    '-- xx
    if meter1 > threshold and not muted
        if checkingCount >= checkingIter
            checkingCount = 0
            '-- ducking of all inputs marked with Bus-A (Stage-IN)
            dim busInputs as XmlNodeList = x.SelectNodes("//inputs/input[@audiobusses = 'M,A']")
            for each busInput as XmlNode in busInputs
                dim num as integer = Convert.ToInt32(busInput.Attributes("number").InnerText)
                dim isMuted as boolean = Convert.ToBoolean(busInput.Attributes("muted").InnerText)
                if not isMuted
                    Input.Find(num).Function("SetVolumeFade", reduced.tostring + "," + fadeDown.tostring)
                end if
            next busInput
        end if
    else
        if checkingCount < checkingIter
            checkingCount += 1
        end if
        if checkingCount = checkingIter
            '-- unducking of all inputs marked with Bus-A (Stage-IN)
            dim busInputs as XmlNodeList = x.SelectNodes("//inputs/input[@audiobusses = 'M,A']")
            for each busInput as XmlNode in busInputs
                dim num as integer = Convert.ToInt32(busInput.Attributes("number").InnerText)
                dim isMuted as boolean = Convert.ToBoolean(busInput.Attributes("muted").InnerText)
                if not isMuted
                    Input.Find(num).Function("SetVolumeFade", "100" + "," + fadeUp.tostring)
                end if
            next busInput
            checkingCount += 1
        end if
    end if

    '-- wait until next check
    sleep(checkingIterTime)
loop

