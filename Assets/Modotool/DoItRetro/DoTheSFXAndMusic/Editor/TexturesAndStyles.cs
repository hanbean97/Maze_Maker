using DoItRetro;
using UnityEngine;
using static DoItRetro.Utility;

namespace DoTheSFXAndMusic
{
    public struct TexturesAndStyles
    {
        public Texture2D EditCursor;
        
        public Texture2D DurationIcon;
        public Texture2D TimeGridIcon;
        public Texture2D OctaveIcon;

        public Texture2D SelectIcnLight;
        public Texture2D SelectIcnDark;
        
        public Texture2D[] FXSmallIcn;
        
        public ButtonStyleData Button;
        public GUIStyle WhiteBoxStyle;

        public ButtonStyleData Grid;

        public ButtonStyleData ViewData;
        public ButtonStyleData ViewDraw;
                
        public ButtonStyleData ViewParts;
        public ButtonStyleData ViewTrack;
        
        public ButtonStyleData Up;
        public ButtonStyleData Down;

        public ButtonStyleData Delete;
        public ButtonStyleData Add;
        public ButtonStyleData Edit;
        
        public ButtonStyleData Mute;
        public ButtonStyleData Sound;

        public ButtonStyleData Export;
        public ButtonStyleData Play;
        public ButtonStyleData Pause;
        public ButtonStyleData Stop;
        public ButtonStyleData Loop;

        public ButtonStyleData[] Waveform;
        public ButtonStyleData[] FX;
        public bool TextureNotFound;
    }
    
    public struct ButtonStyleData
    {
        public Texture2D Normal;
        public Texture2D Active;
        
        public GUIStyle Style;
    }

    public static class TextureStyleOperations
    {
        public static TexturesAndStyles RetroMusicSFXStyles;
        
