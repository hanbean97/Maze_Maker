using System;
using System.Collections.Generic;
using System.Linq;
using DoItRetro;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using static DoTheSFXAndMusic.AudioPreviewUtility;
using static DoTheSFXAndMusic.Utility;
using static DoItRetro.Utility;
using static DoTheSFXAndMusic.TextureStyleOperations;

namespace DoTheSFXAndMusic
{
    public class DoTheMusicEditorWindow : EditorWindow
    {
        static DoTheMusicEditorWindow m_currentSampler;

        #region constants

        const string k_title = "Do the Music";
        
        const int k_pixelPerPartTrack = 32;

        const int k_partsMaxWidth = 512;
        const int k_volumePixelPerPartTrack = 4;

        const string k_partsAreaControl = nameof(RetroSFX) + "MusicParts";
        const string k_trackAreaControl = nameof(RetroSFX) + "MusicTrack";

        const string k_partGo = "Part";
        const string k_musicTrackGo = "MusicTrack";

        enum View
        {
            MusicParts,
            MusicTracks
        }

        #endregion

        #region Members

        RetroMusicConfig m_musicConfig;

        Texture2D m_waveTex;

        AudioClip m_clip;
        // int m_lastIdxMin;
        // int m_lastIdxMax;
        // int m_selectionStart;

        float[] m_samples;

        Texture2D m_musicPartTexture;

        RetroMusic m_selectedMusic;
        RetroMusicPart m_selectedMusicPart;
        RetroSFX m_selectedSFX;

        Vector2 m_mainScrollPos;
        Vector2 m_musicPartListScrollPos;
        Vector2 m_sfxListScrollPos;
        Vector2 m_trackListScrollPos;
        Vector2 m_musicArrangementPos;

        bool m_partLoop;

        readonly List<int> m_selectedSFXSnippets = new List<int>();
        readonly List<int> m_selectedPartRefsInMusicTrack = new List<int>();

        View m_view;

        bool m_editPartName;
        bool m_editMusicTrackName;
        Rect m_musicArrangementRect;
        Vector2 m_lastMousePos;

        bool m_lastDirty;
        int m_clearFocusNextFrame;
        
        bool m_windowWasFocus;
        #endregion

        #region Properties
        bool IsSelectionTool => m_selectedSFX == null;

        bool ValidMusicPartSelection => m_musicConfig!= null &&
            m_selectedMusicPart != null && m_musicConfig.MusicParts.Contains(m_selectedMusicPart);

        bool ValidMusicTrackSelection => m_musicConfig!= null && m_selectedMusic != null && m_musicConfig.MusicTracks.Contains(m_selectedMusic);
        bool ValidSFXSelection => m_musicConfig!= null && m_selectedSFX != null 
                                                       && m_musicConfig.SFXConfig!= null && m_musicConfig.SFXConfig.Contains(m_selectedSFX);
        #endregion

        #region Unity Hooks

        [MenuItem("Tools/Do the Music")]
        public static DoTheMusicEditorWindow Open()
        {
            var window = GetWindow<DoTheMusicEditorWindow>(typeof(DoTheSFXEditorWindow), typeof(SceneView));
            window.titleContent = new GUIContent(k_title);
            window.Focus();
            return window;
        }

        void OnEnable() => Undo.undoRedoPerformed += UndoPerformed;

        void OnDisable() => Undo.undoRedoPerformed -= UndoPerformed;

        void UndoPerformed()
        {
            UpdatePartsTexture();
            UpdateNameIndices();
        }

        void OnGUI()
        {
            m_currentSampler = this;
            CustomCursor();

            if (!AllTexturesSetup(ref RetroMusicSFXStyles))
                return;

            SelectConfigGUI();
            if (m_musicConfig == null)
                return;

            UpdateWindowFocus();

            var dirty = EditorUtility.IsDirty(m_musicConfig);
            if (m_lastDirty != dirty)
                UpdateTitleDirty(dirty);
            
            MusicConfigGUI();
            MainDataGUI();
            DrawWaveTexture();
            UpdateKeys();
            
            if (m_clearFocusNextFrame > 0)
                ClearSelectionAndFocus();
            
            Repaint();
        }

        void UpdateWindowFocus()
        {
            var window = focusedWindow;
            var isFocus = this == window;
            if (isFocus == m_windowWasFocus) 
                return;
            m_windowWasFocus = isFocus;
            
            UpdateNameIndices();
        }

        void UpdateTitleDirty(bool dirty)
        {
            m_lastDirty = dirty;
            var titleStr = dirty ? $"{k_title} *" : k_title;
            titleContent = new GUIContent(titleStr);
        }

        void Update() => CleanupAudioOnDone();

        void OnDestroy() => DestroyAudio();

        #endregion

        #region Shortcuts
        [Shortcut(nameof(DoTheMusicEditorWindow) + ".SwitchView", typeof(DoTheMusicEditorWindow), KeyCode.Tab)]
        public static void SwitchView(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.SwitchView();
        }

        [Shortcut(nameof(DoTheMusicEditorWindow) + ".ReduceDuration", typeof(DoTheMusicEditorWindow), KeyCode.Period)]
        public static void ReduceDuration(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.ModPartDuration(-1, 1);
        }
        
        [Shortcut(nameof(DoTheMusicEditorWindow) + ".IncreaseDuration", typeof(DoTheMusicEditorWindow), KeyCode.Comma)]
        public static void IncreaseDuration(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.ModPartDuration(1, 1);
        }

        [Shortcut(nameof(DoTheMusicEditorWindow) + ".ReduceDurationFast", typeof(DoTheMusicEditorWindow), KeyCode.Period,
            ShortcutModifiers.Shift)]
        public static void ReduceDurationFast(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.ModPartDuration(-1, 4);
        }

        [Shortcut(nameof(DoTheMusicEditorWindow) + ".IncreaseDurationFast", typeof(DoTheMusicEditorWindow), KeyCode.Comma,
            ShortcutModifiers.Shift)]
        public static void IncreaseDurationFast(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.ModPartDuration(1, 4);
        }

        [Shortcut(nameof(DoTheMusicEditorWindow) + ".Play", typeof(DoTheMusicEditorWindow), KeyCode.Space)]
        public static void Play(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.StopOrPlay();
        }
        
        [Shortcut(nameof(DoTheMusicEditorWindow) + ".EditNextElement", typeof(DoTheMusicEditorWindow), KeyCode.Equals)]
        public static void EditNextElement(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.EditNext(1);
        }
        
        [Shortcut(nameof(DoTheMusicEditorWindow) + ".EditPrevElement", typeof(DoTheMusicEditorWindow), KeyCode.Minus)]
        public static void EditPrevElement(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.EditNext(-1);
        }

        [Shortcut(nameof(DoTheMusicEditorWindow) + ".EditNextElementJump4", typeof(DoTheMusicEditorWindow), KeyCode.Equals,
            ShortcutModifiers.Shift)]
        public static void EditNextElementFast(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.EditNext(4);
        }

        [Shortcut(nameof(DoTheMusicEditorWindow) + ".EditPrevElementJump4", typeof(DoTheMusicEditorWindow), KeyCode.Minus,
            ShortcutModifiers.Shift)]
        public static void EditPrevElementFast(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.EditNext(-4);
        }
        
        [Shortcut(nameof(DoTheMusicEditorWindow) + ".DrawSelectionNextElement", typeof(DoTheMusicEditorWindow), KeyCode.LeftBracket)]
        public static void DrawSelectionNextElement(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.DrawSelectionNext(-1);
        }
        
        [Shortcut(nameof(DoTheMusicEditorWindow) + ".DrawSelectionPrevElement", typeof(DoTheMusicEditorWindow), KeyCode.RightBracket)]
        public static void DrawSelectionPrevElement(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.DrawSelectionNext(1);
        }

        [Shortcut(nameof(DoTheMusicEditorWindow) + ".DrawSelectionNextElementJump4", typeof(DoTheMusicEditorWindow), KeyCode.LeftBracket,
            ShortcutModifiers.Shift)]
        public static void DrawSelectionNextElementFast(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.DrawSelectionNext(4);
        }

        [Shortcut(nameof(DoTheMusicEditorWindow) + ".DrawSelectionPrevElementJump4", typeof(DoTheMusicEditorWindow), KeyCode.RightBracket,
            ShortcutModifiers.Shift)]
        public static void DrawSelectionPrevElementFast(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.DrawSelectionNext(-4);
        }
        
        #endregion
        
        #region GUI
        void MainDataGUI()
        {
            if (m_musicConfig.SFXConfig == null)
                return;
            
            if (m_view == View.MusicParts)
                MusicPartsGUI();
            else
                MusicTrackGUI();

            // m_musicConfig.LimitVolume = EditorGUILayout.Slider("Limit Volume", m_musicConfig.LimitVolume, 
            //     0.2f, 2f, GUILayout.Width(350f));
        }

