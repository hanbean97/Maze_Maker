using System;
using System.Collections.Generic;
using System.Linq;
using DoItRetro;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using static DoItRetro.Utility;
using static DoTheSFXAndMusic.AudioPreviewUtility;
using static DoTheSFXAndMusic.Utility;
using static DoTheSFXAndMusic.RetroSFXBuilder;
using static DoTheSFXAndMusic.TextureStyleOperations;
using static DoTheSFXAndMusic.WaveFormEnumConstants;

namespace DoTheSFXAndMusic
{
    public class DoTheSFXEditorWindow : EditorWindow
    {
        static DoTheSFXEditorWindow m_currentSampler;

        #region constants
        const string k_title = "Do the SFX";

        const int k_spacePerSoundDataBar = 16;
        const int k_pixelPerPitchMin = 4;
        const int k_pixelPerPitchMax = 8;
        const int k_pixelPerVolume = 8;

        const string k_pitchAreaControl = nameof(RetroSFX) + "PitchArea";
        const string k_volumeAreaControl = nameof(RetroSFX) + "VolumeArea";
        const string k_sfxGo = "SFX";

        readonly string[] m_notes = {"C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"};

        enum View
        {
            Draw,
            Data
        }

        enum PaintArea
        {
            PitchWaveform,
            Volume
        }

        #endregion

        #region Members

        RetroSFXConfig m_config;

        Texture2D m_waveTex;
        AudioClip m_clip;

        WaveForm m_waveForm;
        FX m_fx;

        readonly List<int> m_multiSelection = new List<int>();
        
        int m_lastIdxMin;
        int m_lastIdxMax;

        int m_selectionStart;

        View m_view;
        PaintArea m_lastClicked;

        readonly List<float> m_samples = new List<float>();

        Texture2D m_pitchAndWaveformTexture;
        Texture2D m_volumeTexture;

        Vector2 m_scrollPos;
        Vector2 m_sfxListScrollPos;

        RetroSFX m_selectedSFX;
        int m_sectionPlayStart;

        SoundData[] m_tempData;
        SoundData[] m_copyData;

        float m_lastFrameKey;

        int m_grid;
        int m_gridOffset;

        bool m_useScaleFilter;
        readonly bool[] m_scaleFilter = new bool[12];
        
        bool m_editName;
        int m_customForm;
        Vector2 m_lastMousePos;
        bool m_lastDirty;

        #endregion

        #region Properties
        bool IsSelectionTool => m_waveForm == WaveForm.Select;
        int MaxSamples => QuarterSampleRate * m_selectedSFX.Duration;
        int SamplesPerSoundData => MaxSamples / SoundDataMaxLength;

        ICollection<float> SampleRange
        {
            get
            {
                var useSection = m_selectedSFX.SectionMin < m_selectedSFX.SectionMax && m_selectedSFX.SectionMax > 0;
                return useSection
                    ? m_samples.GetRange(m_selectedSFX.SectionMin * SamplesPerSoundData,
                        (m_selectedSFX.SectionMax - m_selectedSFX.SectionMin) * SamplesPerSoundData)
                    : m_samples;
            }
        }

        bool ValidSelection => m_selectedSFX != null 
                               && m_config!= null 
                               && m_config.Contains(m_selectedSFX);
        
        int Octaves => m_selectedSFX.OctaveMax - m_selectedSFX.OctaveMin;
        int HalfTones => Octaves > 0 ? Octaves * 12 : PitchValues;
        int PitchStart => m_selectedSFX.OctaveMin * 12;

        int PitchEnd => m_selectedSFX.OctaveMax == 0
            ? PitchValues
            : m_selectedSFX.OctaveMax * 12;

        int PixelPerPitch
        {
            get
            {
                var maxHeight = 96 * k_pixelPerPitchMin;
                var pixel = maxHeight / HalfTones;
                return Mathf.Clamp(pixel, k_pixelPerPitchMin, k_pixelPerPitchMax);
            }
        }
        
        bool SelectionValid => m_lastIdxMin >= 0 && m_lastIdxMin < m_selectedSFX.SoundData.Length 
                               || m_multiSelection.Count > 0;
        
        IEnumerable<int> Selection
        {
            get
            {
                foreach (var i in m_multiSelection)
                {
                    if (i>= m_lastIdxMin && i <= m_lastIdxMax)
                        continue;
                    
                    yield return i;
                }
                
                if (m_lastIdxMin < 0 || m_lastIdxMax >= m_selectedSFX.SoundData.Length)
                    yield break;
                for (var i = m_lastIdxMin; i <= m_lastIdxMax; i++)
                    yield return i;
            }
        }
        
        IEnumerable<Vector2Int> SelectionsMinMax
        {
            get
            {
                var curr= -Vector2Int.one;
                foreach (var i in Selection)
                {
                    var connectedValue = i == curr.y + 1;
                    if (!connectedValue 
                        || curr.x < 0)
                    {
                        if (curr.x >= 0 && curr.y >= 0)
                            yield return curr;
                        // init new min value
                        curr.x = i;
                    }

                    // new max value
                    curr.y = i;
                }
                if (curr.x >= 0 && curr.y >= 0)
                    yield return curr;
            }
        }
        #endregion

        #region Unity Hooks

        public const string MenuPath = "Tools/Do the SFX";
        [MenuItem(MenuPath)]
        public static DoTheSFXEditorWindow Open()
        {
            var window = GetWindow<DoTheSFXEditorWindow>(typeof(DoTheMusicEditorWindow), typeof(SceneView));
            window.titleContent = new GUIContent(k_title);
            window.Focus();
            return window;
        }

        void OnEnable() => Undo.undoRedoPerformed += UndoPerformed;

        void OnDisable() => Undo.undoRedoPerformed -= UndoPerformed;

        void UndoPerformed()
        {
            UpdatePitchAndWaveformTexture();
            UpdateVolumeTexture();
        }

        void OnGUI()
        {
            m_currentSampler = this;
            
            CustomCursor();
            if (!AllTexturesSetup(ref RetroMusicSFXStyles))
                return;

            SelectConfigGUI();
            if (m_config == null)
                return;
            
            var dirty = EditorUtility.IsDirty(m_config);
            if (m_lastDirty != dirty)
                UpdateTitleDirty(dirty);

            using (var scrollScope = new GUILayout.ScrollViewScope(m_scrollPos))
            {
                m_scrollPos = scrollScope.scrollPosition;
                using (var hs = new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    using (var vs = new EditorGUILayout.VerticalScope(GUILayout.Width(200f)))
                    {
                        SwitchViewGUI();
                        ConfigSFXListGUI();
                    }

                    using (var vs = new EditorGUILayout.VerticalScope())
                    {
                        DataHeaderGUI();
                        MainDataGUI();
                    }
                }

                DrawWaveTexture();
                UpdateKeys();
            }
        }
        
        void UpdateTitleDirty(bool dirty)
        {
            m_lastDirty = dirty;
            var titleStr = dirty ? $"{k_title} *" : k_title;
            titleContent = new GUIContent(titleStr);
        }
        
        void Update() => CleanupAudioOnDone();

        void OnDestroy() => DestroyAudio();

        #region Shortcuts

        [Shortcut(nameof(DoTheSFXEditorWindow) + ".SwitchView", typeof(DoTheSFXEditorWindow), KeyCode.Tab)]
        public static void SwitchView(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.SwitchView();
        }

        [Shortcut(nameof(DoTheSFXEditorWindow) + ".IncreaseSpeed", typeof(DoTheSFXEditorWindow), KeyCode.Period)]
        public static void IncreaseSpeed(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.ModDuration(-1, 1);
        }

