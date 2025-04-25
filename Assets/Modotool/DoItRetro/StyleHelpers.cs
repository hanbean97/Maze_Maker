using UnityEngine;

namespace DoItRetro
{
    public static class StyleHelpers
    {
        public static void InitStyleOnOff(GUIStyle style, Texture2D normal, Texture2D active, 
            Color textColorNormal = default, Color textColorActive = default)
        {
            var stateNormal = new GUIStyleState { background = normal };
            var stateActive = new GUIStyleState { background = active };
            stateNormal.textColor = textColorNormal;
            stateActive.textColor = textColorActive;

            style.normal = stateNormal;
            style.hover = stateNormal;
            style.focused = stateNormal;
            style.active = stateActive;
            style.onNormal = stateActive;
            style.onActive = stateActive;
            style.onHover = stateActive;
            style.onFocused = stateActive;
        }
        public static void InitStyleOnOffPress(GUIStyle style, Texture2D normal, Texture2D active, Texture2D pressed,
            Color textColorNormal = default, Color textColorActive = default)
        {
            var stateOff = new GUIStyleState { background = normal };
            var stateOn = new GUIStyleState { background = active };
            var statePressed = new GUIStyleState { background = pressed };

            stateOff.textColor = textColorNormal;
            stateOn.textColor = textColorActive;
            statePressed.textColor = textColorActive;

            style.onNormal = stateOn;
            style.onHover = stateOn;
            style.onFocused = stateOn;
            style.focused = stateOn;
            style.normal = stateOff;
            style.hover = stateOff;
            style.active = statePressed;
            style.onActive = statePressed;
        }

        internal static void InitStyleOnOffFixedSize(GUIStyle style, Texture2D normal, Texture2D active, int width, int height)
        {
            InitStyleOnOff(style, normal, active);
            InitStyleFixedSize(style, width, height);
        }

        static void InitStyleFixedSize(GUIStyle style, int width, int height)
        {
            style.stretchHeight = false;
            style.stretchWidth = false;
            style.fixedWidth = width;
            style.fixedHeight = height;
        }
    }
}