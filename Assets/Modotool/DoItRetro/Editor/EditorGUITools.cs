using UnityEngine;

namespace DoItRetro
{
    public static class EditorGUITools
    {
        static readonly Texture2D k_backgroundTexture = Texture2D.whiteTexture;

        static readonly GUIStyle k_textureStyle = new GUIStyle
            {normal = new GUIStyleState {background = k_backgroundTexture}};

        public static void DrawRect(Rect position, Color color, GUIContent content = null)
        {
            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUI.Box(position, content ?? GUIContent.none, k_textureStyle);
            GUI.backgroundColor = backgroundColor;
        }

        public static void LayoutBox(Color color, GUIContent content = null)
        {
            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUILayout.Box(content ?? GUIContent.none, k_textureStyle);
            GUI.backgroundColor = backgroundColor;
        }
    }
}