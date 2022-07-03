
Audio-Sidechain
===============

**Audio Sidechain Compression**

Allow audio output volumes to be automatically and temporarily
reduced, based on audio input volumes (when temporarily above a
threshold) -- similar to an audio side-chain compression.

Problem
-------

In practice there are two challenges in vMix when it comes to audio:

1. MICROPHONE DUCKING:
   Allow input devices (microphones, attached to the Master bus plus the
   "marker" bus Bus-A, but individually controlled) to be temporarily
   "ducked" (volume reduced) as long as output devices (callers and
   media, monitored on Bus-B) are active. This prevents nasty echos or
   even full loops.

2. TRANSLATOR VOICE-OVER:
   Allow one or more people (usually remote translators, sitting
   on vMix Call inputs and mixed on the Master audio bus and
   additionally monitored on Bus-C) to speak over the program
   (usually received via NDI and mixed on the Master audio bus
   after being "dimmed" on Bus-B).

Solution
--------

The solution to both challenges is to place those audio inputs onto a
monitoring audio bus which should be observed (similar to the side-chain
of a compressor) and the audio inputs which should be adjusted onto
another audio bus (similar to the regular input of a compressor).

Usage
-----

The recommended configurations are:

1. MICROPHONE DUCKING:

       busMonitor        = "B"   (Notice: callers and media)
       busAdjust         = "A"   (Notice: microphones)
       busAdjustInputs   = true  (Notice: adjust the inputs)
       volumeFullDB      = 0
       volumeReducedDB   = -55   (Notice: pull down volume to about 20%)
       volumeThresholdDB = -32
       timeSlice         = 10
       timeAwaitOver     = 20
       timeAwaitBelow    = 250
       timeFadeDown      = 50
       timeFadeUp        = 250

2. TRANSLATOR VOICE-OVER:

       busMonitor        = "C"   (Notice: translators)
       busAdjust         = "B"   (Notice: program)
       busAdjustInputs   = false (Notice: adjust the bus)
       volumeFullDB      = 0
       volumeReducedDB   = -24   (Notice: pull down volume to about 50%)
       volumeThresholdDB = -32
       timeSlice         = 10
       timeAwaitOver     = 20
       timeAwaitBelow    = 1500  (Notice: allow translators to breathe)
       timeFadeDown      = 50
       timeFadeUp        = 500   (Notice: fade in program slowly)

The audio volume science is a little bit hard to understand and vMix
in addition also makes it even more complicated by using difference
scales. Here is some background on the above Decibel (dB) based
scales, the formulars how the scales can be converted and some
examples:

    Scales:
        Volume:      0 to 100    (used for UI volume bars, SetVolumeFade)
        Amplitude:   0 to 1      (used for API audio bus meter, @meterF1)
        Amplitude2:  0 to 100    (used for API input volume input, @volume)
        Decibels:    -oo to 0    (used in audio science)

    Formulas:
        Amplitude    = Amplitude2 / 100
        Amplitude2   = Amplitude * 100
        Volume       = (Amplitude ^ 0.25) * 100
        Amplitude    = (Volume / 100) ^ 4
        Decibels     = 20 * Math.Log10(Amplitude)
        Amplitude    = 10 ^ (Decibels / 20)

    Examples:
        Volume: 100, Amplitude: 1.0000, Decibel:  0
        Volume:  97, Amplitude: 0.8913, Decibel: -1  (clipping border)
        Volume:  92, Amplitude: 0.7079, Decibel: -3
        Volume:  84, Amplitude: 0.5012, Decibel: -6
        Volume:  77, Amplitude: 0.3548, Decibel: -9
        Volume:  71, Amplitude: 0.2512, Decibel: -12
        Volume:  63, Amplitude: 0.1585, Decibel: -16 (broadcast standard)
        Volume:  50, Amplitude: 0.0631, Decibel: -23 (film standard)
        Volume:  40, Amplitude: 0.0224, Decibel: -32 (voice gate border)
        Volume:  27, Amplitude: 0.0056, Decibel: -45
        Volume:  21, Amplitude: 0.0020, Decibel: -54 (very much dimmed)
        Volume:  15, Amplitude: 0.0006, Decibel: -65 (silence border)
        Volume:  10, Amplitude: 0.0001, Decibel: -80
        Volume:   0, Amplitude: 0.0000, Decibel: -Infinity

