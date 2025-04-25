using System;

namespace DoTheSFXAndMusic
{
    public enum WaveForm
    {
        Select,     // 0
        
        Pulse,      // 1
        Sawtooth,   // 2
        Sine,       // 3
        Triangle,   // 4
        Noise,      // 5
        
        Custom = 12
    }
    
    public enum FX
    {
        None,
        FadeIn, // 1
        FadeOut, // 2
        Vibrato, // 3
        Slide, // 4
        Drop, // 5
        ARP, // 6
    }

    public static class WaveFormEnumConstants
    {   
        public static WaveForm LastShape => WaveForm.Noise;
    }
    
    [Serializable]
    public struct SoundData
    {
        public int WaveForm;
        public int Pitch;
        public int Volume; //0 = none, 1 - k_volumeLevels
        public FX Effect;
    }
}