        public static bool AllTexturesSetup(ref TexturesAndStyles data)
        {
            var allOK = true;

            allOK &= EnsureTextureSetup(ref data.EditCursor, "icn_retro_cursor", ref data.TextureNotFound);
            
            allOK &= EnsureTextureSetup(ref data.DurationIcon, "icn_retro_duration_white", ref data.TextureNotFound);
            allOK &= EnsureTextureSetup(ref data.TimeGridIcon, "icn_retro_grid_white", ref data.TextureNotFound);
            
            allOK &= EnsureTextureSetup(ref data.OctaveIcon, "icn_retro_octave_white", ref data.TextureNotFound);
            
            allOK &= EnsureTextureSetup(ref data.SelectIcnLight, "icn_retro_select_blue", ref data.TextureNotFound);
            allOK &= EnsureTextureSetup(ref data.SelectIcnDark, "icn_retro_select_white", ref data.TextureNotFound);
            
            if (data.FXSmallIcn == null)
                data.FXSmallIcn = new Texture2D[7];
            allOK &= EnsureTextureSetup(ref data.FXSmallIcn[(int) FX.FadeIn], "icn_retro_effect_fadein_small", ref data.TextureNotFound);
            allOK &= EnsureTextureSetup(ref data.FXSmallIcn[(int) FX.FadeOut], "icn_retro_effect_fadeout_small", ref data.TextureNotFound);
            allOK &= EnsureTextureSetup(ref data.FXSmallIcn[(int) FX.Vibrato], "icn_retro_effect_vibrato_small", ref data.TextureNotFound);
            allOK &= EnsureTextureSetup(ref data.FXSmallIcn[(int) FX.Slide], "icn_retro_effect_slide_small", ref data.TextureNotFound);
            allOK &= EnsureTextureSetup(ref data.FXSmallIcn[(int) FX.Drop], "icn_retro_effect_drop_small", ref data.TextureNotFound);
            allOK &= EnsureTextureSetup(ref data.FXSmallIcn[(int) FX.ARP], "icn_retro_effect_arp_small", ref data.TextureNotFound);
            
            allOK &= EnsureGUIStyleSetup(ref data.Button, "icn_retro_blue",  "icn_retro_white", ref data.TextureNotFound);
            if (allOK)
            {
                data.Button.Style.border = new RectOffset {bottom = 7, left = 7, right = 7, top = 7};
                data.Button.Style.padding = new RectOffset(5, 5, 2, 2);
            }

            allOK &= EnsureGUIStyleSetup(ref data.Grid, "icn_retro_timegrid", "icn_retro_timegrid_active",
                ref data.TextureNotFound);
            
            allOK &= EnsureGUIStyleSetup(ref data.ViewData, "icn_retro_view", "icn_retro_view_active", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.ViewDraw, "icn_retro_track", "icn_retro_track_active", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.ViewParts, "icn_retro_parts", "icn_retro_parts_active", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.ViewTrack, "icn_retro_tracks", "icn_retro_tracks_active", ref data.TextureNotFound);
            
            allOK &= EnsureGUIStyleSetup(ref data.Up, "icn_retro_up", "icn_retro_up_active", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.Down, "icn_retro_down", "icn_retro_down_active", ref data.TextureNotFound);

            allOK &= EnsureGUIStyleSetup(ref data.Add, "icn_retro_plus", "icn_retro_plus_active", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.Delete, "icn_retro_delete", "icn_retro_delete_active", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.Edit, "icn_retro_edit", "icn_retro_edit_active", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.Mute, "icn_retro_mute", "icn_retro_mute_active", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.Sound, "icn_retro_sound", "icn_retro_sound_active", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.Export, "icn_retro_export", "icn_retro_export_active", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.Play, "icn_retro_play", "icn_retro_play_active", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.Pause, "icn_retro_pause", "icn_retro_pause_active", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.Stop, "icn_retro_stop", "icn_retro_stop_active", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.Loop, "icn_retro_loop", "icn_retro_loop_active", ref data.TextureNotFound);
            
            if (data.Waveform == null)
                data.Waveform = new ButtonStyleData[13];
            allOK &= EnsureGUIStyleSetup(ref data.Waveform[(int) WaveForm.Select], "icn_retro_sfx_select", "icn_retro_sfx_select_active", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.Waveform[(int) WaveForm.Custom], "icn_retro_small_custom_box", "icn_retro_small_custom_active_box", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.Waveform[(int) WaveForm.Noise], "icn_retro_small_noise_box", "icn_retro_small_noise_active_box", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.Waveform[(int) WaveForm.Pulse], "icn_retro_small_pulse_box","icn_retro_small_pulse_active_box", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.Waveform[(int) WaveForm.Sawtooth], "icn_retro_small_sawtooth_box","icn_retro_small_sawtooth_active_box", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.Waveform[(int) WaveForm.Sine], "icn_retro_small_sinus_box","icn_retro_small_sinus_active_box", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.Waveform[(int) WaveForm.Triangle], "icn_retro_small_triangle_box", "icn_retro_small_triangle_active_box", ref data.TextureNotFound);

            if (data.FX == null)
                data.FX = new ButtonStyleData[7];
            allOK &= EnsureGUIStyleSetup(ref data.FX[(int) FX.None], "icn_retro_effect_none", "icn_retro_effect_none_active", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.FX[(int) FX.FadeIn], "icn_retro_effect_fadein", "icn_retro_effect_fadein_active", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.FX[(int) FX.FadeOut], "icn_retro_effect_fadeout", "icn_retro_effect_fadeout_active", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.FX[(int) FX.Vibrato], "icn_retro_effect_vibrato", "icn_retro_effect_vibrato_active", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.FX[(int) FX.Slide], "icn_retro_effect_slide", "icn_retro_effect_slide_active", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.FX[(int) FX.Drop], "icn_retro_effect_drop", "icn_retro_effect_drop_active", ref data.TextureNotFound);
            allOK &= EnsureGUIStyleSetup(ref data.FX[(int) FX.ARP], "icn_retro_effect_arp", "icn_retro_effect_arp_active", ref data.TextureNotFound);
            
            allOK &= EnsureGUIStyleSetup(ref data.WhiteBoxStyle, Texture2D.whiteTexture, Texture2D.whiteTexture);
            if (allOK)
            {
                data.WhiteBoxStyle.border = new RectOffset {bottom = 2, left = 2, right = 2, top = 2};
                data.WhiteBoxStyle.padding = new RectOffset(5, 5, 2, 2);
            }
            
            return allOK;
        }

        static bool EnsureGUIStyleSetup(ref ButtonStyleData btn, string normalName, string activeName, ref bool texNotFound)
        {
            var allOK = true;

            allOK &= EnsureTextureSetup(ref btn.Normal, normalName, ref texNotFound);
            allOK &= EnsureTextureSetup(ref btn.Active, activeName, ref texNotFound);
            allOK &= EnsureGUIStyleSetup(ref btn.Style, btn.Normal, btn.Active);
            return allOK;
        }

        static bool EnsureGUIStyleSetup(ref GUIStyle style, Texture2D tex, Texture2D activeTex = null,
            Texture2D pressedTex = null)
        {
            if (tex == null)
            {
                style = null;
                return false;
            }

            if (style != null)
                return true;
            
            if (activeTex == null)
                activeTex = tex;
            if (pressedTex == null)
                pressedTex = activeTex;
            
            if (style == null)
                style = new GUIStyle();
            StyleHelpers.InitStyleOnOffPress(style, tex, activeTex, pressedTex, Color.white, Color.black);
            return true;
        }
    }
}