using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScene : MonoBehaviour
{
    [SerializeField] Image fade;
    [SerializeField] Button startBt;
    bool isstart = false;
    float timmer;
    [SerializeField] float nextscenetime;
    [SerializeField] Button MenuOpenBt;
    [SerializeField] Button MenuCloseBt;
    [SerializeField] Button QuitBt;
    [SerializeField] GameObject MenuPanel;
    [SerializeField] Button RankOpenBt;
    [SerializeField] Button RankCloseBt;
    [SerializeField] GameObject RankPanel;
    void Start()
    {
        startBt.onClick.AddListener(nextScecnbool);
        MenuOpenBt.onClick.AddListener(OpenMenu);
        MenuCloseBt.onClick.AddListener(CloseMenu);
        QuitBt.onClick.AddListener(QuitGmae);
        RankOpenBt.onClick.AddListener(onRankPanel);
        RankCloseBt.onClick.AddListener(offRankPanel);

        SoundManager.instance.PlayBgm(SoundManager.Bgm.start);
    }

    void Update()
    {
        if(isstart == true)
        {
            timmer += Time.deltaTime;
            fade.color = new Color(fade.color.r, fade.color.g, fade.color.b,Mathf.Lerp(0,1, timmer/ nextscenetime));
            if (timmer > nextscenetime)
            {
                isstart = false;
                SceneManager.LoadSceneAsync(1);
            }

        }
    }
    private void nextScecnbool()
    {
        isstart = true;
    }

    private void OpenMenu()
    {
        MenuPanel.SetActive(true);
    }
    private void CloseMenu()
    {
        MenuPanel.SetActive(false);
    }
    private void QuitGmae()
    {
        Application.Quit();
    }
   private void onRankPanel()
    {
        RankPanel.SetActive(true);
    }
    private void offRankPanel()
    {
        RankPanel.SetActive(false);
    }
}