        void MusicPartsGUI()
        {
            // using var scrollScope = new GUILayout.ScrollViewScope(m_mainScrollPos);
            // m_mainScrollPos = scrollScope.scrollPosition;
            using (var hs1 = new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                ConfigMusicPartListGUI();

                if (!ValidMusicPartSelection)
                    return;

                SFXListGUI();
                PartDataGUI();
            }
        }

        void MusicTrackGUI()
        {
            using (var hs2 = new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                ConfigMusicTrackListGUI();

                if (!ValidMusicTrackSelection)
                    return;

                ConfigMusicPartListGUI();
                MusicDataGUI();
            }
        }

        void MusicConfigGUI()
        {
            using (var hs = new GUILayout.HorizontalScope(EditorStyles.helpBox))
            using (var ccs = new EditorGUI.ChangeCheckScope())
            {
                if (m_musicConfig == null)
                    return;

                var viewStyle = m_view == View.MusicParts
                    ? RetroMusicSFXStyles.ViewParts.Style
                    : RetroMusicSFXStyles.ViewTrack.Style;
                
                // $"View [{m_view}]"
                using (new EditorGUI.DisabledScope(m_musicConfig.SFXConfig == null))
                {
                    if (GUILayout.Button(GUIContent.none, viewStyle, GUILayout.Width(53f), GUILayout.Height(20f)))
                        SwitchView();
                }
                GUILayout.Space(15);
                
                EditorGUILayout.LabelField("╚ SFX config", GUILayout.Width(100));
                var newSfxConfig = (RetroSFXConfig) EditorGUILayout.ObjectField(GUIContent.none, m_musicConfig.SFXConfig,
                    typeof(RetroSFXConfig), false, GUILayout.Width(200));
               
                if(m_musicConfig.SFXConfig== null)
                    EditorGUILayout.HelpBox($"<- Please choose a SFX config! You can create and Edit one via '{DoTheSFXEditorWindow.MenuPath}'", MessageType.Info);
                if (ccs.changed)
                {
                    RecordUndoDirty(m_musicConfig, "SFX Config changed");
                    m_musicConfig.SetSFXConfig(newSfxConfig);
                }
            }
        }

        void SwitchView()
        {
            // hack, for Tab-Key also switches focus of buttons. After switch view it should be possible to press space
            // to play track or part immediately. We need to remove focus after 3 frames for this to work.
            m_clearFocusNextFrame = 3;
            m_view = (View) (((int) m_view + 1) % 2);
        }

        void DrawWaveTexture() => GUILayout.Box(m_waveTex);

        #region MuiscParts
        void ConfigMusicPartListGUI()
        {
            var deleteOrEditIdx = -1;
            using (var scv = new EditorGUILayout.ScrollViewScope(m_musicPartListScrollPos, GUILayout.Width(200f)))
            using (var vs = new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(180f),
                GUILayout.MinHeight(100)))
            {
                m_musicPartListScrollPos = scv.scrollPosition;

                if (m_view == View.MusicTracks) 
                    ConfigMusicPartListGUI_SelectButton();

                GUILayout.Label($"Music Parts: ", EditorStyles.boldLabel);

                ConfigMusicPartListGUI_List(ref deleteOrEditIdx);
                ConfigMusicPartListGUI_Add();

                if (deleteOrEditIdx != -1)
                {
                    if (m_view == View.MusicTracks)
                    {
                        m_view = View.MusicParts;
                        m_selectedMusicPart = m_musicConfig.MusicParts[deleteOrEditIdx];
                    }
                    else ConfigMusicPartListGUI_Delete(deleteOrEditIdx);
                }
                
            }
        }
        
        void ConfigMusicPartListGUI_List(ref int deleteOrEditIdx)
        {
            for (var i = 0; i < m_musicConfig.MusicParts.Count; i++)
            {
                var music = m_musicConfig.MusicParts[i];

                using (var hs = new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label($"{i}: ", GUILayout.Width(20f));

                    var selected = m_selectedMusicPart == music;
                    // using (new ColorScope(selected ? music.Color : 0.2f * music.Color))
                    if (m_editPartName && selected)
                    {
                        using (var ccs = new EditorGUI.ChangeCheckScope())
                        {
                            GUI.SetNextControlName("MusicName");
                            var sfxName = EditorGUILayout.DelayedTextField(music.Name, GUILayout.Width(120));
                            if (ccs.changed)
                            {
                                RecordUndoDirty(m_musicConfig, "Music Part Name changed");
                                music.Name = sfxName;
                            }
                        }
                    }
                    else if (selected != GUILayout.Toggle(selected, music.Name, RetroMusicSFXStyles.Button.Style,
                        GUILayout.Height(20f), GUILayout.Width(120)))
                    {
                        if (selected)
                        {
                            m_editMusicTrackName = false;
                            m_editPartName = true;
                        }
                        else SelectMusicPart(i);
                    }

                    music.Color = EditorGUILayout.ColorField(GUIContent.none, music.Color,
                        false, false, false, GUILayout.Width(10));
                    music.Color.a = 1f;

                    var style = m_view == View.MusicTracks
                        ? RetroMusicSFXStyles.Edit.Style
                        : RetroMusicSFXStyles.Delete.Style;
                    if (GUILayout.Button(GUIContent.none, style, GUILayout.Height(20f),
                        GUILayout.Width(20f)))
                        deleteOrEditIdx = i;
                }
            }
        }
        
        void ConfigMusicPartListGUI_Add()
        {
            if (!GUILayout.Button(GUIContent.none, RetroMusicSFXStyles.Add.Style, GUILayout.Height(20f),
                GUILayout.Width(20f))) 
                return;
            
            GUI.FocusControl("");
            RecordUndoDirty(m_musicConfig, "Add Music Part");
            m_musicConfig.AddPart();
            if (!ValidMusicTrackSelection)
                SelectMusicPart(0);

            UpdateTrackPartNameIndices();
        }
        
        void ConfigMusicPartListGUI_Delete(int deleteIdx)
        {
            if (m_musicConfig.MusicParts[deleteIdx] == m_selectedMusicPart)
                SelectMusicPart(Mathf.Clamp(deleteIdx, 0, m_musicConfig.MusicParts.Count - 1));

            GUI.FocusControl("");
            RecordUndoDirty(m_musicConfig, "Music Part deleted");
            m_musicConfig.MusicParts.RemoveAt(deleteIdx);

            OnPartRemoved(deleteIdx);
            UpdateTrackPartNameIndices();
        }

        void ConfigMusicPartListGUI_SelectButton()
        {
            var selectSelected = !ValidMusicPartSelection;
            var icon = selectSelected ? RetroMusicSFXStyles.SelectIcnLight : RetroMusicSFXStyles.SelectIcnDark;
            if (GUILayout.Toggle(selectSelected, new GUIContent("Select", icon),
                RetroMusicSFXStyles.Button.Style, GUILayout.Height(20f)))
                m_selectedMusicPart = null;
        }
        #endregion

