using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DoTheSFXAndMusic
{
    [CreateAssetMenu(fileName = nameof(RetroSFXConfig), menuName = "RetroSFX/" + nameof(RetroSFXConfig))]
    public class RetroSFXConfig : ScriptableObject,
            IList<RetroSFX>
        // IList, IReadOnlyList<RetroSFX>
    {
        [SerializeField] List<RetroSFX> m_sounds = new List<RetroSFX>();

        [SerializeField] AnimationCurve[] m_customCurves = new AnimationCurve[7];
        [SerializeField] Color[] m_customCurveColors = new Color[7];

        #region IList
        public int Count => m_sounds.Count;
        bool ICollection<RetroSFX>.IsReadOnly => false;
        public void Add(RetroSFX item) => m_sounds.Add(item);
        public void Insert(int index, object value) => m_sounds.Insert(index, value as RetroSFX);
        public void Remove(object value) => m_sounds.Remove(value as RetroSFX);
        public void Clear() => m_sounds.Clear();
        public bool Contains(RetroSFX item) => m_sounds.Contains(item);
        public void CopyTo(RetroSFX[] array, int arrayIndex) => m_sounds.CopyTo(array, arrayIndex);
        public bool Remove(RetroSFX item) => m_sounds.Remove(item);
        public IEnumerator<RetroSFX> GetEnumerator() => m_sounds.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int IndexOf(RetroSFX item) => m_sounds.IndexOf(item);
        public void Insert(int index, RetroSFX item) => m_sounds.Insert(index, item);
        public void RemoveAt(int index) => m_sounds.RemoveAt(index);

        public RetroSFX this[int index]
        {
            get => m_sounds[index];
            set => m_sounds[index] = value;
        }
        
        // bonus
        public int FindIndex(Predicate<RetroSFX> func) => m_sounds.FindIndex(func);
        #endregion

        public IEnumerable<RetroSFX> Sounds => m_sounds;

        public AnimationCurve GetCurve(int idx)
        {
            if (m_customCurves == null)
                m_customCurves = new AnimationCurve[7];
            if (m_customCurves.Length != 7)
                Array.Resize(ref m_customCurves, 7);
            return m_customCurves[idx];
        }

        public Color GetCurveColor(int idx)
        {
            if (m_customCurveColors == null)
                m_customCurveColors = new Color[7];
            if (m_customCurveColors.Length != 7)
                Array.Resize(ref m_customCurveColors, 7);
            var col = m_customCurveColors[idx];
            if (col.a != 0) 
                return col;
            
            col = Color.grey;
            m_customCurveColors[idx] = col;
            return m_customCurveColors[idx];
        }

        public void SetCurveColor(int idx, Color c)
        {           
            if (m_customCurveColors == null)
                m_customCurveColors = new Color[7];
            if (m_customCurveColors.Length != 7)
                Array.Resize(ref m_customCurveColors, 7);
            m_customCurveColors[idx] = c;
        }
        public void SetCurve(int idx, AnimationCurve curve)
        {
            if (m_customCurves == null)
                m_customCurves = new AnimationCurve[7];
            if (m_customCurves.Length != 7)
                Array.Resize(ref m_customCurves, 7);
            m_customCurves[idx] = curve;
        }
        public void AddSound()
        {
            Add(new RetroSFX()
            {
                Name = "Untitled",
                Duration = 16,
                SectionMin = 0,
                SectionMax = 0,
                Loop = false
            });
        }

        #region Audio Clip
        public void BuildSounds()
        {
            foreach (var sound in m_sounds)
            {
                RetroSFXBuilder.TempSamples.Clear();
                RetroSFXBuilder.BuildSamplesSFX(sound, RetroSFXBuilder.TempSamples, this);
                sound.m_clip = Utility.CreateClip(RetroSFXBuilder.TempSamples, sound.Name);
            }
        }

        public AudioClip GetClip(int idx)
        {
            if (m_sounds == null)
                return null;
            if (idx < 0 || idx >= m_sounds.Count)
                return null;

            if (m_sounds[idx].m_clip != null) 
                return m_sounds[idx].m_clip;
            
            RetroSFXBuilder.TempSamples.Clear();
            RetroSFXBuilder.BuildSamplesSFX(m_sounds[idx], RetroSFXBuilder.TempSamples, this);
            m_sounds[idx].m_clip = Utility.CreateClip(RetroSFXBuilder.TempSamples, m_sounds[idx].Name);

            return m_sounds[idx].m_clip;
        }
        
        public AudioClip GetClip(string sfxName)
        {
            if (m_sounds == null)
                return null;
            var idx = m_sounds.FindIndex(a => string.Equals(a.Name, sfxName, StringComparison.Ordinal));
            return GetClip(idx);
        }
        #endregion
    }

    [Serializable]
    public struct FXSettings
    {
        public int DropMaxAmount;
        public int DropPitch;
        public int ARPSpeed;
    }
    
    [Serializable]
    public class RetroSFX
    {
        public string Name;
        public int Duration;
        public int SectionMin;
        public int SectionMax;

        public int OctaveMin = 0;
        public int OctaveMax = 8;

        public bool Loop;

        const int k_soundDataMaxLength = 32;
        public int MaxLength => k_soundDataMaxLength;
        public SoundData[] SoundData = new SoundData[k_soundDataMaxLength];

        public FXSettings FXSettings;
        
        // non-serialized, regenerate this, saves disk-space
        [NonSerialized]
        internal AudioClip m_clip;
    }
}