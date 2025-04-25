using System;
using System.Collections.Generic;
using UnityEngine;

using static DoTheSFXAndMusic.Utility;

namespace DoTheSFXAndMusic
{
    public static class RetroSFXBuilder
    {
        public static readonly float[] NoiseBuffer = new float[32];
        public static readonly List<float> TempSamplesTrack = new List<float>();
        public static readonly List<float> TempSamples = new List<float>();

        public static readonly List<float> ModulatedVolumes = new List<float>();
        public static readonly List<float> ModulatedFrequencyMod = new List<float>();

        // some compensation, so volumes dont appear to different, note that volumes over >1 will also cut the shape
        // user can decrease vol-level to prevent it.
        const float k_pulseVolCompensation = 0.75f;
        const float k_sawToothVolCompensation = 0.9f;
        const float k_sineWaveVolCompensation = 1.5f;
        const float k_triangleWaveVolCompensation = 1.75f;
        const float k_noiseVolCompensation = 0.9f;
        
        public static void BuildSamplesMusicPart(RetroMusicConfig musicConfig, RetroMusicPart part, List<float> samples)
        {
            var maxSamples = part.Duration * QuarterSampleRate;
            
            samples.Clear();
            for (var i = 0; i < maxSamples; i++) 
                samples.Add(0);
            
            BuildSamplesMusicPart(musicConfig, part, 0, samples);
        }
        
        public static void BuildSamplesMusicPart(RetroMusicConfig musicConfig, RetroMusicPart part, int timeOffset, 
            List<float> samples)
        {
            var sampleIdxOff = timeOffset * QuarterSampleRate;
            
            // var noiseBuffer = new float[32];

            foreach (var snip in part.Snippets)
            {
                var sfxConfig = musicConfig.SFXConfig;
                var sfx = sfxConfig[snip.SFXIdx];

                TempSamples.Clear();
                BuildSamplesSFX(sfx, TempSamples, musicConfig.SFXConfig, NoiseBuffer, part.Volume);

                var sampleStartPos = sampleIdxOff + snip.TimePosition * QuarterSampleRate;
                for (var i = sampleStartPos; i < sampleStartPos + TempSamples.Count && i < samples.Count; i++)
                    samples[i] += TempSamples[i - sampleStartPos];
            }
        }
        
        public static void BuildSamplesMusicTrack(RetroMusicConfig musicConfig,
            RetroMusic musicTrack, List<float> samples)
        {
            GetDurationForMusicArrangement(musicConfig, musicTrack, out musicTrack.Duration);
            var maxSamples = musicTrack.Duration * QuarterSampleRate;
            
            samples.Clear();
            for (var i = 0; i < maxSamples; i++) 
                samples.Add(0);
            
            var t = 0;
            foreach (var p in musicTrack.Parts)
            {
                if (p.PartIdx < 0 || p.PartIdx >= musicConfig.MusicParts.Count)
                    continue;
                var part = musicConfig.MusicParts[p.PartIdx];

                BuildSamplesMusicPart(musicConfig, part, t, samples);
                
                t += part.Duration;
            }
        }
        
        public static void GetDurationForMusicArrangement(RetroMusicConfig musicConfig, RetroMusic musicTrack, out int duration)
        {
            duration = 0;
            foreach (var p in musicTrack.Parts)
            {
                if (p.PartIdx < 0 || p.PartIdx >= musicConfig.MusicParts.Count)
                    continue;

                var part = musicConfig.MusicParts[p.PartIdx];
                duration += part.Duration;
            }
        }

        struct BuildSampleData
        {
            public float P;
            public float Vol;
            public float Mod;
            public int LastWaveform;
        }
        
