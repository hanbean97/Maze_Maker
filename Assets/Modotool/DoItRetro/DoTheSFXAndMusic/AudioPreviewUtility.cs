using System;
using UnityEngine;

namespace DoTheSFXAndMusic
{
    public static class AudioPreviewUtility
    {
        // copied ------------------------------------------------------------------
        public static AudioSource AudioObject;

        public static void PlayAudioClip(string goName, AudioClip aClip, bool loop)
        {
            var go = new GameObject(goName);
            // {hideFlags = HideFlags.HideAndDontSave};
            var source = go.AddComponent<AudioSource>();
            source.clip = aClip;
            source.spatialBlend = 0f;
            source.loop = loop;
            source.volume = 1f;
            source.Play();

            AudioObject = source;
        }

        public static void CleanupAudioOnDone()
        {
            if (AudioObject == null || AudioObject.isPlaying)
                return;

            var timeSamples = AudioObject.timeSamples;
            var paused = timeSamples > 0
                         && timeSamples < AudioObject.clip.samples;
            if (paused)
                return;
            
            DestroyAudio();
        }

        public static void DestroyAudio()
        {
            if (AudioObject == null)
                return;
            UnityEngine.Object.DestroyImmediate(AudioObject.gameObject);
            AudioObject = null;
            Resources.UnloadUnusedAssets();
        }
    }
}
