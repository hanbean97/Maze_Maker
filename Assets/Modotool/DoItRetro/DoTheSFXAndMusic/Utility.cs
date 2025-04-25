using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DoTheSFXAndMusic
{
    public static class Utility
    {
        public const float C0 = 16.35160f; // C 0 frequency
        public const int VolumeLevels = 7;

        public const int SampleRate = 44100;
        // duration of 1 is one quarter of a second so we use this 
        public const int QuarterSampleRate = 11025;

        public const int PitchValues = 96;
        public const int SoundDataMaxLength = 32;

        public static readonly Color InactiveColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        public static readonly Color BGColorA = new Color(0.05f, 0.05f, 0.06f, 1f);
        public static readonly Color BGColorB = new Color(0.07f, 0.07f, 0.1f, 1f);
        public static readonly Color BGColorC = new Color(0.14f, 0.14f, 0.2f, 1f);
        
        public static readonly Color[] WaveFormColor =
        {
            new Color(0.5f, 0.5f, 0.5f, 1f),
            new Color(0.89f, 0.13f, 0.37f, 1f),
            new Color(1f, 0.69f, 0f, 1f),
            new Color(0.19f, 1f, 0.66f, 1f),
            new Color(0f, 0.7f, 0.72f, 1f),
            new Color(0f, 0.17f, 1f, 1f),
            new Color(0.49f, 0.11f, 0.84f),
        };

        public static readonly Color[] VolumeColors =
        {
            InactiveColor,
            new Color(0.25f, 0f, 0.1f, 1f),
            new Color(0.5f, 0f, 0.25f, 1f),
            new Color(0.5f, 0f, 0.5f, 1f),
            new Color(0.5f, 0f, 0.75f, 1f),
            new Color(0.5f, 0f, 1f, 1f),
            new Color(0.75f, 0f, 1f, 1f),
            new Color(1f, 0f, 1f, 1f),
        };
                
        public static float Freq(float baseFreq, float steps) => baseFreq * Mathf.Pow(1.0594630943593f, steps);

        public static AudioClip CreateClip(ICollection<float> samples, string clip_name)
        {
            var clip = AudioClip.Create(clip_name, samples.Count, 1, SampleRate, false);
            clip.SetData(samples.ToArray(), 0);
            return clip;
        }

        public static Texture2D CreateAudioClipTexture(IList<float> samples, int width, int height, 
            Color wavColor, Color bgColor)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var waveform = new float[width];

            var packSize = samples.Count / width + 1;
            var s = 0;
            for (var i = 0; i < samples.Count; i += packSize)
            {
                waveform[s] = Mathf.Abs(samples[i]);
                s++;
            }

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    tex.SetPixel(x, y, bgColor);
                }
            }

            for (var x = 0; x < waveform.Length; x++)
            {
                for (var y = 0; y <= waveform[x] * (height * .75f); y++)
                {
                    tex.SetPixel(x, height / 2 + y, wavColor);
                    tex.SetPixel(x, height / 2 - y, wavColor);
                }
            }

            tex.Apply();

            return tex;
        }
    }
}