        public static void BuildSamplesSFX(RetroSFX sfx,
            List<float> samples, RetroSFXConfig config) => BuildSamplesSFX(sfx, samples, config, NoiseBuffer, 1f);
        public static void BuildSamplesSFX(RetroSFX sfx,
            List<float> samples, RetroSFXConfig config, IList<float> noiseBuffer, float volumeMultiplier)
        {
            var samplesTotal = QuarterSampleRate * sfx.Duration;
            var samplesPerSoundData = samplesTotal / SoundDataMaxLength;

            var sampleIdx = 0;
            samples.Clear();

            var p = new BuildSampleData()
            {
                LastWaveform = (int) WaveForm.Select
            };
            for (var i = 0; i < SoundDataMaxLength; i++, sampleIdx += samplesPerSoundData)
            {
                var soundData = sfx.SoundData[i];
                
                var octave = Mathf.FloorToInt((float) soundData.Pitch / 12);
                var outsideOfOctaveRange = octave < sfx.OctaveMin || octave >= sfx.OctaveMax;

                var shape = soundData.WaveForm;
                var currentIsNothing = soundData.Volume == 0 || shape == (int) WaveForm.Select ||
                                       outsideOfOctaveRange;
                if (currentIsNothing)
                {
                    shape = p.LastWaveform;
                    p.LastWaveform = (int) WaveForm.Select;

                    if (shape == 0)
                    {
                        AddNothing(samplesPerSoundData, samples);
                        continue;
                    }

                    // current is nothing but we fade out super fast with last shape instead, to avoid sound glitches
                    FastFadeOutMod(p, samplesPerSoundData);
                }
                else
                {
                    p.LastWaveform = soundData.WaveForm;
                    // var mod = GetFreqMod(soundData, samplesTotal);
                    ModulateVolumesAndFrequencies(sfx, i, samplesPerSoundData, samplesTotal,
                        sfx.FXSettings, sampleIdx, volumeMultiplier);
                }
                
                if (shape >= (int) WaveForm.Custom)
                {
                    AddCustom(shape - (int) WaveForm.Custom, ref p, samplesPerSoundData, config, samples);
                    continue;
                }
                
                switch ((WaveForm) shape)
                {
                    case WaveForm.Pulse:
                        AddPulse(ref p, samplesPerSoundData, samples);
                        break;
                    case WaveForm.Sine:
                        AddSineWave(ref p, samplesPerSoundData, samples);
                        break;
                    case WaveForm.Sawtooth:
                        AddSawTooth(ref p, samplesPerSoundData, samples);
                        break;
                    case WaveForm.Triangle:
                        AddTriangle(ref p, samplesPerSoundData, samples);
                        break;
                    case WaveForm.Noise:
                        AddNoise(ref p, samplesPerSoundData, noiseBuffer, samples);
                        break;
                    case WaveForm.Custom:
                    case WaveForm.Select:
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }
        static float GetFreqMod(SoundData soundData, int samplesTotal)
        {
            var freq = Freq(C0, soundData.Pitch);
            var mod = GetFreqMod(freq, samplesTotal);
            return mod;
        }

        static float GetFreqMod(float freq, int samplesTotal)
        {
            var seconds = (float) samplesTotal / SampleRate;
            var repeats = freq * seconds;
            var mod = samplesTotal / repeats;
            return mod;
        }

        static void ModulateVolumesAndFrequencies(RetroSFX sfx, int soundDataIdx, int samples, int samplesTotal, 
            FXSettings fxSettings, int off, float volumeMult)
        {
            var soundData = sfx.SoundData[soundDataIdx];

            ModulatedVolumes.Clear();
            ModulatedFrequencyMod.Clear();

            switch (soundData.Effect)
            {
                case FX.Slide: SlideMod(sfx, soundDataIdx, samples, samplesTotal, volumeMult); break;
                case FX.Vibrato: VibratoMod(soundData, samples, samplesTotal, volumeMult); break; 
                case FX.Drop: DropMod(soundData, samples, samplesTotal, fxSettings, volumeMult); break; 
                case FX.FadeIn: FadeInMod(soundData, samples, samplesTotal, volumeMult); break;
                case FX.FadeOut: FadeOutMod(soundData, samples, samplesTotal, volumeMult); break;
                case FX.ARP: ARPMod(sfx, soundDataIdx, samples, samplesTotal, fxSettings, off, volumeMult); break;
                case FX.None: DefaultMod(soundData, samples, samplesTotal, volumeMult);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
        
        static void DefaultMod(SoundData soundData, int samples, int samplesTotal, float volumeMult)
        {
            var mod = GetFreqMod(soundData, samplesTotal);
            for (var i = 0; i < samples; i++)
            {
                ModulatedVolumes.Add(volumeMult * soundData.Volume);
                ModulatedFrequencyMod.Add(mod);
            }
        }

        static void SlideMod(RetroSFX sfx, int soundDataIdx, int samples, int samplesTotal, float volumeMult)
        {
            var currSoundData = sfx.SoundData[soundDataIdx];
            
            if (soundDataIdx + 1 >= sfx.SoundData.Length)
            {
                DefaultMod(currSoundData, samples, samplesTotal, volumeMult);
                return;
            }
            
            var nextSoundData = sfx.SoundData[soundDataIdx + 1];

            var vol = currSoundData.Volume;
            var nextVol = nextSoundData.Volume;
            
            var mod = GetFreqMod(currSoundData, samplesTotal);
            var nextMod = GetFreqMod(nextSoundData, samplesTotal);
            
            for (var i = 0; i < samples; i++)
            {
                ModulatedVolumes.Add(volumeMult * Mathf.Lerp(vol, nextVol, (float) i / samples));
                ModulatedFrequencyMod.Add(Mathf.Lerp(mod, nextMod, (float) i / samples));
            }
        }
        
        static void VibratoMod(SoundData soundData, int samples, int samplesTotal, float volumeMult)
        {
            var vol = soundData.Volume;
            var modMin = GetFreqMod(Freq(C0, soundData.Pitch - 0.25f), samplesTotal);
            var modMax = GetFreqMod(Freq(C0, soundData.Pitch + 0.25f), samplesTotal);

            for (var i = 0; i < samples; i++)
            {
                var sin = 0.5f * (1 + Mathf.Sin(i/ (0.1f * QuarterSampleRate)));
                ModulatedVolumes.Add(volumeMult * vol);
                ModulatedFrequencyMod.Add(Mathf.Lerp(modMin, modMax, sin));
            }
        }
        
        static void DropMod(SoundData soundData, int samples, int samplesTotal,
            FXSettings fxSettings, float volumeMult)
        {
            var mod = GetFreqMod(soundData, samplesTotal);

            var amount= Mathf.Min(fxSettings.DropMaxAmount, Mathf.Abs(soundData.Pitch - fxSettings.DropPitch));
            var dir = fxSettings.DropPitch > soundData.Pitch ? 1f : -1f;
            var targetPitch = soundData.Pitch + dir * amount;
            var targetFreq = Freq(C0, targetPitch);
            // var maxDrop = Freq(C0, soundData.Pitch - fxSettings.DropMaxAmount);
            // var clampDrop = Freq(C0, fxSettings.DropPitch);
            var dropMod = GetFreqMod(targetFreq, samplesTotal);
            
            for (var i = 0; i < samples; i++)
            {
                ModulatedVolumes.Add(volumeMult * soundData.Volume);
                ModulatedFrequencyMod.Add(Mathf.Lerp(mod, dropMod, (float) i / samples));
            }
        }
        
        static void FadeInMod(SoundData soundData, int samples, int samplesTotal, float volumeMult)
        {
            var mod = GetFreqMod(soundData, samplesTotal);
            var vol = soundData.Volume;
            var fromVol = 0f;
            
            for (var i = 0; i < samples; i++)
            {
                ModulatedVolumes.Add(volumeMult * Mathf.Lerp(fromVol, vol, (float) i / samples));
                ModulatedFrequencyMod.Add(mod);
            }
        }
        
        static void FastFadeOutMod(BuildSampleData lastData, int samples)
        {
            ModulatedVolumes.Clear();
            ModulatedFrequencyMod.Clear();

            var mod = lastData.Mod;
            var vol = lastData.Vol;
            for (var i = 0; i < samples; i++)
            {
                vol = Mathf.Lerp(vol, 0f, 0.01f);

                ModulatedVolumes.Add(vol);
                ModulatedFrequencyMod.Add(mod);
            }
        }
        
        static void FadeOutMod(SoundData soundData, int samples, int samplesTotal, float volumeMult)
        {
            var mod = GetFreqMod(soundData, samplesTotal);
            var vol = soundData.Volume;
            var toVol = 0f;
            
            for (var i = 0; i < samples; i++)
            {
                ModulatedVolumes.Add(volumeMult * Mathf.Lerp(vol, toVol, (float) i / samples));
                ModulatedFrequencyMod.Add(mod);
            }
        }
        static void ARPMod(RetroSFX sfx, int soundDataIdx, int samples, int samplesTotal, FXSettings fxSettings, 
            int off, float volumeMult)
        {
            var rangeIdx = Mathf.FloorToInt((float) soundDataIdx / 4) * 4;
            
            var currSoundData = sfx.SoundData[soundDataIdx];
            var vol = currSoundData.Volume;

            var a = sfx.SoundData[rangeIdx];
            var b = sfx.SoundData[rangeIdx + 1];
            var c = sfx.SoundData[rangeIdx + 2];
            var d = sfx.SoundData[rangeIdx + 3];
            
            var aMod = GetFreqMod(a, samplesTotal);
            var bMod = GetFreqMod(b, samplesTotal);
            var cMod = GetFreqMod(c, samplesTotal);
            var dMod = GetFreqMod(d, samplesTotal);

            var modCycleDuration = QuarterSampleRate * 0.125f * sfx.Duration * 1f / Mathf.Max(fxSettings.ARPSpeed, 
                float.Epsilon); // multiplied by a quarter for quarter segmentation
            
            for (var i = 0; i < samples; i++)
            {
                ModulatedVolumes.Add(volumeMult * vol);

                var cyclePos = (off + i) % modCycleDuration;
                if (cyclePos < 0.25f * modCycleDuration)
                    ModulatedFrequencyMod.Add(aMod);
                else if(cyclePos < 0.5f * modCycleDuration)
                    ModulatedFrequencyMod.Add(bMod);
                else if(cyclePos < 0.75f * modCycleDuration)
                    ModulatedFrequencyMod.Add(cMod);
                else
                    ModulatedFrequencyMod.Add(dMod);
            }
        }
        
        static void AddNothing(int samples, ICollection<float> sampleList)
        {
            for (var i = 0; i < samples; i++)
                sampleList.Add(0f);
        }
        
        static void AddPulse(ref BuildSampleData d, int samples, ICollection<float> sampleList)
        {
            var i = 0;

            while (i < samples)
            {
                d.Vol = Mathf.Lerp(d.Vol, ModulatedVolumes[i], 0.01f);
                d.Mod = ModulatedFrequencyMod[i];

                var amp = d.Vol / VolumeLevels * k_pulseVolCompensation;

                d.P = Mathf.Repeat(d.P + 1 / d.Mod, 1f);
                sampleList.Add(d.P < 0.5f ? amp : -amp);
                i++;
            }
        }
        
        static void AddSawTooth(ref BuildSampleData d, int samples, ICollection<float> sampleList)
        {
            var i = 0;
            while (i < samples)
            {
                d.Vol = Mathf.Lerp(d.Vol, ModulatedVolumes[i], 0.01f);
                d.Mod = ModulatedFrequencyMod[i];

                var amp = d.Vol / VolumeLevels * k_sawToothVolCompensation;
                var sawAmp = 2 * amp;

                d.P = Mathf.Repeat(d.P + 1 / d.Mod, 1f);
                sampleList.Add(d.P < 0.5 ? d.P * sawAmp : d.P * sawAmp - sawAmp);
                i++;
            }
        }

        static void AddSineWave(ref BuildSampleData d, int samples, ICollection<float> sampleList)
        {
            var i = 0;

            while (i < samples)
            {
                d.Vol = Mathf.Lerp(d.Vol, ModulatedVolumes[i], 0.01f);
                d.Mod = ModulatedFrequencyMod[i];
                
                var amp = d.Vol / VolumeLevels * k_sineWaveVolCompensation;

                d.P = Mathf.Repeat(d.P + 1 / d.Mod, 1f);
                sampleList.Add(Mathf.Sin(d.P * 2 * Mathf.PI) * amp);
                i++;
            }

        }

        static void AddTriangle(ref BuildSampleData d, int samples, ICollection<float> sampleList)
        {
            var i = 0;
            
            while (i < samples)
            {
                d.Vol = Mathf.Lerp(d.Vol, ModulatedVolumes[i], 0.01f);
                d.Mod = ModulatedFrequencyMod[i];
                
                var amp = d.Vol / VolumeLevels * k_triangleWaveVolCompensation;

                var ampA = 4 * amp;
                var ampB = 2 * amp;
                
                // var p = (off + i) % mod / mod;
                d.P = Mathf.Repeat(d.P + 1 / d.Mod, 1f);
                sampleList.Add(d.P < 0.25 ? d.P * ampA : d.P < 0.75 ? ampB - d.P * ampA : d.P * ampA - ampA);
                i++;
            }
        }

        static void AddNoise(ref BuildSampleData d, int samples, IList<float> noiseBuffer, ICollection<float> sampleList)
        {
            var i = 0;
            var j = 0;
            
            while (i < samples)
            {
                d.Vol = Mathf.Lerp(d.Vol, ModulatedVolumes[i], 0.01f);
                d.Mod = ModulatedFrequencyMod[i];
                var amp = d.Vol / VolumeLevels * k_noiseVolCompensation;

                if (j == 0)
                {
                    for (var n = 0; n < noiseBuffer.Count; n++)
                        noiseBuffer[n] = UnityEngine.Random.value * 2.0f - 1.0f;
                }

                // var d.P = (off + i) % mod / mod;
                d.P = Mathf.Repeat(d.P + 1 / d.Mod, 1f);
                var buffIdxA = (int) (d.P * noiseBuffer.Count % noiseBuffer.Count);
                sampleList.Add(noiseBuffer[buffIdxA] * amp);
                i++;
                j = (int) ((j + 1) % d.Mod);
            }
        }
        
        static void AddCustom(int customIdx, ref BuildSampleData d, int samples,
            RetroSFXConfig sfxConfig, ICollection<float> sampleList)
        {
            var i = 0;

            while (i < samples)
            {
                d.Vol = Mathf.Lerp(d.Vol, ModulatedVolumes[i], 0.01f);
                d.Mod = ModulatedFrequencyMod[i];
                
                var amp = d.Vol / VolumeLevels;

                // var d.P = (off + i) % mod / mod;
                d.P = Mathf.Repeat(d.P + 1 / d.Mod, 1f);

                var curve = sfxConfig.GetCurve(customIdx);
                sampleList.Add(curve.Evaluate(d.P) * amp);
                i++;
            }
        }
    }
}