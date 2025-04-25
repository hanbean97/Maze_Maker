using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("#BGM")]
    [SerializeField] AudioClip[] bgmCilp;
    [SerializeField] float bgmVolume;
    AudioSource bgmPlayer;
    [Header("#SFX")]
    [SerializeField] AudioClip[] sfxCilps;
    [SerializeField] float sfxVolume;
    [SerializeField] int channels;
    AudioSource[] sfxPlayers;
    int channelIndex;
    [SerializeField] Slider BgmSlider;
    [SerializeField] Slider SfxSlider;
    //클립들을 이넘 순서에 맞게 저장
    public enum Bgm { }
    public enum Sfx { }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        Init();
        SettingInit();
    }
    void Init()
    {
        GameObject bgmObj = new GameObject("BgmPlayer");
        bgmObj.transform.parent = transform;
        bgmPlayer = bgmObj.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        bgmPlayer.clip = bgmCilp[0];

        GameObject sfxObj = new GameObject("sfxPlayer");
        sfxObj.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            sfxPlayers[i] = sfxObj.AddComponent<AudioSource>();
            sfxPlayers[i].playOnAwake = false;
            sfxPlayers[i].volume = sfxVolume;
        }
    }
    void SettingInit()
    {
        BgmSlider.value = bgmVolume;
        SfxSlider.value = sfxVolume;
    }
    public void PlayBgm(Bgm bgm)
    {
        bgmPlayer.clip = bgmCilp[(int)bgm];
        bgmPlayer.Play();
    }
    public void StopBgm(Bgm bgm)
    {
        bgmPlayer.Stop();
    }
    public void PlaySfx(Sfx sfx, int RandSound = 0)
    {
        int LoopIndex = 0;
        for (int index = 0; index < channels; index++)
        {
            LoopIndex = (index + channelIndex) % channels;

            if (sfxPlayers[LoopIndex].isPlaying)
                continue;

            channelIndex = LoopIndex;

            switch (RandSound)
            {
                case 0:
                    sfxPlayers[LoopIndex].clip = sfxCilps[(int)sfx];
                    break;

                case > 0:

                    sfxPlayers[LoopIndex].clip = sfxCilps[(int)sfx + Random.Range(0, RandSound)];
                    break;
            }
            sfxPlayers[LoopIndex].Play();
            break;
        }
    }
    public void BgmVolumeChange(float volume)
    {
        bgmPlayer.volume = volume;
    }
    public void SfxVolumeChange(float volume)
    {
        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            sfxPlayers[i].volume = volume;
        }
    }
    public void BgmSliderChange()
    {
        BgmVolumeChange(BgmSlider.value);
    }
    public void SfxSliderChange()
    {
        SfxVolumeChange(SfxSlider.value);
    }
}