        [Shortcut(nameof(DoTheSFXEditorWindow) + ".DecreaseSpeed", typeof(DoTheSFXEditorWindow), KeyCode.Comma)]
        public static void DecreaseSpeed(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.ModDuration(1, 1);
        }

        [Shortcut(nameof(DoTheSFXEditorWindow) + ".IncreaseSpeedFast", typeof(DoTheSFXEditorWindow), KeyCode.Period,
            ShortcutModifiers.Shift)]
        public static void IncreaseSpeedFast(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.ModDuration(-1, 4);
        }

        [Shortcut(nameof(DoTheSFXEditorWindow) + ".DecreaseSpeedFast", typeof(DoTheSFXEditorWindow), KeyCode.Comma,
            ShortcutModifiers.Shift)]
        public static void DecreaseSpeedFast(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.ModDuration(1, 4);
        }

        [Shortcut(nameof(DoTheSFXEditorWindow) + ".Play", typeof(DoTheSFXEditorWindow), KeyCode.Space)]
        public static void Play(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.StopOrPlay();
        }

        [Shortcut(nameof(DoTheSFXEditorWindow) + ".NextSFX", typeof(DoTheSFXEditorWindow), KeyCode.Equals)]
        public static void NextSFX(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.ChangeSFX(1);
        }

        [Shortcut(nameof(DoTheSFXEditorWindow) + ".PrevSFX", typeof(DoTheSFXEditorWindow), KeyCode.Minus)]
        public static void PrevSFX(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.ChangeSFX(-1);
        }

        [Shortcut(nameof(DoTheSFXEditorWindow) + ".NextSFXJump4", typeof(DoTheSFXEditorWindow), KeyCode.Equals,
            ShortcutModifiers.Shift)]
        public static void NextSFXFast(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.ChangeSFX(4);
        }

        [Shortcut(nameof(DoTheSFXEditorWindow) + ".PrevSFXJump4", typeof(DoTheSFXEditorWindow), KeyCode.Minus,
            ShortcutModifiers.Shift)]
        public static void PrevSFXFast(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.ChangeSFX(-4);
        }
        
        [Shortcut(nameof(DoTheSFXEditorWindow) + ".NextInstrument", typeof(DoTheSFXEditorWindow), KeyCode.LeftBracket)]
        public static void NextInstrument(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.NextInstrument(-1);
        }
        
        [Shortcut(nameof(DoTheSFXEditorWindow) + ".PrevInstrument", typeof(DoTheSFXEditorWindow), KeyCode.RightBracket)]
        public static void PrevInstrument(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.NextInstrument(1);
        }
        
        [Shortcut(nameof(DoTheSFXEditorWindow) + ".SelectionToSection", typeof(DoTheSFXEditorWindow), KeyCode.C)]
        public static void SelectionToSection(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.SelectionToSection();
        }

        [Shortcut(nameof(DoTheSFXEditorWindow) + ".FlattenSelection", typeof(DoTheSFXEditorWindow), KeyCode.F, ShortcutModifiers.Alt)]
        public static void FlattenSelection(ShortcutArguments args)
        {
            if (m_currentSampler == null)
                return;
            m_currentSampler.FlattenSelection();
        }

        #endregion

        #endregion

        #region GUI

        void CustomCursor()
        {
            if (Event.current != null) 
                m_lastMousePos = Event.current.mousePosition;

            if (m_view == View.Data)
                return;
            if (!ValidSelection) 
                return;
            if (m_waveForm == WaveForm.Select)
                return;
            
            Cursor.SetCursor(RetroMusicSFXStyles.EditCursor, Vector2.zero, CursorMode.Auto);
            EditorGUIUtility.AddCursorRect(new Rect(m_lastMousePos, 0.5f * Vector2.one),
                MouseCursor.CustomCursor);
        }
        
        void SwitchViewGUI()
        {
            var viewStyle = m_view == View.Data ? RetroMusicSFXStyles.ViewData.Style : RetroMusicSFXStyles.ViewDraw.Style;
            // $"View [{m_view}]"
            if (GUILayout.Button(GUIContent.none, viewStyle, GUILayout.Width(53f), GUILayout.Height(20f)))
                SwitchView();
        }
        
        void DataHeaderGUI()
        {
            if (!ValidSelection)
                return;
            
            using (var ccs = new EditorGUI.ChangeCheckScope())
            using (var hs = new GUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                var durContent = new GUIContent("Dur", RetroMusicSFXStyles.DurationIcon, "Duration");
                EditorGUILayout.LabelField(durContent, GUILayout.Height(20f), GUILayout.Width(50f));
                GUI.SetNextControlName("Duration");
                var dur = (int) EditorGUILayout.Slider(GUIContent.none, m_selectedSFX.Duration, 1, 255, GUILayout.Width(125f));
                if (ccs.changed)
                {
                    RecordUndoDirty(m_config, "Duration changed");
                    m_selectedSFX.Duration = dur;
                }
                
                GUILayout.Space(20f);
                
                OctaveGUI();
                GUILayout.Space(20f);
            }

            using (var hs = new GUILayout.HorizontalScope())
            {
                using (new GUILayout.HorizontalScope(EditorStyles.helpBox, GUILayout.Width(600f)))
                {
                    GridGUI();
                    ScaleGUI();
                }
                using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    PausePlayExportGUI();
                }
            }

            // ScaleGUI();
        }

        void GridGUI()
        {
            var toggled = m_grid > 1;
            if (toggled != EditorGUILayout.Toggle(GUIContent.none, toggled, RetroMusicSFXStyles.Grid.Style, GUILayout.Height(20f), GUILayout.Width(20f)))
            {
                GUI.FocusControl("");
                m_grid = toggled ? 1 : 2;
                m_gridOffset = 0;
                UpdatePitchAndWaveformTexture();
                UpdateVolumeTexture();
                return;
            }
            
            if (!toggled)
                return;
            
            GUI.SetNextControlName("Grid");
            var grid = EditorGUILayout.IntSlider(GUIContent.none, m_grid, 2, 8,
                GUILayout.Height(20f), GUILayout.Width(150f));

            EditorGUILayout.LabelField("Offset", GUILayout.Height(20f), GUILayout.Width(50f));
            GUI.SetNextControlName("Offset");
            var off = EditorGUILayout.IntSlider(GUIContent.none, m_gridOffset, 0, m_grid-1,
                GUILayout.Height(20f), GUILayout.Width(150f));
            if (grid == m_grid && off == m_gridOffset)
                return;
            
            m_gridOffset = off;
            m_grid = grid;
            UpdatePitchAndWaveformTexture();
            UpdateVolumeTexture();
        }
        void ScaleGUI()
        {
            // using var hs = new EditorGUILayout.HorizontalScope();
            var toggled = m_useScaleFilter;
            if (toggled != GUILayout.Toggle(m_useScaleFilter, "Scale", RetroMusicSFXStyles.Button.Style, GUILayout.Height(20f), GUILayout.Width(42f)))
            {
                GUI.FocusControl("");
                m_useScaleFilter = !m_useScaleFilter;
                return;
            }
            
            if (!m_useScaleFilter)
                return;

            for (var i = 0; i < m_scaleFilter.Length; i++)
            {
                var noteToggled = m_scaleFilter[i];
                if (noteToggled != GUILayout.Toggle( noteToggled,$"{m_notes[i]}",
                    RetroMusicSFXStyles.Button.Style, GUILayout.Height(20f), GUILayout.Width(25f)))
                    m_scaleFilter[i] = !m_scaleFilter[i];
            }
        }
        void PausePlayExportGUI()
        {
            using (var hs2 = new EditorGUILayout.HorizontalScope()) //(EditorStyles.helpBox);
            {
                var style = (AudioObject == null || !AudioObject.isPlaying)
                    ? RetroMusicSFXStyles.Play.Style
                    : RetroMusicSFXStyles.Pause.Style;
                if (GUILayout.Button(GUIContent.none, style, GUILayout.Width(20f), GUILayout.Height(20f)))
                    PauseOrPlay();

                using (new EditorGUI.DisabledScope(AudioObject == null))
                {
                    if (GUILayout.Button(GUIContent.none, RetroMusicSFXStyles.Stop.Style, GUILayout.Width(20f),
                        GUILayout.Height(20f)))
                        Stop();
                }

                GUILayout.Space(25);

                if (GUILayout.Button("", RetroMusicSFXStyles.Export.Style, GUILayout.Width(20f), GUILayout.Height(20f)))
                {
                    var sfxNamePath = EditorUtility.SaveFilePanel("Save SFX as wav", "", m_selectedSFX.Name, "wav");

                    if (!string.IsNullOrEmpty(sfxNamePath))
                        ExportSFX(sfxNamePath);
                }
            }
        }
        
