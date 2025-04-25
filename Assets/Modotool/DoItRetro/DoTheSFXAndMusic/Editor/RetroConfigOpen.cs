using UnityEditor;
using UnityEditor.Callbacks;

namespace DoTheSFXAndMusic
{
    public static class RetroConfigOpen
    {
        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            var sfxConfig = obj as RetroSFXConfig;
            var musicConfig = obj as RetroMusicConfig;

            if (sfxConfig != null)
            {
                var w = DoTheSFXEditorWindow.Open();
                w.Select(sfxConfig, 0);
                return true;
            }

            if (musicConfig != null)
            {
                var w = DoTheMusicEditorWindow.Open();
                w.Select(musicConfig);
                return true;
            }
            return false; // we did not handle the open

        }

    }
}