
[Audio-Sidechain](audio-sidechain.vb)
=====================================

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

       busMonitor           = "B"   (Notice: callers and media)
       busAdjust            = "A"   (Notice: microphones)
       busAdjustInputs      = true  (Notice: adjust the inputs)
       busAdjustInputsExcl  = ""
       busAdjustUnmutedOnly = false
       volumeThreshold      = -42   (Notice: -42 dB FS)
       volumeFull           = 100   (Notice: 100%)
       volumeReduced        = 30    (Notice: 30% = reduce by -42 dB)
       timeSlice            = 10
       timeAwaitOver        = 20
       timeAwaitBelow       = 150
       timeFadeDown         = 50
       timeFadeUp           = 50

2. TRANSLATOR VOICE-OVER:

       busMonitor           = "C"   (Notice: translators)
       busAdjust            = "B"   (Notice: program)
       busAdjustInputs      = false (Notice: adjust the bus)
       busAdjustInputsExcl  = ""
       busAdjustUnmutedOnly = false
       volumeThreshold      = -42   (Notice: -42 dB FS)
       volumeFull           = 100   (Notice: 100%)
       volumeReduced        = 60    (Notice: 60% = reduce by -18 dB)
       timeSlice            = 10
       timeAwaitOver        = 10
       timeAwaitBelow       = 1500  (Notice: allow translators to breathe)
       timeFadeDown         = 10
       timeFadeUp           = 400   (Notice: fade in program slowly)

The audio volume science is a little bit hard to understand and vMix
in addition also makes it even more complicated by using different
scales. Here is some background on the above Decibel (dB) based
scales, the formulas how the scales can be converted, and some
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
        ==== METER (measured) ===============      ==== CONTROL =============
        Volume (%) Amplitude  Decibel (dB FS)      Volume (%) Decibel (dB FS) *
        ---------- ---------- ---------------      ---------- ---------------
        100        1,0000        0,00              100        - 18,00
         95        0,8145     -  1,78               95        - 19,78
         90        0,6561     -  3,66               90        - 21,66
         85        0,5220     -  5,65               85        - 23,65
         80        0,4096     -  7,75               80        - 25,75
         75        0,3164     - 10,00               75        - 28,00
         70        0,2401     - 12,39               70        - 30,39
         65        0,1785     - 14,97               65        - 32,97
         60        0,1296     - 17,75               60        - 35,75
         55        0,0915     - 20,77               55        - 38,77
         50        0,0625     - 24,08               50        - 42,08
         45        0,0410     - 27,74               45        - 45,74
         40        0,0256     - 31,84               40        - 49,84
         35        0,0150     - 36,47               35        - 54,47
         30        0,0081     - 41,83               30        - 59,83
         25        0,0039     - 48,16               25        - 66,16
         20        0,0016     - 55,92               20        - 73,92
         15        0,0005     - 65,91               15        - 83,91
         10        0,0001     - 80,00               10        - 98,00
          5        0,0000     -104,08                5        -122,08
          1        0,0000     -160,00                1        -178,08
          0        0,0000     -    oo                0        -    oo

      (*) for a usual incoming voice signal of -18 dB FS