        void OctaveGUI()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var octMinF = (float) m_selectedSFX.OctaveMin;
                var octMaxF = (float) m_selectedSFX.OctaveMax;

                var octContent = new GUIContent("Octaves", RetroMusicSFXStyles.OctaveIcon, "Octaves");
                EditorGUILayout.LabelField(octContent, GUILayout.Height(20f), GUILayout.Width(75f));
                GUI.SetNextControlName("Octaves");
                EditorGUILayout.MinMaxSlider(GUIContent.none, ref octMinF, ref octMaxF, 0,
                    8, GUILayout.Width(200f));

                if (!check.changed)
                    return;

                var octMin = Mathf.RoundToInt(octMinF);
                var octMax = Mathf.RoundToInt(octMaxF);

                if (octMin == octMax)
                    return;

                RecordUndoDirty(m_config, "Octaves changed");
                m_selectedSFX.OctaveMin = octMin;
                m_selectedSFX.OctaveMax = octMax;
                UpdatePitchAndWaveformTexture();
            }
        }

        void DrawWaveTexture() => GUILayout.Box(m_waveTex);
        
        void ConfigSFXListGUI()
        {
            var deleteIdx = -1;

            using (var scv = new EditorGUILayout.ScrollViewScope(m_sfxListScrollPos, GUILayout.Width(200f)))
            using (var vs = new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(180f),
                GUILayout.MinHeight(100)))
            {
                m_sfxListScrollPos = scv.scrollPosition;

                for (var i = 0; i < m_config.Count; i++)
                {
                    var sound = m_config[i];

                    using (var hs = new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label($"{i}: ", GUILayout.Width(20f));
                        var selected = m_selectedSFX == sound;
                        if (m_editName && selected)
                        {
                            using (var ccs = new EditorGUI.ChangeCheckScope())
                            {
                                GUI.SetNextControlName("SFXName");
                                var sfxName = EditorGUILayout.DelayedTextField(sound.Name);
                                if (ccs.changed)
                                {
                                    RecordUndoDirty(m_config, "SFX Name changed");
                                    sound.Name = sfxName;
                                }
                            }
                        }
                        else if (selected != GUILayout.Toggle(selected, sound.Name, RetroMusicSFXStyles.Button.Style, GUILayout.Height(20)))
                        {
                            if (selected)
                                m_editName = true;
                            else
                                SelectSFX(i);
                        }
                        
                        if (GUILayout.Button("", RetroMusicSFXStyles.Delete.Style, GUILayout.Width(20f), GUILayout.Height(20f)))
                            deleteIdx = i;
                    }
                }

                if (GUILayout.Button("", RetroMusicSFXStyles.Add.Style, GUILayout.Width(20f), GUILayout.Height(20f)))
                {
                    GUI.FocusControl("");
                    RecordUndoDirty(m_config, "Add SFX");
                    m_config.AddSound();
                    if (!ValidSelection)
                        SelectSFX(0);
                }

                if (deleteIdx != -1)
                {
                    if (m_config[deleteIdx] == m_selectedSFX)
                        SelectSFX(Mathf.Clamp(deleteIdx, 0, m_config.Count - 1));

                    GUI.FocusControl("");
                    RecordUndoDirty(m_config, "Delete SFX");
                    m_config.RemoveAt(deleteIdx);
                }
            }
        }

        void SelectConfigGUI()
        {
            var prevConfig = m_config;
            GUILayout.Space(5);

            using (var hs = new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Space(5);
                EditorGUILayout.LabelField("SFX Config:", GUILayout.Width(75));
                m_config = (RetroSFXConfig) EditorGUILayout.ObjectField(m_config, typeof(RetroSFXConfig), false,
                    GUILayout.Width(200));

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Create new", GUILayout.Width(90)))
                {
                    var path = EditorUtility.SaveFilePanelInProject("Save Retro asset", "RetroSFX.asset", "asset",
                        "Please choose a location for the new asset");

                    m_config = CreateInstance<RetroSFXConfig>();
                    AssetDatabase.CreateAsset(m_config, path);
                    AssetDatabase.SaveAssets();
                }

                GUILayout.Space(5);

                if (prevConfig != m_config)
                    SelectSFX(0);  
            }
        }

        void MainDataGUI()
        {
            if (!ValidSelection)
                return;
            using (var vs = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (m_view == View.Data)
                    DataGUI();
                else
                    DrawGUI();
            }
        }
        
        void DrawGUI()
        {
            // if (GUILayout.Button("Clear Tex"))
            // {
            //     m_pitchAndWaveformTexture = null;
            //     m_volumeTexture = null;
            // }

            if (!ValidSelection)
                return;

            UpdatePitchAndWaveformTextureIfNeeded();
            UpdateVolumeTextureIfNeeded();

            SelectWaveformGUI();
            SectionLoopGUI();

            DrawTexture_GetRectMinMax(k_pitchAreaControl, m_pitchAndWaveformTexture,
                new Vector2(2 * k_spacePerSoundDataBar, 0),
                out var pitchWaveRect, out var pitchWaveMinPos, out var pitchWaveMaxPos);

            OctaveButtons(pitchWaveMinPos, pitchWaveMaxPos);
            
            GUILayout.Space(15);
            GUILayout.Label("Volumes: ");
            DrawTexture_GetRectMinMax(k_volumeAreaControl, m_volumeTexture,
                new Vector2(2 * k_spacePerSoundDataBar, k_pixelPerVolume),
                out var volumeRect, out var volumeMinPos, out _);

            DrawPlayPositionMarker(pitchWaveRect, volumeRect, k_sfxGo);

            var mp = Event.current.mousePosition;

            GetGridPosCeil(mp, pitchWaveMinPos, new Vector2Int(m_pitchAndWaveformTexture.width, m_pitchAndWaveformTexture.height - k_fxPixel), SoundDataMaxLength, HalfTones,
                out var pitchWaveGridPosX, out var pitchWaveGridPosY);
            GetGridPosCeil(mp, volumeMinPos, new Vector2Int(m_volumeTexture.width, m_volumeTexture.height), SoundDataMaxLength, VolumeLevels,
                out var volumeGridPosX, out var volumeGridPosY);
            
            DrawPositionLabels(pitchWaveMinPos, pitchWaveMaxPos);
            DrawSelectedNoteLabels(pitchWaveMinPos, pitchWaveMaxPos);

            var eventType = Event.current.type;
            var click = Event.current.isMouse && Event.current.button == 0;
            if (click)
                MouseDrawPitchAndVolume(eventType, pitchWaveGridPosX, pitchWaveGridPosY, volumeGridPosX,
                    volumeGridPosY);

            // GUILayout.Label(
            //     $"{mp} pitchWaveRect {pitchWaveRect} volumeRect {volumeRect} rel pos {volumeGridPosX} {volumeGridPosY}");

            Repaint();
        }

        void OctaveButtons(Vector2 pitchWaveMinPos, Vector2 pitchWaveMaxPos)
        {
            var topRightPos = new Vector2(pitchWaveMaxPos.x, pitchWaveMinPos.y);
            var bottomRightPos = new Vector2(pitchWaveMaxPos.x, pitchWaveMaxPos.y);

            var canIncreaseUpper = m_selectedSFX.OctaveMax < 8;
            var canDecreaseUpper = m_selectedSFX.OctaveMax-1 > m_selectedSFX.OctaveMin;
            var canIncreaseLower = m_selectedSFX.OctaveMin + 1 < m_selectedSFX.OctaveMax;
            var canDecreaseLower = m_selectedSFX.OctaveMin > 0;

            bool octIncreaseUpper, octDecreaseUpper, octIncreaseLower, octDecreaseLower;
            using (new EditorGUI.DisabledScope(!canIncreaseUpper))
                octIncreaseUpper = GUI.Button(new Rect(topRightPos.x + 15, topRightPos.y, 20, 20), 
                    GUIContent.none, RetroMusicSFXStyles.Up.Style);
            
            using (new EditorGUI.DisabledScope(!canDecreaseUpper))
                octDecreaseUpper = GUI.Button(new Rect(topRightPos.x + 15, topRightPos.y + 20, 20, 20), 
                    GUIContent.none, RetroMusicSFXStyles.Down.Style);
            
            using (new EditorGUI.DisabledScope(!canIncreaseLower))
                octIncreaseLower = GUI.Button(new Rect(bottomRightPos.x + 15, bottomRightPos.y - 40, 20, 20),
                    GUIContent.none, RetroMusicSFXStyles.Up.Style);
            
            using (new EditorGUI.DisabledScope(!canDecreaseLower))
                octDecreaseLower = GUI.Button(new Rect(bottomRightPos.x + 15, bottomRightPos.y - 20, 20, 20),
                    GUIContent.none, RetroMusicSFXStyles.Down.Style);

            if (octIncreaseUpper || octDecreaseUpper || octIncreaseLower || octDecreaseLower)
            {
                var min = m_selectedSFX.OctaveMin;
                var max = m_selectedSFX.OctaveMax;

                if (octIncreaseUpper)
                    max++;
                else if (octDecreaseUpper)
                    max--;
                else if (octIncreaseLower)
                    min++;
                else //if (octDecreaseLower)
                    min--;

                GUI.FocusControl("");
                RecordUndoDirty(m_config, "Octaves changed");
                m_selectedSFX.OctaveMin = min;
                m_selectedSFX.OctaveMax = max;
                UpdatePitchAndWaveformTexture();
            }
        }
        void DrawPositionLabels(Vector2 pitchWaveMinPos, Vector2 pitchWaveMaxPos)
        {
            using (new ColorScope(Color.grey))
            {
                for (var i = 1; i < 32; i+=2)
                {
                    var off = i > 10 ? 16 : 13;
                    var x = i * k_spacePerSoundDataBar - off + pitchWaveMinPos.x;
                    var y = pitchWaveMaxPos.y;
                    GUI.Label(new Rect(x, y, 20, 20), $"{i+1}", EditorStyles.miniLabel);
                }
            }
        }

        void DrawSelectedNoteLabels(Vector2 pitchWaveMinPos, Vector2 pitchWaveMaxPos)
        {
            foreach (var i in Selection)
            {
                if (i < 0 || i >= SoundDataMaxLength)
                    continue;

                var note = m_selectedSFX.SoundData[i].Pitch % 12;
                var octave = Mathf.FloorToInt((float) m_selectedSFX.SoundData[i].Pitch / 12);

                if (octave < m_selectedSFX.OctaveMin || octave >= m_selectedSFX.OctaveMax)
                    continue;
                
                GUI.Label(new Rect(i * k_spacePerSoundDataBar - 18 + pitchWaveMinPos.x,
                        -(m_selectedSFX.SoundData[i].Pitch - PitchStart) * PixelPerPitch
                        + pitchWaveMaxPos.y - 50,
                        50, 50),
                    m_notes[note] + $" {octave}");
            }
        }

        void MouseDrawPitchAndVolume(EventType eventType, int pitchWaveGridPosX, int pitchWaveGridPosY,
            int volumeGridPosX,
            int volumeGridPosY)
        {
            if (m_grid > 1)
            {
                if ((pitchWaveGridPosX-m_gridOffset) % m_grid != 0)
                    return;
            }
            if (pitchWaveGridPosX >= 0 && pitchWaveGridPosX < SoundDataMaxLength && pitchWaveGridPosY >= 0 &&
                pitchWaveGridPosY <= HalfTones)
            {
                GUI.FocusControl(k_pitchAreaControl);
                Event.current.Use();

                if (!IsSelectionTool)
                {
                    RecordUndoDirty(m_config, "Pitch Paint");

                    m_selectedSFX.SoundData[pitchWaveGridPosX].Pitch = GetValidPitch(PitchStart + pitchWaveGridPosY);
                    m_selectedSFX.SoundData[pitchWaveGridPosX].WaveForm = (int) m_waveForm;
                    m_selectedSFX.SoundData[pitchWaveGridPosX].Effect = m_fx;
                    
                    if (m_selectedSFX.SoundData[pitchWaveGridPosX].Volume == 0)
                        m_selectedSFX.SoundData[pitchWaveGridPosX].Volume = 3;

                    m_multiSelection.Clear();
                    m_lastIdxMin = pitchWaveGridPosX;
                    m_lastIdxMax = pitchWaveGridPosX;
                }
                else
                    MouseSelectBars(eventType, pitchWaveGridPosX);

                m_lastClicked = PaintArea.PitchWaveform;

                UpdateVolumeTexture();
                UpdatePitchAndWaveformTexture();
            }
            else if (volumeGridPosX >= 0 && volumeGridPosX < SoundDataMaxLength && volumeGridPosY >= 0 &&
                     volumeGridPosY <= VolumeLevels)
            {
                GUI.FocusControl(k_volumeAreaControl);
                Event.current.Use();

                if (!IsSelectionTool)
                {
                    RecordUndoDirty(m_config, "Volume Paint");

                    var soundData = m_selectedSFX.SoundData[volumeGridPosX];
                    soundData.Volume = volumeGridPosY;
                    
                    if (soundData.WaveForm == (int) WaveForm.Select || volumeGridPosY == 0)
                        soundData.WaveForm = (int) m_waveForm;

                    soundData.Pitch = GetValidPitch(soundData.Pitch);

                    m_selectedSFX.SoundData[volumeGridPosX] = soundData;
                    
                    m_multiSelection.Clear();
                    m_lastIdxMin = volumeGridPosX;
                    m_lastIdxMax = volumeGridPosX;
                }
                else
                    MouseSelectBars(eventType, volumeGridPosX);

                m_lastClicked = PaintArea.Volume;

                UpdatePitchAndWaveformTexture();
                UpdateVolumeTexture();
            }
            else if (pitchWaveGridPosX >= SoundDataMaxLength) // off-side click, deselect controls
            {
                GUI.FocusControl("");
                Event.current.Use();
            }
        }

        int GetValidPitch(int pitch, int jumpDir = 1, bool fixedDir=false)
        {
            var min = m_selectedSFX.OctaveMin * 12;
            var max = m_selectedSFX.OctaveMax * 12;
            var clamped = Mathf.Clamp(pitch, min, max);
            if (!m_useScaleFilter)
                return clamped;

            var note = clamped % 12;
            var isValid = m_scaleFilter[note];

            var jumps = 0;
            var jumpAmount = 1;
            while (!isValid && jumps < 12)
            {
                pitch += jumpDir * jumpAmount;
                clamped = Mathf.Clamp(pitch, min, max);
                note = clamped % 12;

                if (!fixedDir)
                {
                    jumpDir *= -1;
                    jumpAmount++;
                }

                jumps++;
                isValid = m_scaleFilter[note];
            }
            
            return clamped;
        }
        
        void DrawPlayPositionMarker(Rect pitchWaveRect, Rect volumeRect, string audioGoName)
        {
            if (AudioObject == null)
                return;
            if (AudioObject.timeSamples >= AudioObject.clip.samples)
                return;
            if (!AudioObject.name.Contains(audioGoName))
                return;

            var samples = AudioObject.timeSamples;
            var samplesPerSoundData = SamplesPerSoundData;
            var playIdx = m_sectionPlayStart + (float) samples / samplesPerSoundData;

            // GUI.contentColor = Color.white;
            var plPos = pitchWaveRect.min + playIdx * k_spacePerSoundDataBar * Vector2.right;
            var height = volumeRect.max.y - plPos.y;
            EditorGUITools.DrawRect(new Rect(plPos, new Vector2(2, height)), Color.white);
        }

        void SectionLoopGUI()
        {
            using (var hs = new EditorGUILayout.HorizontalScope())
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var secMinF = (float) m_selectedSFX.SectionMin;
                    var secMaxF = (float) m_selectedSFX.SectionMax;

                    GUI.SetNextControlName("Section");
                    EditorGUILayout.MinMaxSlider(ref secMinF, ref secMaxF, 0,
                        SoundDataMaxLength, GUILayout.Width(k_spacePerSoundDataBar * SoundDataMaxLength + 10));

                    if (check.changed)
                    {
                        RecordUndoDirty(m_config, "Section changed");
                        m_selectedSFX.SectionMin = Mathf.RoundToInt(secMinF);
                        m_selectedSFX.SectionMax = Mathf.RoundToInt(secMaxF);
                        UpdatePitchAndWaveformTexture();
                    }
                }

                // using (var check = new EditorGUI.ChangeCheckScope())
                // {
                var loopContent = new GUIContent("", "Loop");
                var loop = EditorGUILayout.Toggle(loopContent, m_selectedSFX.Loop, RetroMusicSFXStyles.Loop.Style, GUILayout.Width(20f), GUILayout.Height(20f));
                EditorGUILayout.LabelField($"Section {m_selectedSFX.SectionMin}:{m_selectedSFX.SectionMax}", GUILayout.Width(100f));
            
                if (loop == m_selectedSFX.Loop)
                    return;
            
                GUI.FocusControl("");
                RecordUndoDirty(m_config, "Loop changed");
                m_selectedSFX.Loop = loop;
                GUI.FocusControl("");
                // }
            }
        }
 
        void MouseSelectBars(EventType eventType, int pitchWaveGridPosX)
        {
            if (eventType == EventType.MouseDown)
            {
                if (Event.current.shift)
                    AddLastSelectionToMultiSelection();
                else m_multiSelection.Clear();

                m_selectionStart = pitchWaveGridPosX;
                m_lastIdxMin = pitchWaveGridPosX;
                m_lastIdxMax = pitchWaveGridPosX;
            }   
            else
            {
                m_lastIdxMax = Mathf.Max(pitchWaveGridPosX, m_selectionStart);
                m_lastIdxMin = Mathf.Min(pitchWaveGridPosX, m_selectionStart);
            }
        }

        void AddLastSelectionToMultiSelection()
        {
            if (m_lastIdxMin != -1 && m_lastIdxMax != -1)
            {
                for (var i = m_lastIdxMin; i <= m_lastIdxMax; i++)
                {
                    if (!m_multiSelection.Contains(i))
                        m_multiSelection.Add(i);
                }
            }

            m_multiSelection.Sort();
            
            m_lastIdxMin = -1;
            m_lastIdxMax = -1;
        }

        void SelectWaveformGUI()
        {
            using (var hs = new GUILayout.HorizontalScope())
            {
                using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    var waveformValues = Enum.GetValues(typeof(WaveForm));
                    for (var i = 0; i < waveformValues.Length; i++)
                    {
                        var wf = (WaveForm) waveformValues.GetValue(i);
                        WaveformBtn(i, wf, wf >= WaveForm.Custom);
                    }
                }
                // GUILayout.Space(10);

                using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    var fxValues = Enum.GetValues(typeof(FX));
                    for (var i = 0; i < fxValues.Length; i++) FXBtn((FX) i);
                }

                if (m_fx != FX.None) 
                    FXSettingsGUI();
                GUILayout.FlexibleSpace();
            }
        }

        void FXSettingsGUI()
        {
            if (m_fx == FX.ARP)
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("FX-Speed", GUILayout.Width(65f));
                    GUI.SetNextControlName("FX-Speed");
                    m_selectedSFX.FXSettings.ARPSpeed = EditorGUILayout.IntSlider(m_selectedSFX.FXSettings.ARPSpeed, 2, 8, GUILayout.Width(220f));
                }
            }
            else if (m_fx == FX.Drop)
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Max Range", GUILayout.Width(65f));
                    GUI.SetNextControlName("DropMaxAmount");
                    m_selectedSFX.FXSettings.DropMaxAmount = EditorGUILayout.IntSlider(m_selectedSFX.FXSettings.DropMaxAmount, 
                        2, 96, GUILayout.Width(220f));
                }

                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    var dropPitch = m_notes[m_selectedSFX.FXSettings.DropPitch % 12];
                    var dropOct = m_selectedSFX.FXSettings.DropPitch / 12;
                    EditorGUILayout.LabelField($"Pitch {dropPitch}{dropOct}", GUILayout.Width(65f));
                    GUI.SetNextControlName("DropPitch");
                    m_selectedSFX.FXSettings.DropPitch = EditorGUILayout.IntSlider(m_selectedSFX.FXSettings.DropPitch, 
                        0, 96, GUILayout.Width(220f));
                }
            }
        }

        void WaveformBtn(int i, WaveForm wf, bool custom)
        {
            var active = wf == m_waveForm || (custom && m_waveForm >= WaveForm.Custom);

            // $"{wf}"
            var style = RetroMusicSFXStyles.Waveform[(int) wf].Style ?? EditorStyles.toolbarButton;
            if (active != GUILayout.Toggle(active, "", style, GUILayout.Width(32f), GUILayout.Height(32f)))
            {
                m_waveForm = custom ? wf + m_customForm : wf;
                if (Event.current.shift)
                    ChangeSelectedInstruments(m_waveForm);
            }

            if (!active)
                return;
            if (!custom)
                return;
            
            // using var hs = new GUILayout.HorizontalScope(GUILayout.Height(15f));
            // var selectedCustomForm = m_config.GetCurveColor(m_customForm);
            using (var vs =
                new EditorGUILayout.HorizontalScope(EditorStyles.helpBox, GUILayout.Height(25)))
            {
                CustomWaveBtn(0);
                CustomWaveBtn(1);
                CustomWaveBtn(2);
                CustomWaveBtn(3);
                CustomWaveBtn(4);
                CustomWaveBtn(5);
                CustomWaveBtn(6);
            }
        }
        
        void FXBtn(FX fx)
        {
            var active = fx == m_fx;

            var style = RetroMusicSFXStyles.FX[(int) fx].Style ?? EditorStyles.toolbarButton;
            if (active == GUILayout.Toggle(active, GUIContent.none, style, GUILayout.Width(32f), GUILayout.Height(32f))) 
                return;
            
            m_fx = fx;
            if (Event.current.shift)
                ChangeSelectedFX(m_fx);
        }

        void CustomWaveBtn(int idx)
        {
            var selected = m_customForm == idx;
            var color = selected ? Color.white : BGColorC;
            
            var btnPressed = false;
            using (new ColorScope(color, color))
                GUILayout.Box($"{idx}", RetroMusicSFXStyles.WhiteBoxStyle, GUILayout.Width(32), GUILayout.Height(23));
            
            var r = GUILayoutUtility.GetLastRect();
            var innerRect = new Rect(r.x + 1, r.y + 1, 23 - 2, r.height - 2);
            using (new ColorScope(Color.black))
            {
                if (!selected)
                    btnPressed = GUI.Button(innerRect, GUIContent.none, RetroMusicSFXStyles.WhiteBoxStyle);
                else GUI.Box(innerRect, GUIContent.none, RetroMusicSFXStyles.WhiteBoxStyle);
            }
            
            using (new EditorGUI.DisabledScope(!selected))
            {
                var innerRect2 = new Rect(r.x + 2, r.y + 2, 23 - 4, r.height - 4);
                var curve = EditorGUI.CurveField(innerRect2, m_config.GetCurve(idx));
                m_config.SetCurve(idx, curve);
            }
            
            if (btnPressed)
            {
                m_customForm = idx;
                m_waveForm = WaveForm.Custom + m_customForm;
                if (Event.current.shift)
                    ChangeSelectedInstruments(m_waveForm);
            }
            
            var col = m_config.GetCurveColor(idx);
            
            var colRect = new Rect(r.x + 21, r.y+1, 10, 21);
            var newCol= EditorGUI.ColorField(colRect, GUIContent.none, m_config.GetCurveColor(idx), 
                false, false, false);
            if (newCol != col)
                m_config.SetCurveColor(idx, newCol);
            
            GUILayout.Space(2);
        }
        
        void DataGUI()
        {
            for (var i = 0; i < m_selectedSFX.SoundData.Length; i++)
            {
                using (var hs = new GUILayout.HorizontalScope())
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    GUI.SetNextControlName("Pitch");
                    var pitch = (int) EditorGUILayout.Slider("Pitch", m_selectedSFX.SoundData[i].Pitch, PitchStart,
                        PitchEnd);
                    GUI.SetNextControlName("Volume");
                    var volume = (int) EditorGUILayout.Slider("Volume", m_selectedSFX.SoundData[i].Volume, 0, VolumeLevels);
                    var waveform = (int) (WaveForm) EditorGUILayout.EnumPopup("Waveform",
                        (WaveForm) m_selectedSFX.SoundData[i].WaveForm);
                    var fx = (FX) EditorGUILayout.EnumPopup("FX", m_selectedSFX.SoundData[i].Effect);

                    if (!check.changed)
                        continue;
                    RecordUndoDirty(m_config, "Data changed");
                    m_selectedSFX.SoundData[i].Pitch = pitch;
                    m_selectedSFX.SoundData[i].Volume = volume;
                    m_selectedSFX.SoundData[i].WaveForm = waveform;
                    m_selectedSFX.SoundData[i].Effect = fx;
                }
            }
        }

        #endregion

        #region GUI Actions

        public void Select(RetroSFXConfig config, int i)
        {
            m_config = config;
            SelectSFX(i);
        }

        void SelectSFX(int i)
        {
            if (i < 0 || m_config.Count <= i)
                return;

            GUI.FocusControl("");
            m_editName = false;
            m_selectedSFX = m_config[i];
            UpdatePitchAndWaveformTexture();
            UpdateVolumeTexture();
        }
        
        void UpdateKeys()
        {
            if (focusedWindow != this)
            {
                m_lastFrameKey = Time.realtimeSinceStartup;
                return;
            }
            if (Event.current.type != EventType.KeyDown)
                return;
            
            var kc = Event.current.keyCode;
            if (kc == KeyCode.A && Event.current.control)
            {
                SelectAll();
                return;
            }
            if (!SelectionValid)
                return;
            if (kc == KeyCode.Escape)
                SelectMode();
            
            if (!CanUpdateKeys())
                return;

            if (kc == KeyCode.W || kc == KeyCode.UpArrow)
                UpPressed();
            else if (kc == KeyCode.A || kc == KeyCode.LeftArrow)
                LeftPressed();
            else if (kc == KeyCode.D || kc == KeyCode.RightArrow)
                RightPressed();
            else if ((kc == KeyCode.S && !Event.current.control) || kc == KeyCode.DownArrow)
                DownPressed();
            else if (kc == KeyCode.Delete)
                DeletePressed();
            else if (kc == KeyCode.C && Event.current.control)
                CopyPressed();
            else if (kc == KeyCode.X && Event.current.control)
                CutPressed();
            else if (kc == KeyCode.V && Event.current.control)
                PastePressed();
        }
        
        static bool CanUpdateKeys()
        {
            var focusName = GUI.GetNameOfFocusedControl();

            var focusNameOk = focusName == "";
            focusNameOk |= string.Equals(focusName, k_pitchAreaControl);
            focusNameOk |= string.Equals(focusName, k_volumeAreaControl);
            
            return focusNameOk;
        }
        
        void SelectAll()
        {
            m_multiSelection.Clear();
            if (m_lastIdxMin == 0 && m_lastIdxMax == SoundDataMaxLength - 1)
            {
                m_lastIdxMin = -1;
                m_lastIdxMax = -1;
            }
            else
            {
                m_lastIdxMin = 0;
                m_lastIdxMax = SoundDataMaxLength-1;                
            }

            UpdatePitchAndWaveformTexture();
        }

        bool KeyFrameOK()
        {
            if (m_lastFrameKey + 0.15f > Time.realtimeSinceStartup)
                return false;

            m_lastFrameKey = Time.realtimeSinceStartup;
            return true;
        }

        void DownPressed()
        {
            if (!KeyFrameOK())
                return;

            var shift = Event.current.shift;
            Event.current.Use();

            var val = shift ? -4 : -1;

            ModifyAtCursor(val);
            UpdateTextureAtCursor();
        }

        void UpPressed()
        {
            if (!KeyFrameOK())
                return;

            var shift = Event.current.shift;
            Event.current.Use();

            var val = shift ? 4 : 1;

            ModifyAtCursor(val);
            UpdateTextureAtCursor();
        }

        void RightPressed()
        {
            if (!KeyFrameOK())
                return;

            var shift = Event.current.shift;
            var ctrl = Event.current.control;

            Event.current.Use();
            var val = shift ? 4 : 1;

            if (ctrl)
                MoveBars(val);
            else
            {
                MoveCursor(val);
                UpdateTextureAtCursor();
            }
        }
        
        void LeftPressed()
        {
            if (!KeyFrameOK())
                return;

            var shift = Event.current.shift;
            var ctrl = Event.current.control;

            Event.current.Use();
            var val = shift ? -4 : -1;

            if (ctrl)
                MoveBars(val);
            else
            {
                MoveCursor(val);
                UpdateTextureAtCursor();
            }
        }

        void DeletePressed()
        {
            if (!KeyFrameOK())
                return;
            
            GUI.FocusControl("");
            Event.current.Use();
            RecordUndoDirty(m_config, "Delete");

            Delete();
        }

        void Delete()
        {
            foreach (var i in Selection)
            {
                if (i < 0 || i >= m_selectedSFX.SoundData.Length)
                    continue;

                m_selectedSFX.SoundData[i].WaveForm = (int) WaveForm.Select;
                m_selectedSFX.SoundData[i].Pitch = 0;
                m_selectedSFX.SoundData[i].Volume = 0;
            }

            UpdatePitchAndWaveformTexture();
            UpdateVolumeTexture();
        }

        void CutPressed()
        {
            if (!KeyFrameOK())
                return;

            Event.current.Use();

            Copy();
            Delete();
        }

        void CopyPressed()
        {
            if (!KeyFrameOK())
                return;

            Event.current.Use();
            // Debug.Log("Copy pressed");

            Copy();
        }

        void Copy()
        {
            // multi-selection not supported for copy, show to user
            m_multiSelection.Clear();
            
            var count = m_lastIdxMax - m_lastIdxMin + 1;
            m_copyData = new SoundData[count];
            for (var i = 0; i < count; i++)
                m_copyData[i] = m_selectedSFX.SoundData[m_lastIdxMin + i];
            
            UpdatePitchAndWaveformTexture();
            UpdateVolumeTexture();
        }

        void PastePressed()
        {
            Event.current.Use();
            
            m_multiSelection.Clear();
            m_lastIdxMax = m_lastIdxMin;
            // Debug.Log("Paste pressed");

            GUI.FocusControl("");
            RecordUndoDirty(m_config, $"Paste Data {m_copyData.Length}");
            for (var i = 0; i < m_copyData.Length; i++)
                m_selectedSFX.SoundData[m_lastIdxMin + i] = m_copyData[i];

            UpdatePitchAndWaveformTexture();
            UpdateVolumeTexture();
        }

        void MoveBars(int val)
        {
            AddLastSelectionToMultiSelection();
            var minSelection = m_multiSelection.Min();
            var maxSelection = m_multiSelection.Max();
            
            if (maxSelection + val >= m_selectedSFX.SoundData.Length)
                return;
            if (minSelection + val < 0)
                return;
            
            GUI.FocusControl("");
            RecordUndoDirty(m_config, "Move Bars");

            foreach (var minMax in SelectionsMinMax)
            {
                var targetIdxMin = minMax.x + val;
                var targetIdxMax = minMax.y + val;

                var min = Mathf.Min(minMax.x, targetIdxMin);
                var max = Mathf.Max(minMax.y, targetIdxMax);
            
                var count = max - min + 1;
                m_tempData = new SoundData[count];

                for (var i = min; i <= max; i++)
                    m_tempData[i - min] = m_selectedSFX.SoundData[i];

                for (var i = min; i <= max; i++)
                {
                    var moveIdx = (count + i - min - val) % count;
                    m_selectedSFX.SoundData[i] = m_tempData[moveIdx];
                }
            }

            MoveCursor(val);
            
            UpdatePitchAndWaveformTexture();
            UpdateVolumeTexture();
        }

        void MoveCursor(int val)
        {
            if (m_lastIdxMin != -1 && m_lastIdxMax != -1)
            {
                if (m_lastIdxMax + val >= m_selectedSFX.SoundData.Length)
                    return;
                if (m_lastIdxMin + val < 0)
                    return;

                m_lastIdxMin = Mathf.Clamp(m_lastIdxMin + val, 0, m_selectedSFX.SoundData.Length);
                m_lastIdxMax = Mathf.Clamp(m_lastIdxMax + val, 0, m_selectedSFX.SoundData.Length);
            }

            for (var i = 0; i < m_multiSelection.Count; i++)
                m_multiSelection[i] = Mathf.Clamp(m_multiSelection[i] + val, 0, m_selectedSFX.SoundData.Length);
        }

        void UpdateTextureAtCursor()
        {
            if (m_lastClicked == PaintArea.PitchWaveform)
                UpdatePitchAndWaveformTexture();
            else
                UpdateVolumeTexture();
        }

        void ModifyAtCursor(int val)
        {
            var modifyStr = m_lastClicked == PaintArea.PitchWaveform ? "Pitch" : "Volume";
            GUI.FocusControl("");
            RecordUndoDirty(m_config, $"Modify {modifyStr}");

            foreach (var i in Selection)
            {
                if (m_lastClicked == PaintArea.PitchWaveform)
                    m_selectedSFX.SoundData[i].Pitch = GetValidPitch(m_selectedSFX.SoundData[i].Pitch + val, (int) Mathf.Sign(val), true);
                else
                    m_selectedSFX.SoundData[i].Volume =
                        Mathf.Clamp(m_selectedSFX.SoundData[i].Volume + val, 0, VolumeLevels);
            }
        }

        void PauseOrPlay()
        {
            if (AudioObject != null)
            {
                if (AudioObject.isPlaying)
                    AudioObject.Pause();
                else 
                    AudioObject.UnPause();
            }
            else
                Play();
        }
        
        void StopOrPlay()
        {
            if (AudioObject != null)
                DestroyAudio();
            else
                Play();
        }

        void Stop()
        {
            if (AudioObject != null)
                DestroyAudio();
        }

        void Play()
        {
            m_sectionPlayStart = m_selectedSFX.SectionMin;
            BuildSamples();
            CreateAndPlayClip();
            m_waveTex = CreateAudioClipTexture(SampleRange.ToArray(), m_pitchAndWaveformTexture.width, 50, Color.gray, BGColorB);
        }

        void ExportSFX(string path)
        {
            BuildSamples();
            var clip = CreateClip(m_samples, m_selectedSFX.Name);
            SavWav.Save(path, clip);
            AssetDatabase.Refresh();
        }

        void CreateAndPlayClip()
        {
            m_clip = CreateClip(SampleRange, "Clip-" + Time.frameCount);
            PlayAudioClip(k_sfxGo, m_clip, m_selectedSFX.Loop);
        }

        void BuildSamples() => BuildSamplesSFX(m_selectedSFX, m_samples, m_config);

        const int k_fxPixel = 16;
        int PitchWaveformTexSupposedHeight => (HalfTones + 1) * PixelPerPitch + k_fxPixel;
        
        void UpdatePitchAndWaveformTextureIfNeeded()
        {
            var supposedHeight =PitchWaveformTexSupposedHeight;
            if (m_pitchAndWaveformTexture != null && m_pitchAndWaveformTexture.height == supposedHeight)
                return;
            m_pitchAndWaveformTexture = new Texture2D(k_spacePerSoundDataBar * SoundDataMaxLength,
                supposedHeight, TextureFormat.RGBA32, false);
            UpdatePitchAndWaveformTexture();
        }

        void UpdatePitchAndWaveformTexture()
        {
            var supposedHeight = PitchWaveformTexSupposedHeight;
            if (m_pitchAndWaveformTexture == null || m_pitchAndWaveformTexture.height != supposedHeight)
            {
                m_pitchAndWaveformTexture = new Texture2D(k_spacePerSoundDataBar * SoundDataMaxLength,
                    supposedHeight, TextureFormat.RGBA32, false);
            }

            const int xOff = 4;
            const int colWidth = 8;
            
            for (var xi = 0; xi < m_pitchAndWaveformTexture.width; xi++)
            for (var yi = 0; yi < k_fxPixel; yi++)
                m_pitchAndWaveformTexture.SetPixel(xi, yi, BGColorA);

            for (var xi = 0; xi < m_pitchAndWaveformTexture.width; xi++)
            for (var yi = k_fxPixel; yi < m_pitchAndWaveformTexture.height; yi++)
            {
                var gridPos = m_grid > 1 && (xi - 6 - k_spacePerSoundDataBar * m_gridOffset) % (k_spacePerSoundDataBar * m_grid) == 0;
                gridPos |= (yi - k_fxPixel) % (12 * PixelPerPitch)==0;

                var alternate = (yi - k_fxPixel) % (2 * PixelPerPitch) >= PixelPerPitch;
                
                var col =  gridPos ? BGColorC : alternate ? BGColorA : BGColorB;
                m_pitchAndWaveformTexture.SetPixel(xi, yi, col);
            }

            for (var i = 0; i < SoundDataMaxLength; i++)
            {
                var selected = Selection.Contains(i) && m_lastClicked == PaintArea.PitchWaveform;
                
                var sound = m_selectedSFX.SoundData[i];

                var octave = Mathf.FloorToInt((float) sound.Pitch / 12);
                var outsideOfOctaveRange = octave < m_selectedSFX.OctaveMin || octave >= m_selectedSFX.OctaveMax;
                    
                var custom = sound.WaveForm >= (int) WaveForm.Custom;
                var customIdx = sound.WaveForm - (int) WaveForm.Custom;
                var inactive = sound.Volume == 0 || outsideOfOctaveRange;
                var color = inactive ? InactiveColor
                    : custom
                        ? m_config.GetCurveColor(customIdx)
                        : WaveFormColor[sound.WaveForm];

                var xFullBarStart = k_spacePerSoundDataBar * i;
                var xFullBarEnd = xFullBarStart + k_spacePerSoundDataBar;
                
                var xStart = k_spacePerSoundDataBar * i + xOff;
                var xEnd = xStart + colWidth;

                if (m_selectedSFX.SectionMin == i
                    || m_selectedSFX.SectionMax == i)
                {
                    for (var yi = 0; yi < m_pitchAndWaveformTexture.height; yi++)
                        m_pitchAndWaveformTexture.SetPixel(xStart - 4, yi, Color.grey);
                }

                if (sound.Effect != FX.None && !inactive)
                {
                    var icon = RetroMusicSFXStyles.FXSmallIcn[(int) sound.Effect];
                    m_pitchAndWaveformTexture.SetPixels(xFullBarStart, 0, k_fxPixel, k_fxPixel, 
                        icon.GetPixels(0, 0, k_fxPixel, k_fxPixel));
                }

                var yEnd = k_fxPixel + (sound.Pitch - PitchStart + 1) * PixelPerPitch;
                for (var yi = k_fxPixel; yi < yEnd; yi++)
                {
                    var colFin = 0.5f * color;
                    if (yi > yEnd - PixelPerPitch)
                        colFin = selected ? Color.white : color;
                    for (var xi = xStart; xi < xEnd; xi++)
                        m_pitchAndWaveformTexture.SetPixel(xi, yi, colFin);
                }
            }

            m_pitchAndWaveformTexture.Apply();
        }

        void UpdateVolumeTextureIfNeeded()
        {
            if (m_volumeTexture != null)
                return;
            m_volumeTexture = new Texture2D(k_spacePerSoundDataBar * SoundDataMaxLength,
                k_pixelPerVolume * (VolumeLevels + 1),
                TextureFormat.RGBA32, false);
            UpdateVolumeTexture();
        }

        void UpdateVolumeTexture()
        {
            if (m_volumeTexture == null)
                return;
            const int xOff = 7;
            const int colWidth = 8;

            for (var xi = 0; xi < m_volumeTexture.width; xi++)
            for (var yi = 0; yi < m_volumeTexture.height; yi++)
            {
                var gridPos = m_grid > 1 && (xi-6 - k_spacePerSoundDataBar * m_gridOffset) % (k_spacePerSoundDataBar * m_grid) == 0;

                var alternate = yi % (2 * k_pixelPerVolume) >= k_pixelPerVolume;
                var col = gridPos ? BGColorC : alternate ? BGColorA : BGColorB;
                m_volumeTexture.SetPixel(xi, yi, col);
            }

            for (var i = 0; i < SoundDataMaxLength; i++)
            {
                var selected = Selection.Contains(i) && m_lastClicked == PaintArea.Volume;

                var sound = m_selectedSFX.SoundData[i];
                var color = sound.WaveForm == (int) WaveForm.Select
                    ? InactiveColor
                    : VolumeColors[sound.Volume];

                var xStart = k_spacePerSoundDataBar * i + xOff;
                var xEnd = xStart + colWidth - 6;

                var xStartFinal = xStart - 6;
                var xEndFinal = xEnd + 6;

                var yEnd = (sound.Volume + 1) * k_pixelPerVolume;

                for (var yi = 0; yi < yEnd; yi++)
                {
                    var colFin = 0.5f * color;
                    if (yi > yEnd - k_pixelPerVolume)
                    {
                        xStart = xStartFinal;
                        xEnd = xEndFinal;
                        colFin = selected ? Color.white : color;
                    }

                    for (var xi = xStart; xi < xEnd; xi++)
                        m_volumeTexture.SetPixel(xi, yi, colFin);
                }
            }

            m_volumeTexture.Apply();
        }

        void ChangeSFX(int steps) =>
            SelectSFX(Mathf.Clamp(m_config.IndexOf(m_selectedSFX) + steps, 0, m_config.Count - 1));

        void NextInstrument(int dir)
        {
            var wf = (int) m_waveForm;
            wf += dir;
            
            while (wf > (int) LastShape && wf < (int) WaveForm.Custom) 
                wf += dir;
            
            wf = Mathf.Clamp(wf, 0, (int) WaveForm.Custom + 6);
            if (wf >= (int) WaveForm.Custom)
                m_customForm = wf - (int) WaveForm.Custom;
            
            m_waveForm = (WaveForm) wf;
        }
        
        void SelectionToSection()
        {
            GUI.FocusControl("");
            RecordUndoDirty(m_config, "Modify Section");

            m_selectedSFX.SectionMin = Selection.Min();
            m_selectedSFX.SectionMax = Selection.Max();
            UpdatePitchAndWaveformTexture();
        }
        
        
        void FlattenSelection()
        {
            var avg = (int) (from i in Selection 
                where m_selectedSFX.SoundData[i].Volume != 0 
                select m_selectedSFX.SoundData[i].Pitch).Average();

            var pitch = GetValidPitch(avg);
            foreach (var i in Selection)
            {
                if (m_selectedSFX.SoundData[i].Volume == 0)
                   continue; 
                m_selectedSFX.SoundData[i].Pitch = pitch;
            }
            UpdatePitchAndWaveformTexture();
        }

        void SelectMode()
        {
            if (!KeyFrameOK())
                return;
            
            Event.current.Use();
            m_editName = false;
            GUI.FocusControl("");
            m_waveForm = WaveForm.Select;
        }
        
        void SwitchView() => m_view = (View) (((int) m_view + 1) % 2);

        void ModDuration(int dir, int steps)
        {
            GUI.FocusControl("");
            RecordUndoDirty(m_config, "Modify Duration");

            m_selectedSFX.Duration = Mathf.Clamp(m_selectedSFX.Duration + dir * steps, 1, 255);
        }

        void ChangeSelectedInstruments(WaveForm wf)
        {
            GUI.FocusControl("");
            RecordUndoDirty(m_config, "Change Instrument");

            var instrumentWillChange = Selection.Any(si => m_selectedSFX.SoundData[si].WaveForm != (int) wf);
            var activateSilent = !instrumentWillChange;
            foreach (var i in Selection)
            {
                m_selectedSFX.SoundData[i].WaveForm = (int) wf;
                if (!activateSilent || m_selectedSFX.SoundData[i].Volume > 0)
                    continue;
                m_selectedSFX.SoundData[i].Volume = 3;
            }
            UpdatePitchAndWaveformTexture();
        }

        void ChangeSelectedFX(FX fx)
        {
            GUI.FocusControl("");
            RecordUndoDirty(m_config, "Change FX");

            foreach (var i in Selection)
                m_selectedSFX.SoundData[i].Effect = fx;

            UpdatePitchAndWaveformTexture();
        }

        #endregion
    }
}