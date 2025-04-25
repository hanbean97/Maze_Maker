using UnityEditor;
using UnityEngine;

namespace DoItRetro
{
    public static class CustomEditorPrefs
    {
        public static void SetColor(string key, Color value)
        {
            EditorPrefs.SetFloat(key + "/R", value.r);
            EditorPrefs.SetFloat(key + "/G", value.g);
            EditorPrefs.SetFloat(key + "/B", value.b);
            EditorPrefs.SetFloat(key + "/A", value.a);
        }

        public static Color GetColor(string key, Color defColor = default)
        {
            var r = EditorPrefs.GetFloat(key + "/R", defColor.r);
            var g = EditorPrefs.GetFloat(key + "/G", defColor.g);
            var b = EditorPrefs.GetFloat(key + "/B", defColor.b);
            var a = EditorPrefs.GetFloat(key + "/A", defColor.a);
            return new Color(r, g, b, a);
        }

        public static void ClearPrefsColor(string key)
        {
            EditorPrefs.DeleteKey($"{key}/R");
            EditorPrefs.DeleteKey($"{key}/G");
            EditorPrefs.DeleteKey($"{key}/B");
            EditorPrefs.DeleteKey($"{key}/A");
        }
    }
}
