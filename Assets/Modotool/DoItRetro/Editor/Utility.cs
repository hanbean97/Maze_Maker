using UnityEditor;
using UnityEngine;

namespace DoItRetro
{
    public static class Utility
    {
        static Material m_previewTexMaterial;
        
        public static void RecordUndoDirty(Object config, string undoName)
        {
            Undo.RecordObject(config, undoName);
            EditorUtility.SetDirty(config);
        }
        
        public static void DrawTexture_GetRectMinMax(string ctrl, Texture texture, Vector2 marginOff, out Rect fullGUIRect, out Vector2 minPos, out Vector2 maxPos)
        {
            GUI.SetNextControlName(ctrl);
            GUILayout.Box(texture);
            GetLastRectMinMax(texture, 1, marginOff, out fullGUIRect, out minPos, out maxPos);
        }

        public static void DrawTexture_GetRectMinMax(string ctrl, Texture texture, int size,
            Vector2 marginOff, out Rect fullGUIRect, out Vector2 minPos, out Vector2 maxPos)
        {
            GUI.SetNextControlName(ctrl);

            var w = texture.width;
            var h = texture.height;
            GUILayout.Box("",GUILayout.Width(w * size), GUILayout.Height(h * size));

            GetLastRectMinMax(texture, size, marginOff, out fullGUIRect, out minPos, out maxPos);

            if (m_previewTexMaterial == null)
                m_previewTexMaterial = new Material(Shader.Find("UI/Unlit/Transparent"));
            EditorGUI.DrawPreviewTexture(fullGUIRect, texture, m_previewTexMaterial);
        }
        
        public static void GetLastRectMinMax(Texture texture, int size, Vector2 marginOff, out Rect rect, out Vector2 minPos, out Vector2 maxPos)
        {
            rect = GUILayoutUtility.GetLastRect();
            minPos = rect.min;
            maxPos = rect.max;
            
            if (texture == null)
                return;
            
            var w = texture.width * size;
            var h = texture.height * size;
            
            var pitchWaveMargin = new Vector2(
                rect.size.x - w + marginOff.x,
                rect.size.y - h + marginOff.y);
            minPos = rect.min + 0.5f * pitchWaveMargin;
            maxPos = rect.max - 0.5f * pitchWaveMargin;
        }
        
        public static void GetGridPosCeil(Vector2 mousePos, Vector2 minPos,
            Vector2Int size, int xSteps, int ySteps,
            out int gridPosX, out int gridPosY)
        {
            var relPitchPos = mousePos - minPos;

            gridPosX = Mathf.CeilToInt(relPitchPos.x / size.x * xSteps);
            gridPosY = ySteps - Mathf.CeilToInt(relPitchPos.y / size.y * ySteps);
        }
        public static void GetGridPos_FloorXCeilY(Vector2 mousePos, Vector2 minPos,
            Vector2Int size, int xSteps, int ySteps,
            out int gridPosX, out int gridPosY)
        {
            var relPitchPos = mousePos - minPos;

            gridPosX = Mathf.FloorToInt(relPitchPos.x / size.x * xSteps); 
            gridPosY = ySteps - Mathf.CeilToInt(relPitchPos.y / size.y * ySteps);
        }
        
        public static bool EnsureTextureSetup(ref Texture2D tex, string textureName, ref bool texNotFound)
        {
            if (tex != null)
                return true;

            tex = LoadTexture($"t: texture2d {textureName}", ref texNotFound);
            if (tex == null)
            {
                EditorGUILayout.HelpBox(new GUIContent($"{textureName} not found!"));
                // tex = EditorGUILayout.ObjectField($"{textureName}", tex, typeof(Texture2D), false) as Texture2D;
            }   
            return false;
        }
        
        static Texture2D LoadTexture(string filter, ref bool textureNotFound)
        {
            if (textureNotFound)
                return null;
            var guids = AssetDatabase.FindAssets(filter);
            if (guids == null || guids.Length == 0)
                return null;
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null)
                textureNotFound = true;
            return tex;
        }
    }
}