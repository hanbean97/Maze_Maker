using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SoundSlider : MonoBehaviour
{
    SoundManager soundM;
    [SerializeField] Slider Bgmslider;
    [SerializeField] Slider SfxsSlider;
    private void Start()
    {
        soundM = SoundManager.instance;
        Bgmslider.value = soundM.BgmVolume;
        SfxsSlider.value = soundM.SfxVolume;
    }

    private void Update()
    {
        soundM.BgmVolumeChange(Bgmslider.value);
        soundM.SfxVolumeChange(SfxsSlider.value);
    }
}