        #region SFX
        void SFXListGUI()
        {
            if (m_musicConfig.SFXConfig == null)
                return;

            var editIdx = -1;
            using (var scv = new EditorGUILayout.ScrollViewScope(m_sfxListScrollPos, GUILayout.Width(200f)))
            using (var vs = new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(180f),
                GUILayout.MinHeight(100)))
            {
                m_sfxListScrollPos = scv.scrollPosition;

                SFXListGUI_Select();

                GUILayout.Label($"SFX: ", EditorStyles.boldLabel);

                SFXListGUI_List(ref editIdx);

                if (editIdx != -1) 
                    SFXListGUIEdit(editIdx);
            }
        }
        void SFXListGUI_Select()
        {
            var selectSelected = !ValidSFXSelection;
            var icon = selectSelected ? RetroMusicSFXStyles.SelectIcnLight : RetroMusicSFXStyles.SelectIcnDark;
            if (GUILayout.Toggle(selectSelected, new GUIContent("Select", icon), RetroMusicSFXStyles.Button.Style,
                GUILayout.Height(20f)))
                m_selectedSFX = null;
        }
        void SFXListGUI_List(ref int editIdx)
        {
            for (var i = 0; i < m_musicConfig.SFXConfig.Count; i++)
            {
                var sfx = m_musicConfig.SFXConfig[i];

                using (var hs = new EditorGUILayout.HorizontalScope())
                {
                    var active = m_selectedSFX == sfx;

                    GUILayout.Label($"{i}: ", GUILayout.Width(20f));
                    if (active != GUILayout.Toggle(active, sfx.Name, RetroMusicSFXStyles.Button.Style,
                        GUILayout.Height(20f), GUILayout.Width(120)))
                        SelectSFX(i);

                    if (GUILayout.Button(GUIContent.none, RetroMusicSFXStyles.Edit.Style, GUILayout.Height(20f),
                        GUILayout.Width(20f)))
                        editIdx = i;
                }
            }
        }
        void SFXListGUIEdit(int editIdx)
        {
            var sfxWindow = GetWindow<DoTheSFXEditorWindow>(typeof(DoTheMusicEditorWindow));
            sfxWindow.titleContent = new GUIContent("Retro SFX Editor");
            sfxWindow.Select(m_musicConfig.SFXConfig, editIdx);
            sfxWindow.Focus();
        }
        #endregion

        #region MusicTracks
        void ConfigMusicTrackListGUI()
        {
            var deleteIdx = -1;
            using (var scv = new EditorGUILayout.ScrollViewScope(m_trackListScrollPos, GUILayout.Width(200f)))
            using (var vs = new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(180f),
                GUILayout.MinHeight(100)))
            {
                m_trackListScrollPos = scv.scrollPosition;
                GUILayout.Label($"Music Tracks: ", EditorStyles.boldLabel);

                ConfigMusicTrackListGUI_List(ref deleteIdx);
                ConfigMusicTrackListGUI_Add();

                if (deleteIdx != -1) 
                    ConfigMusicTrackListGUI_Delete(deleteIdx);
            }
        }

        void ConfigMusicTrackListGUI_Delete(int deleteIdx)
        {
            if (m_musicConfig.MusicTracks[deleteIdx] == m_selectedMusic)
                SelectMusic(Mathf.Clamp(deleteIdx, 0, m_musicConfig.MusicTracks.Count - 1));

            GUI.FocusControl("");
            RecordUndoDirty(m_musicConfig, "Music deleted");
            m_musicConfig.MusicTracks.RemoveAt(deleteIdx);
        }

        void ConfigMusicTrackListGUI_Add()
        {
            if (!GUILayout.Button(GUIContent.none, RetroMusicSFXStyles.Add.Style, GUILayout.Height(20f),
                GUILayout.Width(20f))) 
                return;
            
            GUI.FocusControl("");
            RecordUndoDirty(m_musicConfig, "Add Music");
            m_musicConfig.AddMusicTrack();
            if (!ValidMusicTrackSelection)
                SelectMusic(0);
        }

        void ConfigMusicTrackListGUI_List(ref int deleteIdx)
        {
            for (var i = 0; i < m_musicConfig.MusicTracks.Count; i++)
            {
                var music = m_musicConfig.MusicTracks[i];

                using (var hs = new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label($"{i}: ", GUILayout.Width(20f));
                    var selected = m_selectedMusic == music;
                    if (m_editMusicTrackName && m_selectedMusic == music)
                    {
                        using (var ccs = new EditorGUI.ChangeCheckScope())
                        {
                            GUI.SetNextControlName("SFXName");
                            var sfxName = EditorGUILayout.DelayedTextField(music.Name, GUILayout.Width(120));
                            if (ccs.changed)
                            {
                                RecordUndoDirty(m_musicConfig, "Music Name changed");
                                music.Name = sfxName;
                            }
                        }
                    }

                    else if (selected != GUILayout.Toggle(selected, music.Name, RetroMusicSFXStyles.Button.Style,
                        GUILayout.Width(120)))
                    {
                        if (selected)
                        {
                            m_editPartName = false;
                            m_editMusicTrackName = true;
                        }
                        else SelectMusic(i);
                    }

                    if (GUILayout.Button(GUIContent.none, RetroMusicSFXStyles.Delete.Style, GUILayout.Height(20f),
                        GUILayout.Width(20f)))
                        deleteIdx = i;
                }
            }
        }
        #endregion

        void SelectConfigGUI()
        {
            var prevConfig = m_musicConfig;
            GUILayout.Space(5);

            using (var hs = new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Space(5);
                EditorGUILayout.LabelField("Music Config:", GUILayout.Width(75));
                var musicConfig = (RetroMusicConfig) EditorGUILayout.ObjectField(m_musicConfig, typeof(RetroMusicConfig),
                    false, GUILayout.Width(200));

                if (musicConfig != m_musicConfig)
                {
                    m_musicConfig = musicConfig;
                    UpdateNameIndices();
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Create new", GUILayout.Width(90)))
                {
                    var path = EditorUtility.SaveFilePanelInProject("Save Retro asset", "RetroMusic.asset", "asset",
                        "Please choose a location for the new asset");

                    m_musicConfig = CreateInstance<RetroMusicConfig>();
                    AssetDatabase.CreateAsset(m_musicConfig, path);
                    AssetDatabase.SaveAssets();
                }

                GUILayout.Space(5);

                if (prevConfig != m_musicConfig)
                    SelectMusic(0);
            }
        }

        #region PartData
        
        void PartDataGUI()
        {
            if (m_musicConfig == null || m_musicConfig.MusicParts.Count <= 0)
                return;
            if (!ValidMusicPartSelection)
                return;

            using (var vs = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                PartDataGUI_Header();

                UpdatePartsTextureIfNeeded();

                DrawTexture_GetRectMinMax(k_partsAreaControl, m_musicPartTexture, new Vector2(2 * 1, 0),
                    out var partsRect, out var partsMinPos, out var partsMaxPos);
                DrawSelectedSFXLabels(partsRect);
                DrawRemoveTrackButtons(partsRect);
                var mp = Event.current.mousePosition;

                GetGridPos_FloorXCeilY(mp, partsMinPos,
                    new Vector2Int(m_musicPartTexture.width, m_musicPartTexture.height), m_selectedMusicPart.Duration, m_selectedMusicPart.Tracks,
                    out var partsGridPosX, out var partsGridPosY);

                var xGrid = Mathf.FloorToInt((float) partsGridPosX / m_selectedMusicPart.TimeGrid) *
                            m_selectedMusicPart.TimeGrid;

                var eventType = Event.current.type;
                var click = Event.current.isMouse && Event.current.button == 0;
                if (click)
                    MouseDrawParts(eventType, xGrid, m_selectedMusicPart.Tracks - (1 + partsGridPosY));

                DrawPlayPositionMarker(partsRect, k_partGo);

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(k_partsMaxWidth + 6);
                    PartDataGUI_AddTrack();
                }
            }
        }

        void PartDataGUI_Export()
        {
            if (!GUILayout.Button(new GUIContent("", "Export Part"), RetroMusicSFXStyles.Export.Style,
                GUILayout.Height(20f), GUILayout.Width(20f))) 
                return;
            
            var partNamePath =
                EditorUtility.SaveFilePanel("Save Part as wav", "", m_selectedMusicPart.Name, "wav");

            if (!string.IsNullOrEmpty(partNamePath))
                ExportPart(partNamePath);
        }

        void PartDataGUI_Header()
        {
            using (var cs = new EditorGUI.ChangeCheckScope())
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    // EditorGUIUtility.labelWidth = 100f;
                    PartDataGUI_Duration();

                    GUILayout.Space(107f);
                    var partLoop = EditorGUILayout.Toggle(new GUIContent("", "Loop"), m_partLoop,
                        RetroMusicSFXStyles.Loop.Style,
                        GUILayout.Height(20f), GUILayout.Width(20f));

                    if (partLoop != m_partLoop)
                    {
                        GUI.FocusControl("");
                        m_partLoop = partLoop;
                    }

                    var playingPart = AudioObject != null && AudioObject.name.Contains(k_partGo) &&
                                      AudioObject.isPlaying;
                    
                    var style = !playingPart
                        ? RetroMusicSFXStyles.Play.Style
                        : RetroMusicSFXStyles.Pause.Style;
                    
                    if (GUILayout.Button(GUIContent.none, style, GUILayout.Height(20f),
                        GUILayout.Width(20f)))
                        PauseOrPlayPart();
                    
                    using (new EditorGUI.DisabledScope(AudioObject == null))
                    {
                        if (GUILayout.Button(GUIContent.none, RetroMusicSFXStyles.Stop.Style, GUILayout.Height(20f),
                            GUILayout.Width(20f)))
                            Stop();
                    }
                    
                    GUILayout.Space(50f);
                    PartDataGUI_Export();
                }

                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    PartDataGUI_Grid();
                    GUILayout.Space(15f);
                    PartDataGUI_Volume();

                    GUILayout.Space(107f);
                }

                if (cs.changed)
                    UpdatePartsTexture();
            }
        }

        void PartDataGUI_AddTrack()
        {
            using (new EditorGUI.DisabledScope(m_selectedMusicPart.Tracks >= 8))
            {
                var addTrackContent = new GUIContent("", "Add Track");
                if (!GUILayout.Button(addTrackContent, RetroMusicSFXStyles.Add.Style, GUILayout.Height(20f),
                    GUILayout.Width(20f)))
                    return;
                
                GUI.FocusControl("");
                RecordUndoDirty(m_musicConfig, "Added Track");
                m_selectedMusicPart.Tracks++;
                UpdateTrackMutedArray(m_selectedMusicPart);
            }
        }

        void PartDataGUI_Grid()
        {
            var gridContent = new GUIContent("Time-Grid: ", RetroMusicSFXStyles.TimeGridIcon, "Time-Grid");
            EditorGUILayout.LabelField(gridContent, GUILayout.Height(20f), GUILayout.Width(100f));

            GUI.SetNextControlName("TimeGrid");
            var grid = EditorGUILayout.IntSlider(GUIContent.none, m_selectedMusicPart.TimeGrid, 1, 255,
                GUILayout.Height(20f), GUILayout.Width(300f));

            if (grid == m_selectedMusicPart.TimeGrid) 
                return;
            
            RecordUndoDirty(m_musicConfig, "TimeGrid changed");
            m_selectedMusicPart.TimeGrid = grid;
        }

        void PartDataGUI_Volume()
        {
            var gridContent = new GUIContent("Volume: ");
            EditorGUILayout.LabelField(gridContent, GUILayout.Height(20f), GUILayout.Width(75f));

            if (m_selectedMusicPart.Volume == 0f)
                m_selectedMusicPart.Volume = 1f;
            
            GUI.SetNextControlName("Volume");
            var vol = EditorGUILayout.Slider(GUIContent.none, m_selectedMusicPart.Volume, 0.0001f, 3,
                GUILayout.Height(20f), GUILayout.Width(150f));

            if (Math.Abs(vol - m_selectedMusicPart.Volume) < float.Epsilon) 
                return;
            
            RecordUndoDirty(m_musicConfig, "Volume changed");
            m_selectedMusicPart.Volume = vol;
        }
        
        void PartDataGUI_Duration()
        {
            var durContent = new GUIContent("Duration: ", RetroMusicSFXStyles.DurationIcon);
            EditorGUILayout.LabelField(durContent, GUILayout.Height(20f), GUILayout.Width(100f));

            GUI.SetNextControlName("Duration");
            var dur = EditorGUILayout.IntSlider(GUIContent.none, m_selectedMusicPart.Duration, 1, 255,
                GUILayout.Height(20f), GUILayout.Width(300f));

            if (dur == m_selectedMusicPart.Duration)
                return;
            
            RecordUndoDirty(m_musicConfig, "Duration changed");
            m_selectedMusicPart.Duration = dur;
        }
        #endregion

        void DrawRemoveTrackButtons(Rect partsRect)
        {
            var h = partsRect.height / m_selectedMusicPart.Tracks;
            var deleteIdx = -1;
            for (var i = 0; i < m_selectedMusicPart.Tracks; i++)
            {
                var x = partsRect.max.x;
                var y = partsRect.y + 5 + i * h;

                using (new EditorGUI.DisabledScope(m_selectedMusicPart.Tracks<=1))
                {
                    if (GUI.Button(new Rect(x, y, 20, 20), GUIContent.none, RetroMusicSFXStyles.Delete.Style))
                        deleteIdx = i;
                }

                UpdateTrackMutedArray(m_selectedMusicPart);

                var isMute = m_selectedMusicPart.TrackMuted[i];
                var style = isMute ? RetroMusicSFXStyles.Mute.Style : RetroMusicSFXStyles.Sound.Style; 
                var newMute = GUI.Toggle(new Rect(x + 25, y, 20, 20), 
                    m_selectedMusicPart.TrackMuted[i], GUIContent.none, style);
                
                if (newMute != m_selectedMusicPart.TrackMuted[i])
                {
                    m_selectedMusicPart.TrackMuted[i] = newMute;
                    UpdatePartsTexture();
                }
                
                GUI.Label(new Rect(x + 50, y, 75, 20), $"Track {i}");
            }
            if (deleteIdx != -1)
                DeleteTrack(deleteIdx);
        }
        
        static void DrawPlayPositionMarker(Rect rect, string audioGoName)
        {
            if (AudioObject == null)
                return;
            if (AudioObject.timeSamples >= AudioObject.clip.samples)
                return;
            if (!AudioObject.name.Contains(audioGoName))
                return;

            var samples = AudioObject.timeSamples;
            var playPos = (float) samples / AudioObject.clip.samples;

            // GUI.contentColor = Color.white;
            var plPos = rect.min + playPos * rect.width * Vector2.right;
            var height = rect.height;
            EditorGUITools.DrawRect(new Rect(plPos, new Vector2(2, height)), Color.white);
        }

        static void DrawPlayPositionMarker(int start, int dur, string audioGoName)
        {
            if (AudioObject == null)
                return;
            if (AudioObject.timeSamples >= AudioObject.clip.samples)
                return;
            if (!AudioObject.name.Contains(audioGoName))
                return;

            var end = start + dur;
            var sampleStart = start * QuarterSampleRate;
            var sampleEnd = end * QuarterSampleRate;
            var r = GUILayoutUtility.GetLastRect();
            var w = dur * 4;

            if (AudioObject.timeSamples < sampleStart
                || AudioObject.timeSamples > sampleEnd)
                return;

            var samples = sampleEnd - sampleStart;
            var playPos = (float) (AudioObject.timeSamples - sampleStart) / samples;
            var plPos = r.min + playPos * w * Vector2.right;
            EditorGUITools.DrawRect(new Rect(plPos, new Vector2(2, 25)), Color.white);
        }

        void DrawSelectedSFXLabels(Rect partsTex)
        {
            if (!ValidMusicPartSelection || m_selectedMusicPart.Snippets.Count <= 0)
                return;

            var xSteps = partsTex.width / m_selectedMusicPart.Duration;
            var ySteps = partsTex.height / m_selectedMusicPart.Tracks;

            foreach (var i in m_selectedSFXSnippets)
            {
                var snip = m_selectedMusicPart.Snippets[i];
                var snipOffX = snip.TimePosition * xSteps;
                var snipOffY = snip.TrackPosition * ySteps;

                GUI.Label(new Rect(partsTex.x + snipOffX,
                        partsTex.y + snipOffY - 15, 100, 50),
                    $" {snip.SFXName}");
            }
        }

        void MouseDrawParts(EventType eventType, int time, int track)
        {
            if (time < 0 || time >= m_selectedMusicPart.Duration
                         || track < 0 || track >= m_selectedMusicPart.Tracks)
                return;

            GUI.FocusControl(k_partsAreaControl);
            Event.current.Use();

            if (!IsSelectionTool)
            {
                RecordUndoDirty(m_musicConfig, "Parts Paint");

                var newSnip = new RetroMusicSFXSnippet
                {
                    SFXIdx = m_musicConfig.SFXConfig.IndexOf(m_selectedSFX),
                    SFXName = m_selectedSFX.Name,
                    TrackPosition = track,
                    TimePosition = time
                };
                var newSnipEnd = newSnip.TimePosition + m_selectedSFX.Duration;

                AddNewSnippetRemoveOverlapping(newSnip, newSnipEnd);

                // m_musicConfig.SoundData[pitchWaveGridPosX].Pitch = PitchStart + pitchWaveGridPosY;
                // m_selectedSFX.SoundData[pitchWaveGridPosX].WaveForm = (int) m_waveForm;
                // if (m_selectedSFX.SoundData[pitchWaveGridPosX].Volume == 0)
                //     m_selectedSFX.SoundData[pitchWaveGridPosX].Volume = 3;

                // m_lastIdxMin = pitchWaveGridPosX;
                // m_lastIdxMax = pitchWaveGridPosX;
            }
            else
                MouseSelectSFXSnippets(eventType, time, track);

            // m_lastClicked = PaintArea.PitchWaveform;

            UpdatePartsTexture();
        }

        void MouseSelectSFXSnippets(EventType eventType, int time, int track)
        {
            for (var i = 0; i < m_selectedMusicPart.Snippets.Count; i++)
            {
                var snip = m_selectedMusicPart.Snippets[i];
                if (snip.TrackPosition != track)
                    continue;

                GetSFXTimings(snip, out var start, out var dur, out var end);

                if (start > time || end <= time)
                    continue;

                if (!Event.current.shift)
                    m_selectedSFXSnippets.Clear();

                m_selectedPartRefsInMusicTrack.Clear();
                if (!m_selectedSFXSnippets.Contains(i))
                    m_selectedSFXSnippets.Add(i);
            }
        }
        
        void MusicDataGUI()
        {
            if (m_musicConfig == null || m_musicConfig.MusicTracks.Count <= 0)
                return;
            if (!ValidMusicTrackSelection)
                return;

            using (var vs = new GUILayout.VerticalScope(GUILayout.Width(700)))
            {
                GUILayout.Space(0f);
                if (Event.current.type == EventType.Repaint)
                {
                    m_musicArrangementRect = GUILayoutUtility.GetLastRect();
                    m_musicArrangementRect.y += 30;
                }            
                
                MusicDataHeaderGUI();
                
                // GUILayout.Space(50f);
            }

            MusicDataGUI_Draw();
            
            using (var vs = new GUILayout.VerticalScope(GUILayout.Width(700)))
            {
                GUILayout.Space(65f);
   
                if (m_selectedPartRefsInMusicTrack.Count == 1 &&
                    m_selectedPartRefsInMusicTrack[0] < m_selectedMusic.Parts.Count)
                    MusicDataGUI_Selected();
            }

            // weird unity-bug: 
            EditorGUILayout.EndHorizontal();
        }

        void MusicDataHeaderGUI()
        {
            var vs = new GUILayout.HorizontalScope(EditorStyles.helpBox);
            MusicDataLoopGUI();
            
            var playingTrack = AudioObject != null && AudioObject.name.Contains(k_musicTrackGo) &&
                               AudioObject.isPlaying;
                
            var style = !playingTrack
                ? RetroMusicSFXStyles.Play.Style
                : RetroMusicSFXStyles.Pause.Style;
            
            if (GUILayout.Button(GUIContent.none, style, GUILayout.Width(20f),
                GUILayout.Height(20f)))
                PauseOrPlayMusicTrack();
              
            using (new EditorGUI.DisabledScope(AudioObject == null))
            {
                if (GUILayout.Button(GUIContent.none, RetroMusicSFXStyles.Stop.Style, GUILayout.Height(20f),
                    GUILayout.Width(20f)))
                    Stop();
            }
            
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("", "Export Track"), RetroMusicSFXStyles.Export.Style,
                GUILayout.Height(20f), GUILayout.Width(20f)))
            {
                var namePath = EditorUtility.SaveFilePanel("Save Track as wav", "", m_selectedMusic.Name, "wav");

                if (!string.IsNullOrEmpty(namePath))
                    ExportTrack(namePath);
            }
        }
        
        void MusicDataGUI_Selected()
        {
            var selectedPartIdx = m_selectedPartRefsInMusicTrack[0];
            GUILayout.Space(20);
            GUILayout.Label("Selected:");
            using (var hs = new GUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                var partRef = m_selectedMusic.Parts[selectedPartIdx];
                var partIdx = EditorGUILayout.Popup(partRef.PartIdx,
                    m_musicConfig.MusicParts.Select(m => m.Name).ToArray());
                if (partIdx != partRef.PartIdx)
                {
                    GUI.FocusControl("");
                    RecordUndoDirty(m_musicConfig, "Track: Part idx changed");
                    partRef.PartIdx = partIdx;
                }

                if (partRef.PartIdx >= 0 && partRef.PartIdx < m_musicConfig.MusicParts.Count)
                    partRef.PartName = m_musicConfig.MusicParts[partRef.PartIdx].Name;

                m_selectedMusic.Parts[selectedPartIdx] = partRef;

                if (GUILayout.Button(GUIContent.none, RetroMusicSFXStyles.Delete.Style, GUILayout.Height(20f),
                    GUILayout.Width(20f)))
                {
                    GUI.FocusControl("");
                    RecordUndoDirty(m_musicConfig, "Track: Part removed");

                    m_selectedMusic.Parts.RemoveAt(selectedPartIdx);
                    m_selectedPartRefsInMusicTrack.Remove(selectedPartIdx);
                }
            }
        }

        void MusicDataGUI_Draw()
        {
            var click = Event.current.isMouse && Event.current.button == 0;

            using (var ar = new GUILayout.AreaScope(new Rect(m_musicArrangementRect.position.x,
                m_musicArrangementRect.position.y, 700, 70)))
            using (var svs =
                new GUILayout.ScrollViewScope(m_musicArrangementPos, GUILayout.Width(700), GUILayout.Height(70)))
            using (var hs = new GUILayout.HorizontalScope(EditorStyles.helpBox, GUILayout.Height(50)))
            {
                m_musicArrangementPos = svs.scrollPosition;
                GUILayout.Space(10);
                var time = 0;
                
                MusicDataGUI_DrawAddButtons();

                for (var i = 0; i < m_selectedMusic.Parts.Count; i++)
                {
                    // AddPartButtonInMusicArrangement(i);

                    var p = m_selectedMusic.Parts[i];
                    RetroMusicPart part = null;
                    if (p.PartIdx >= 0 && p.PartIdx < m_musicConfig.MusicParts.Count)
                        part = m_musicConfig.MusicParts[p.PartIdx];

                    var partDur = part?.Duration ?? 8;
                    var partCol = part?.Color ?? Color.white;
                    var partName = part?.Name ?? "";
                    
                    var width = partDur * 4;
                    var active = m_selectedPartRefsInMusicTrack.Contains(i);
                    using (new ColorScope(active ? Color.white : new Color(0, 0, 0, 0)))
                        GUILayout.Box("", RetroMusicSFXStyles.WhiteBoxStyle, GUILayout.Width(width),
                            GUILayout.Height(25f));
                    
                    var boxRect = GUILayoutUtility.GetLastRect();
                    
                    using (new ColorScope(Color.black))
                        GUI.Box(new Rect(boxRect.x + 1, boxRect.y + 1, boxRect.width - 2, boxRect.height - 2), "",
                            RetroMusicSFXStyles.WhiteBoxStyle);
                
                    using (new ColorScope(partCol))
                        GUI.Box(new Rect(boxRect.x + 2, boxRect.y + 2, boxRect.width - 4, boxRect.height - 4), "",
                            RetroMusicSFXStyles.WhiteBoxStyle);

                    // using (new ColorScope(active ? Color.yellow : Color.white))
                    GUI.Label(new Rect(boxRect.x + 15, boxRect.y, boxRect.width - 15, boxRect.height), partName);
                    // Debug.Log($"{boxRect} , mouse {Event.current.mousePosition}");
                    if (boxRect.Contains(Event.current.mousePosition) && click)
                    {
                        GUI.FocusControl(k_trackAreaControl);

                        if (!Event.current.shift)
                            m_selectedPartRefsInMusicTrack.Clear();

                        ChangeMusicPartAtIdxInSelectedTrack(i);

                        m_selectedSFXSnippets.Clear();
                        m_selectedPartRefsInMusicTrack.Add(i);
                    }

                    DrawPlayPositionMarker(time, partDur, k_musicTrackGo);
                    time += partDur;
                }

                // AddPartButtonInMusicArrangement(m_selectedMusic.Parts.Count);
            }
        }

        void CustomCursor()
        {
            if (Event.current != null) 
                m_lastMousePos = Event.current.mousePosition;

            if (m_view == View.MusicTracks)
            {
                if (!ValidMusicPartSelection) 
                    return;
            }
            else if (m_view == View.MusicParts)
            {
                if (!ValidSFXSelection) 
                    return;
            }
            
            Cursor.SetCursor(RetroMusicSFXStyles.EditCursor, Vector2.zero, CursorMode.Auto);
            EditorGUIUtility.AddCursorRect(new Rect(m_lastMousePos, 0.5f * Vector2.one),
                MouseCursor.CustomCursor);
        }

        void MusicDataGUI_DrawAddButtons()
        {
            var lastRect = GUILayoutUtility.GetLastRect();
            var x = lastRect.x + lastRect.width;
            for (var i = 0; i < m_selectedMusic.Parts.Count; i++)
            {
                AddPartButtonInMusicArrangement(i, x);
                var p = m_selectedMusic.Parts[i];
                RetroMusicPart part = null;
                if (p.PartIdx >= 0 && p.PartIdx < m_musicConfig.MusicParts.Count)
                    part = m_musicConfig.MusicParts[p.PartIdx];

                var partDur = part?.Duration ?? 8;
                x += partDur * 4;
            }

            AddPartButtonInMusicArrangement(m_selectedMusic.Parts.Count, x);
        }

        void MusicDataLoopGUI()
        {
            var newLoop = EditorGUILayout.Toggle(new GUIContent("", "Loop"), m_selectedMusic.Loop,
                RetroMusicSFXStyles.Loop.Style, GUILayout.Width(20f), GUILayout.Height(20f));

            if (newLoop != m_selectedMusic.Loop)
            {
                GUI.FocusControl("");
                RecordUndoDirty(m_musicConfig, "Loop changed");
                m_selectedMusic.Loop = newLoop;
            }
        }
        
        void AddPartButtonInMusicArrangement(int idx, float x)
        {
            using (new ColorScope(BGColorC))
                GUI.Box(new Rect(x - 1, 6, 2, 30), GUIContent.none,
                    RetroMusicSFXStyles.WhiteBoxStyle);
            
            if (!GUI.Button(new Rect(x -10, 30, 20, 20), GUIContent.none, RetroMusicSFXStyles.Add.Style))
                return;
            
            GUI.FocusControl(k_trackAreaControl);

            var newIdx = 0;
            var newName = "";

            if (m_selectedMusicPart != null)
            {
                newIdx = m_musicConfig.MusicParts.FindIndex(m => m == m_selectedMusicPart);
                newName = m_selectedMusicPart.Name;
            }

            GUI.FocusControl("");
            RecordUndoDirty(m_musicConfig, "Add Part");
            m_selectedSFXSnippets.Clear();

            m_selectedMusic.Parts.Insert(idx, new RetroMusicPartRef {PartIdx = newIdx, PartName = newName});

            m_selectedSFXSnippets.Clear();
            m_selectedPartRefsInMusicTrack.Clear();
            m_selectedPartRefsInMusicTrack.Add(m_selectedMusic.Parts.Count - 1);
        }

        #endregion

        #region GUI Actions
        public void Select(RetroMusicConfig musicConfig)
        {
            GUI.FocusControl("");
            m_musicConfig = musicConfig;
        }

        void ChangeMusicPartAtIdxInSelectedTrack(int i)
        {
            RetroMusicPartRef p = default;
            if (!ValidMusicPartSelection)
                return;

            var idx = m_musicConfig.MusicParts.FindIndex(m => m == m_selectedMusicPart);
            if (idx == -1)
                return;

            GUI.FocusControl("");
            RecordUndoDirty(m_musicConfig, "Track: Part idx changed");

            p.PartName = m_selectedMusicPart.Name;
            p.PartIdx = idx;

            m_selectedMusic.Parts[i] = p;
        }

        void GetSFXTimings(RetroMusicSFXSnippet snippet, out int start, out int duration, out int endPosition)
        {
            start = 0;
            duration = 0;
            endPosition = 0;
            if (snippet.SFXIdx < 0 || snippet.SFXIdx >= m_musicConfig.SFXConfig.Count)
                return;

            var sfx = m_musicConfig.SFXConfig[snippet.SFXIdx];
            start = snippet.TimePosition;
            duration = sfx.Duration;
            endPosition = start + duration;
        }

        void AddNewSnippetRemoveOverlapping(RetroMusicSFXSnippet newSnip, int newSnipEnd)
        {
            var newSnipStart = newSnip.TimePosition;
            if (newSnipStart > m_selectedMusicPart.Duration)
                return;

            m_selectedMusicPart.Snippets.RemoveAll(s =>
                s.TrackPosition == newSnip.TrackPosition && s.TimePosition == newSnip.TimePosition);

            m_selectedMusicPart.Snippets.RemoveAll(s =>
            {
                if (s.TrackPosition != newSnip.TrackPosition)
                    return false;
                GetSFXTimings(s, out var start, out var dur, out var end);
                var before = start < newSnipStart && end <= newSnipStart;
                var after = start >= newSnipEnd && end > newSnipEnd;

                return !before && !after;
            });
            m_selectedMusicPart.Snippets.Add(newSnip);

            var selectIdx = m_selectedMusicPart.Snippets.Count - 1;
            SelectSFXSnippet(selectIdx);
        }

        void SelectSFXSnippet(int selectIdx)
        {
            m_selectedPartRefsInMusicTrack.Clear();
            m_selectedSFXSnippets.Clear();
            m_selectedSFXSnippets.Add(selectIdx);
        }
        void ExportTrack(string path)
        {
            BuildSamples();
            var clip = CreateClip(m_samples, m_selectedMusic.Name);
            SavWav.Save(path, clip);
            AssetDatabase.Refresh();
        }

        void ExportPart(string path)
        {
            BuildSamplesPart();
            var clip = CreateClip(m_samples, m_selectedMusicPart.Name);
            SavWav.Save(path, clip);
            AssetDatabase.Refresh();
        }

        void DeleteTrack(int trackIdx)
        {
            GUI.FocusControl("");
            RecordUndoDirty(m_musicConfig, "Track deleted");
            m_selectedSFXSnippets.Clear();
            for (var j = m_selectedMusicPart.Snippets.Count - 1; j >= 0; j--)
            {
                var snip = m_selectedMusicPart.Snippets[j];
                if (snip.TrackPosition < trackIdx)
                    continue;
                if (snip.TrackPosition == trackIdx)
                {
                    m_selectedMusicPart.Snippets.RemoveAt(j);
                    continue;
                }

                snip.TrackPosition--;
                m_selectedMusicPart.Snippets[j] = snip;
            }

            m_selectedMusicPart.Tracks--;
            UpdateTrackMutedArray(m_selectedMusicPart);

            UpdatePartsTexture();
        }

        void UpdateTrackMutedArray(RetroMusicPart part)
        {
            if (part.TrackMuted == null)
                part.TrackMuted = new bool[part.Tracks];
            Array.Resize(ref m_selectedMusicPart.TrackMuted, part.Tracks);
        }

        void UpdateKeys()
        {
            // if (m_lastIdxMin < 0 || m_lastIdxMin >= m_selectedSFX.SoundData.Length)
            //     return;
            //
            if (Event.current.type != EventType.KeyDown)
                return;
            var kc = Event.current.keyCode;
            if (kc == KeyCode.Escape)
                ClearSelectionAndFocus();

            if (!CanUpdateKeys())
                return;
            
            // if (kc == KeyCode.W || kc == KeyCode.UpArrow)
            //     UpPressed();
            // else if (kc == KeyCode.A || kc == KeyCode.LeftArrow)
            //     LeftPressed();
            // else if (kc == KeyCode.D || kc == KeyCode.RightArrow)
            //     RightPressed();
            // else if (kc == KeyCode.S || kc == KeyCode.DownArrow)
            //     DownPressed();
            // else
            
            if (kc == KeyCode.Delete)
                DeletePressed();
            
            // else if (kc == KeyCode.C && Event.current.control)
            //     CopyPressed();
            // else if (kc == KeyCode.X && Event.current.control)
            //     CutPressed();
            // else if (kc == KeyCode.V && Event.current.control)
            //     PastePressed();
        }

        static bool CanUpdateKeys()
        {
            var focusName = GUI.GetNameOfFocusedControl();

            var focusNameOk = focusName == "";
            focusNameOk |= string.Equals(focusName, k_partsAreaControl);
            focusNameOk |= string.Equals(focusName, k_trackAreaControl);
            
            return focusNameOk;
        }

        void DeletePressed()
        {
            if (m_selectedSFXSnippets.Count > 0)
                DeleteSelectedSFXSnippets();
            else if (m_selectedPartRefsInMusicTrack.Count > 0)
                DeleteSelectedPartRefs();
        }

        void DeleteSelectedSFXSnippets()
        {
            if (m_selectedSFXSnippets.Count <= 0)
                return;

            for (var i = m_selectedMusicPart.Snippets.Count - 1; i >= 0; i--)
            {
                if (m_selectedSFXSnippets.Contains(i))
                    m_selectedMusicPart.Snippets.RemoveAt(i);
            }

            m_selectedSFXSnippets.Clear();
            UpdatePartsTexture();
        }

        void DeleteSelectedPartRefs()
        {
            if (m_selectedPartRefsInMusicTrack.Count <= 0)
                return;

            for (var i = m_selectedMusic.Parts.Count - 1; i >= 0; i--)
            {
                if (m_selectedPartRefsInMusicTrack.Contains(i))
                    m_selectedMusic.Parts.RemoveAt(i);
            }

            m_selectedPartRefsInMusicTrack.Clear();
        }

        void SelectMusic(int i)
        {
            GUI.FocusControl("");

            if (m_musicConfig == null)
            {
                m_selectedMusic = null;
                m_selectedSFXSnippets.Clear();
                return;
            }
            
            if (i < 0 || m_musicConfig.MusicTracks.Count <= i)
                return;

            m_editMusicTrackName = false;
            m_selectedMusic = m_musicConfig.MusicTracks[i];
            m_selectedSFXSnippets.Clear();

            // UpdatePitchAndWaveformTexture();
            // UpdateVolumeTexture();
        }

        void SelectMusicPart(int i)
        {
            if (i < 0 || m_musicConfig.MusicParts.Count <= i)
                return;

            GUI.FocusControl("");

            m_editPartName = false;
            m_selectedMusicPart = m_musicConfig.MusicParts[i];
            m_selectedSFXSnippets.Clear();
            UpdatePartsTexture();
            
            if (Event.current.shift && m_view == View.MusicTracks)
                ChangeSelectedParts(m_selectedMusicPart);
            
            // UpdatePitchAndWaveformTexture();
            // UpdateVolumeTexture();
        }

        void ChangeSelectedParts(RetroMusicPart selectedMusicPart)
        {
            var idx = m_musicConfig.MusicParts.IndexOf(selectedMusicPart);
            if (idx == -1)
                return;
            
            foreach (var i in m_selectedPartRefsInMusicTrack)
            {
                var p = m_selectedMusic.Parts[i];
                p.PartIdx = idx;
                m_selectedMusic.Parts[i] = p;
            }
        }

        void SelectSFX(int i)
        {
            if (i < 0 || m_musicConfig.SFXConfig.Count <= i)
                return;

            m_selectedSFX = m_musicConfig.SFXConfig[i];
            if (Event.current.shift && m_view == View.MusicParts)
                ChangeSelectedSnippets(m_selectedSFX);

            // UpdatePitchAndWaveformTexture();
            // UpdateVolumeTexture();
        }

        void ChangeSelectedSnippets(RetroSFX sfx)
        {
            var idx = m_musicConfig.SFXConfig.IndexOf(sfx);
            if (idx == -1)
                return;
            var snippets = m_selectedMusicPart.Snippets;
            foreach (var sIdx in m_selectedSFXSnippets)
            {
                var snip = snippets[sIdx];
                snip.SFXIdx = idx;
                snippets[sIdx] = snip;
            }
            UpdatePartsTexture();
        }
        
        void PauseOrPlayMusicTrack()
        {
            PauseOrPlay(k_musicTrackGo, out var createClipAndPlay);
            if (createClipAndPlay)
                PlayMusicTrack();
        }
        void PauseOrPlayPart()
        {
            PauseOrPlay(k_partGo, out var createClipAndPlay);
            if (createClipAndPlay)
                PlayPart();
        }
        void PauseOrPlay(string audioName, out bool createClipAndPlay)
        {
            createClipAndPlay = false;
            if (AudioObject != null && AudioObject.name.Equals(audioName))
            {
                if (AudioObject.isPlaying)
                    AudioObject.Pause();
                else 
                    AudioObject.UnPause();
            }
            else
            {
                if (AudioObject != null)
                    DestroyAudio();
                createClipAndPlay = true;
            }
        }
        
        void StopOrPlay()
        {
            if (m_view == View.MusicTracks)
                StopOrPlayMusicTrack();
            else 
                StopOrPlayPart();
        }

        void StopOrPlayPart()
        {
            if (!Stop())
                PlayPart();
        }
        void StopOrPlayMusicTrack()
        {
            if (!Stop())
                PlayMusicTrack();
        }

        static bool Stop()
        {
            if (AudioObject == null) 
                return false;
            
            DestroyAudio();
            return true;
        }

        void PlayPart()
        {
            BuildSamplesPart();
            CreateAndPlayClip(k_partGo, m_partLoop);

            m_waveTex = CreateAudioClipTexture(m_samples, 512, 50, Color.gray, BGColorB);
        }

        void PlayMusicTrack()
        {
            BuildSamples();
            CreateAndPlayClip(k_musicTrackGo, m_selectedMusic.Loop);

            m_waveTex = CreateAudioClipTexture(m_samples, 512, 50, Color.gray, BGColorB);
        }

        void CreateAndPlayClip(string goName, bool loop)
        {
            m_clip = CreateClip(m_samples, "Clip-" + Time.frameCount);
            PlayAudioClip(goName, m_clip, loop);
        }

        void BuildSamplesPart()
        {
            var maxSamples = m_selectedMusicPart.Duration * QuarterSampleRate;
            m_samples = new float[maxSamples];
            // Debug.Log( m_selectedMusicPart.Tracks);
            BuildSamplesPart(m_selectedMusicPart, 0, 1f / m_selectedMusicPart.Tracks);
            Normalize();
        }

        void Normalize()
        {
            var maxValues = new List<float>();
            for (var i = 0; i < m_samples.Length; i += QuarterSampleRate)
            {
                var maxValue = 0f;
                for (var si = i; si < i + QuarterSampleRate; si++)
                    maxValue = Mathf.Max(Mathf.Abs(m_samples[si]), maxValue);
                maxValues.Add(maxValue);
            }

            var prevMaxValue = 0f;
            for (var i = 0; i < maxValues.Count; i++)
            {
                var maxValue = maxValues[i];
                var nextValue = i + 1 < maxValues.Count ? maxValues[i + 1] : maxValue;
                maxValue = Mathf.Max(Mathf.Lerp(prevMaxValue, maxValue, 0.33f), maxValue);
                maxValue = Mathf.Max(Mathf.Lerp(maxValue, nextValue, 0.33f), maxValue);
                prevMaxValue = maxValue;
                
                if (maxValue <= m_musicConfig.LimitVolume)
                    continue;

                var samplesStart = i * QuarterSampleRate;
                for (var si = samplesStart; si < samplesStart + QuarterSampleRate; si++)
                {
                    m_samples[si] = Remap(m_samples[si], -maxValue, maxValue, -m_musicConfig.LimitVolume,
                        m_musicConfig.LimitVolume);
                }
            }
        }
        
        public static float Remap(float input, float oldLow, float oldHigh, float newLow, float newHigh)
        {
            var t = Mathf.InverseLerp(oldLow, oldHigh, input);
            return Mathf.Lerp(newLow, newHigh, t);
        }
        void BuildSamplesPart(RetroMusicPart part, int timeOffset, float volume)
        {
            var sampleIdxOff = timeOffset * QuarterSampleRate;
            var tempSamples = new List<float>();
            var noiseBuffer = new float[32];
            
            var dur = part.Duration* QuarterSampleRate;
            var end = sampleIdxOff + dur;
            
            foreach (var snip in part.Snippets)
            {
                if (IsMuted(part, snip))
                    continue;
                
                var sfxConfig = m_musicConfig.SFXConfig;
                if (snip.SFXIdx < 0 || snip.SFXIdx >= sfxConfig.Count)
                    continue;
                
                var sfx = sfxConfig[snip.SFXIdx];

                tempSamples.Clear();
                RetroSFXBuilder.BuildSamplesSFX(sfx, tempSamples, m_musicConfig.SFXConfig, noiseBuffer, part.Volume * volume);
                
                var sampleStartPos = sampleIdxOff + snip.TimePosition * QuarterSampleRate;
                
                for (var i = sampleStartPos; i < sampleStartPos + tempSamples.Count
                                             && i < m_samples.Length
                                             && i < end; i++)
                {
                    m_samples[i] += tempSamples[i - sampleStartPos];
                    // maxValue = Mathf.Max(Mathf.Abs(m_samples[i]), maxValue);
                }
            }
        }

        void BuildSamples()
        {
            GetDurationForMusicArrangement(out m_selectedMusic.Duration);
            var maxSamples = m_selectedMusic.Duration * QuarterSampleRate;
            m_samples = new float[maxSamples];

            var tracksMax = m_selectedMusic.Parts.Max(p => m_musicConfig.MusicParts[p.PartIdx].Tracks);
            // var maxValue = 0f;
            var t = 0;
            foreach (var p in m_selectedMusic.Parts)
            {
                if (p.PartIdx < 0 || p.PartIdx >= m_musicConfig.MusicParts.Count)
                    continue;
                var part = m_musicConfig.MusicParts[p.PartIdx];

                BuildSamplesPart(part, t, 1f / tracksMax);
                t += part.Duration;
            }
            Normalize();
        }

        void GetDurationForMusicArrangement(out int duration)
        {
            duration = 0;
            foreach (var p in m_selectedMusic.Parts)
            {
                if (p.PartIdx < 0 || p.PartIdx >= m_musicConfig.MusicParts.Count)
                    continue;

                var part = m_musicConfig.MusicParts[p.PartIdx];
                duration += part.Duration;
            }
        }

        void UpdatePartsTextureIfNeeded()
        {
            var supposedHeight = m_selectedMusicPart.Tracks * k_pixelPerPartTrack;
            if (m_musicPartTexture == null
                || m_musicPartTexture.height != supposedHeight
                || m_musicPartTexture.width != k_partsMaxWidth)
                UpdatePartsTexture();
        }

        void UpdatePartsTexture()
        {
            if (m_selectedMusicPart == null)
                return;
            var supposedHeight = m_selectedMusicPart.Tracks * k_pixelPerPartTrack;

            if (m_musicPartTexture == null
                || m_musicPartTexture.height != supposedHeight
                || m_musicPartTexture.width != k_partsMaxWidth)
                m_musicPartTexture = new Texture2D(k_partsMaxWidth, supposedHeight, TextureFormat.RGBA32, false);

            var gridPixPos = Mathf.RoundToInt(m_musicPartTexture.width /
                                              ((float) m_selectedMusicPart.Duration / m_selectedMusicPart.TimeGrid));
            for (var xi = 0; xi < m_musicPartTexture.width; xi++)
            for (var yi = 0; yi < m_musicPartTexture.height; yi++)
            {
                var grid = xi % gridPixPos == 0;
                var alternate = yi % (2 * k_pixelPerPartTrack) >= k_pixelPerPartTrack;
                var col = grid ? BGColorC :
                    alternate ? BGColorA : BGColorB;
                m_musicPartTexture.SetPixel(xi, yi, col);
            }

            var snippets = m_selectedMusicPart.Snippets;
            var tPosWidth = k_partsMaxWidth / m_selectedMusicPart.Duration;

            for (var i = 0; i < snippets.Count; i++)
            {
                var selected = m_selectedSFXSnippets.Contains(i);

                var snip = snippets[i];

                var sfxConfig = m_musicConfig.SFXConfig;
                var sfx = sfxConfig[snip.SFXIdx];

                var muted = IsMuted(m_selectedMusicPart, snip);
                var xStart = snip.TimePosition * tPosWidth;
                var yStart = supposedHeight - (1 + snip.TrackPosition) * k_pixelPerPartTrack;

                var yVolStart = yStart;
                var yVolEnd = yStart + k_volumePixelPerPartTrack;
                var yPitchStart = yVolEnd;

                var durationRatio = (float) m_selectedMusicPart.Duration * 32 / sfx.Duration;
                var soundWidth = k_partsMaxWidth / durationRatio;
                // var durationRatio = defDur / sfx.Duration;
                // var soundWidth = tPosWidth / durationRatio;
                for (var sfx_i = 0; sfx_i < 32; sfx_i++)
                {
                    var sound = sfx.SoundData[sfx_i];

                    var octaves = sfx.OctaveMax - sfx.OctaveMin;
                    var halfTones = octaves > 0 ? octaves * 12 : PitchValues;
                    var pitchStart = sfx.OctaveMin * 12;

                    var pitchP = (float) (sound.Pitch - pitchStart) / halfTones;
                    var custom = sound.WaveForm >= (int) WaveForm.Custom;
                    var customIdx = sound.WaveForm - (int) WaveForm.Custom;

                    var volColor = sound.WaveForm == (int) WaveForm.Select || muted
                        ? InactiveColor
                        : VolumeColors[sound.Volume];

                    var color = sound.Volume == 0 || muted
                        ? InactiveColor
                        : custom
                            ? sfxConfig.GetCurveColor(customIdx)
                            : WaveFormColor[sound.WaveForm];

                    var gap = Mathf.FloorToInt(soundWidth / 4);
                    var xiStart = xStart + sfx_i * soundWidth;
                    var xiEnd = xiStart + soundWidth;
                    for (var xi = xiStart; xi < m_musicPartTexture.width && xi < xiEnd; xi++)
                    {
                        for (var yi = yVolStart; yi < yVolEnd; yi++)
                        {
                            var colFin = yi == yVolStart && selected ? Color.white : volColor;
                            m_musicPartTexture.SetPixel((int) xi, yi, colFin);
                        }
                    }

                    xiEnd = xiStart + soundWidth - gap;
                    xiStart = xStart + sfx_i * soundWidth + gap;
                    for (var xi = xiStart; xi < m_musicPartTexture.width && xi < xiEnd; xi++)
                    {
                        var yPitchEnd = yPitchStart +
                                        Mathf.CeilToInt(pitchP * (k_pixelPerPartTrack - k_volumePixelPerPartTrack));
                        for (var yi = yPitchStart; yi < yPitchEnd; yi++)
                        {
                            var colFin = 0.5f * color;
                            if (yi > yPitchEnd - 4)
                                colFin = selected ? Color.white : color;

                            m_musicPartTexture.SetPixel((int) xi, yi, colFin);
                        }
                    }
                }
            }

            m_musicPartTexture.Apply();
        }

        bool IsMuted(RetroMusicPart part, RetroMusicSFXSnippet snip)
        {
            if (part.TrackMuted == null)
                return false;
            if (snip.TrackPosition >= part.TrackMuted.Length)
                return false;
            
            if (snip.TrackPosition < 0 || snip.TrackPosition >= part.Tracks)
                return true;
            return part.TrackMuted[snip.TrackPosition];
        }

        void OnPartRemoved(int removedPartIdx)
        {
            if (m_musicConfig.MusicTracks == null || m_musicConfig.MusicTracks.Count <= 0)
                return;
            foreach (var musicTrack in m_musicConfig.MusicTracks)
            {
                for (var i = 0; i < musicTrack.Parts.Count; i++)
                {
                    var part = musicTrack.Parts[i];
                    if (part.PartIdx < removedPartIdx)
                        continue;
                    if (part.PartIdx >= removedPartIdx)
                        part.PartIdx -= 1;

                    musicTrack.Parts[i] = part;
                }
            }
        }


        void UpdateNameIndices()
        {
            UpdateSFXNameIndices();
            UpdateTrackPartNameIndices();
        }
        void UpdateTrackPartNameIndices()
        {
            if (m_musicConfig == null)
                return;
            if (m_musicConfig.MusicTracks == null || m_musicConfig.MusicTracks.Count <= 0)
                return;
            if (m_musicConfig.MusicParts == null || m_musicConfig.MusicParts.Count <= 0)
                return;

            foreach (var track in m_musicConfig.MusicTracks)
                UpdateTrackPartNameIndices(track);
        }

        void UpdateTrackPartNameIndices(RetroMusic musicTrack)
        {
            for (var i = 0; i < musicTrack.Parts.Count; i++)
            {
                var part = musicTrack.Parts[i];
                var idxInRange = part.PartIdx >= 0 && part.PartIdx < m_musicConfig.MusicParts.Count;
                var ok = idxInRange && string.Equals(m_musicConfig.MusicParts[part.PartIdx].Name, part.PartName);

                if (ok)
                    continue;

                var idx = m_musicConfig.MusicParts.FindIndex(p
                    => string.Equals(part.PartName, p.Name));

                if (idx != -1) // fix idx first
                    part.PartIdx = idx;
                else if (idxInRange) // fix name
                    part.PartName = m_musicConfig.MusicParts[part.PartIdx].Name;
                else
                    Debug.LogWarning($"Could not find Part idx: {part.PartIdx} name: {part.PartName}");

                musicTrack.Parts[i] = part;
            }
        }

        void UpdateSFXNameIndices()
        {
            if (m_musicConfig == null)
                return;
            if (m_musicConfig.SFXConfig == null)
                return;
            
            if (m_musicConfig.MusicParts == null || m_musicConfig.MusicParts.Count <= 0)
                return;

            foreach (var part in m_musicConfig.MusicParts)
                UpdateSFXNameIndices(part);
        }

        void UpdateSFXNameIndices(RetroMusicPart musicTrack)
        {
            for (var i = 0; i < musicTrack.Snippets.Count; i++)
            {
                var snippet = musicTrack.Snippets[i];
                var idxInRange = snippet.SFXIdx >= 0 && snippet.SFXIdx < m_musicConfig.SFXConfig.Count;
                var ok = idxInRange && string.Equals(m_musicConfig.SFXConfig[snippet.SFXIdx].Name, snippet.SFXName);

                if (ok)
                    continue;

                var idx = m_musicConfig.SFXConfig.FindIndex(p => string.Equals(snippet.SFXName, p.Name));

                if (idx != -1) // fix idx first
                    snippet.SFXIdx = idx;
                else if (idxInRange) // fix name
                    snippet.SFXName = m_musicConfig.SFXConfig[snippet.SFXIdx].Name;
                else
                    Debug.LogWarning($"Could not find Part idx: {snippet.SFXIdx} name: {snippet.SFXName}");

                musicTrack.Snippets[i] = snippet;
            }
        }
        
        void ModPartDuration(int dir, int steps)
        {
            if (m_view == View.MusicTracks)
                return;
            if (!ValidMusicPartSelection)
                return;
            
            GUI.FocusControl("");
            RecordUndoDirty(m_musicConfig, "Modify Duration");
            m_selectedMusicPart.Duration = Mathf.Clamp(m_selectedMusicPart.Duration + dir * steps, 1, 255);
        }
        
        void EditNext(int jump)
        {
            if (m_view == View.MusicTracks)
            {
                if (m_musicConfig == null || m_musicConfig.MusicTracks.Count <= 0)
                    return;
                if (!ValidMusicTrackSelection)
                    m_selectedMusic = m_musicConfig.MusicTracks[0];
                else
                {
                    var idx = m_musicConfig.MusicTracks.IndexOf(m_selectedMusic);
                    var targetIdx = Mathf.Clamp(idx + jump, 0, m_musicConfig.MusicTracks.Count - 1); 
                    m_selectedMusic = m_musicConfig.MusicTracks[targetIdx];
                }
            }
            else if (m_view == View.MusicParts)
            {
                if (m_musicConfig == null || m_musicConfig.MusicParts.Count <= 0)
                    return;
                if (!ValidMusicPartSelection)
                    m_selectedMusicPart = m_musicConfig.MusicParts[0];
                else
                {
                    var idx = m_musicConfig.MusicParts.IndexOf(m_selectedMusicPart);
                    var targetIdx = Mathf.Clamp(idx + jump, 0, m_musicConfig.MusicParts.Count - 1); 
                    m_selectedMusicPart = m_musicConfig.MusicParts[targetIdx];
                }
            }
        }
        
        void DrawSelectionNext(int jump)
        {
            if (m_view == View.MusicTracks)
            {
                if (m_musicConfig == null || m_musicConfig.MusicParts.Count <= 0)
                    return;
                if (!ValidMusicPartSelection)
                {
                    if (jump > 0)
                        m_selectedMusicPart = m_musicConfig.MusicParts[0];
                }       
                else
                {
                    var idx = m_musicConfig.MusicParts.IndexOf(m_selectedMusicPart);
                    var targetIdx = Mathf.Clamp(idx + jump, -1, m_musicConfig.MusicParts.Count - 1);
                    m_selectedMusicPart = targetIdx == -1 ? null : m_musicConfig.MusicParts[targetIdx];
                }
            }
            else if (m_view == View.MusicParts)
            {
                if (m_musicConfig == null || m_musicConfig.SFXConfig.Count <= 0)
                    return;
                if (!ValidSFXSelection)
                {
                    if (jump > 0)
                        m_selectedSFX = m_musicConfig.SFXConfig[0];
                }
                else
                {
                    var idx = m_musicConfig.SFXConfig.IndexOf(m_selectedSFX);
                    var targetIdx = Mathf.Clamp(idx + jump, -1, m_musicConfig.SFXConfig.Count - 1); 
                    m_selectedSFX = targetIdx == -1 ? null : m_musicConfig.SFXConfig[targetIdx];
                }
            }
        }
        
        void ClearSelectionAndFocus()
        {
            m_clearFocusNextFrame--;
            GUI.FocusControl("");

            m_editPartName = false;
            m_editMusicTrackName = false;
            
            if (m_view == View.MusicTracks)
                m_selectedMusicPart = null;
            else 
                m_selectedSFX = null;
        }
        #endregion
    }
}