using System;
using UnityEngine;

namespace DoItRetro
{
    public readonly struct ColorScope : IDisposable
    {
        readonly Color m_guiColor;
        readonly Color m_guiContentColor;
        readonly Color m_guiBGColor;

        public ColorScope(Color color)
        {
            m_guiColor = GUI.color;
            m_guiBGColor = GUI.backgroundColor;
            m_guiContentColor = GUI.contentColor;

            GUI.color = color;
        }

        public ColorScope(Color bgColor, Color contentColor)
        {
            m_guiColor = GUI.color;
            m_guiBGColor = GUI.backgroundColor;
            m_guiContentColor = GUI.contentColor;

            GUI.backgroundColor = bgColor;
            GUI.contentColor = contentColor;
        }

        public ColorScope(Color bgColor, Color contentColor, Color color)
        {
            m_guiColor = GUI.color;
            m_guiBGColor = GUI.backgroundColor;
            m_guiContentColor = GUI.contentColor;

            GUI.backgroundColor = bgColor;
            GUI.contentColor = contentColor;
            GUI.color = color;
        }

        public void Dispose()
        {
            GUI.color = m_guiColor;
            GUI.backgroundColor = m_guiBGColor;
            GUI.contentColor = m_guiContentColor;
        }

        public override bool Equals(object other)
        {
            return base.Equals(other);
        }
    }

}