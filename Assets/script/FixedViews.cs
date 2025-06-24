using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FixedViews : MonoBehaviour
{
    
    [Header("화면비")]
    [SerializeField] bool istargetRatio;
    [SerializeField] int[] targetRatio = new int[2];
    [SerializeField] bool ishasHz;
    [SerializeField] Toggle fullscreenToggle;
    [SerializeField] TMP_Dropdown resolutiondropdown;
    List<Resolution> resolutions;
    public int ResolutionIndex { get { return PlayerPrefs.GetInt("ResolutionIndex", 0); } set { PlayerPrefs.SetInt("ResolutionIndex", value); } }
    public bool isFullscreen { get { return PlayerPrefs.GetInt("isFull", 1) == 1; } set { PlayerPrefs.SetInt("isFull", value ? 1 : 0); } }

    public void Start()
    {
        resolutiondropdown.onValueChanged.AddListener(DropDownOptionChange);
        fullscreenToggle.onValueChanged.AddListener(FullscreenToggleChange);

#if !UNITY_EDITOR
       Invoke(nameof(SetReasolution), 0.1f);
#endif
    }
    public void SetReasolution()
    {
        resolutions = new List<Resolution>(Screen.resolutions);
        resolutions.Reverse();

        if (istargetRatio)
        {
            resolutions = resolutions.FindAll(x => (float)x.width / x.height == (float)targetRatio[0] / targetRatio[1]);
        }

        if (ishasHz && resolutions.Count > 0)
        {
            List<Resolution> tempresolutions = new List<Resolution>();
            int curWidth = resolutions[0].width;
            int curHeight = resolutions[0].height;
            tempresolutions.Add(resolutions[0]);

            foreach (Resolution resolution in resolutions)
            {
                if (curWidth != resolution.width || curHeight != resolution.height)
                {
                    tempresolutions.Add(resolution);
                    curWidth = resolution.width;
                    curHeight = resolution.height;
                }
            }
            resolutions = tempresolutions;
        }

        List<string> options = new List<string>();
        foreach (Resolution resolution in resolutions)
        {
            string option = $"{resolution.width} x {resolution.height}";
            if (ishasHz)
            {
                option += $"{resolution.refreshRateRatio} Hz";
            }
            options.Add(option);
        }
        resolutiondropdown.ClearOptions();
        resolutiondropdown.AddOptions(options);

        resolutiondropdown.value = ResolutionIndex;
        fullscreenToggle.isOn = isFullscreen;


    }

    public void DropDownOptionChange(int resolutionIndex)
    {
        ResolutionIndex = resolutionIndex;
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRateRatio);

    }
    public void FullscreenToggleChange(bool isfull)
    {
        isFullscreen = isfull;
        Screen.fullScreen = isfull;
    }

}
