using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace DoTheSFXAndMusic
{
    [CreateAssetMenu(fileName = nameof(RetroSFXConfig), menuName = "RetroSFX/" + nameof(RetroSFXConfig))]
    public class RetroMusicConfig : ScriptableObject
        // IList, IReadOnlyList<RetroSFX>
    {
        [SerializeField] RetroSFXConfig m_sfxConfig;
        
        [SerializeField] List<RetroMusic> m_musicTracks = new List<RetroMusic>();
        [SerializeField] List<RetroMusicPart> m_musicParts = new List<RetroMusicPart>();
        
        public List<RetroMusic> MusicTracks => m_musicTracks;
        public List<RetroMusicPart> MusicParts => m_musicParts;
        public RetroSFXConfig SFXConfig => m_sfxConfig;
        public float LimitVolume;

        public void AddMusicTrack()
        {
            MusicTracks.Add(new RetroMusic()
            {
                Name = "Untitled",
                Parts = new List<RetroMusicPartRef>()
            });
        }
        public void AddPart()
        {
            var color = Color.HSVToRGB(UnityEngine.Random.Range(0f,1f),0.75f, 0.5f);
            MusicParts.Add(new RetroMusicPart()
            {
                Name = "Untitled",
                Color = color,
                Snippets = new List<RetroMusicSFXSnippet>()
            });
        }

        public void SetSFXConfig(RetroSFXConfig sfxConfig) => m_sfxConfig = sfxConfig;

        #region AudioClip
        public void BuildMusicTracks()
        {
            foreach (var music in m_musicTracks) 
                BuildTrack(music);
        }
        
        public void BuildMusicParts()
        {
            foreach (var part in m_musicParts) 
                BuildPart(part);
        }

        void BuildTrack(RetroMusic music)
        {
            RetroSFXBuilder.BuildSamplesMusicTrack(this, music, RetroSFXBuilder.TempSamplesTrack);
            music.m_clip = Utility.CreateClip(RetroSFXBuilder.TempSamplesTrack, music.Name);
        }
        void BuildPart(RetroMusicPart part)
        {
            RetroSFXBuilder.BuildSamplesMusicPart(this, part, RetroSFXBuilder.TempSamplesTrack);
            part.m_clip = Utility.CreateClip(RetroSFXBuilder.TempSamplesTrack, part.Name);
        }

        public AudioClip GetClipForTrack(int idx)
        {
            if (m_musicTracks == null)
                return null;
            if (idx < 0 || idx >= m_musicTracks.Count)
                return null;

            if (m_musicTracks[idx].m_clip != null) 
                return m_musicTracks[idx].m_clip;

            BuildTrack(m_musicTracks[idx]);
            return m_musicTracks[idx].m_clip;
        }
        
        public AudioClip GetClipForTrack(string musicTrackName)
        {
            if (m_musicTracks == null)
                return null;
            var idx = m_musicTracks.FindIndex(a => string.Equals(a.Name, musicTrackName, StringComparison.Ordinal));
            return GetClipForTrack(idx);
        }
        
        public AudioClip GetClipForPart(int idx)
        {
            if (m_musicParts == null)
                return null;
            if (idx < 0 || idx >= m_musicParts.Count)
                return null;

            if (m_musicParts[idx].m_clip != null) 
                return m_musicParts[idx].m_clip;

            BuildPart(m_musicParts[idx]);
            return m_musicParts[idx].m_clip;
        }
        
        public AudioClip GetClipForPart(string musicPartName)
        {
            if (m_musicParts == null)
                return null;
            var idx = m_musicParts.FindIndex(a => string.Equals(a.Name, musicPartName, StringComparison.Ordinal));
            return GetClipForPart(idx);
        }
        #endregion
    }

    [Serializable]
    public struct RetroMusicSFXSnippet
    {
        public int TimePosition;
        public int TrackPosition;

        public string SFXName;
        public int SFXIdx;
        
        // public int DurationMod;
        // public int PitchMod;
        // public int SectionMinMod;
        // public int SectionMaxMod;
    }
    
    
    [Serializable]
    public class RetroMusicPart
    {
        public string Name;
        public Color Color;

        public int TimeGrid = 16;
        public int Duration = 32;
        public int Tracks = 1;
        
        public float Volume;

        public bool[] TrackMuted;
        public List<RetroMusicSFXSnippet> Snippets;
        
        // non-serialized, regenerate this, saves disk-space
        [NonSerialized] 
        internal AudioClip m_clip;

    }
    
    [Serializable]
    public struct RetroMusicPartRef
    {
        // public int TimePosition;
        
        public string PartName;
        public int PartIdx;
    }
    
    [Serializable]
    public class RetroMusic
    {
        public string Name;
        public bool Loop;

        public List<RetroMusicPartRef> Parts = new List<RetroMusicPartRef>();

        // todo auto-update:
        public int Duration;// => Parts.Max(p => p.TimePosition + p.Duration);

        // non-serialized, regenerate this, saves disk-space
        [NonSerialized] 
        internal AudioClip m_clip;
    }
}