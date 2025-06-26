using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using TMPro;

public class ButtonLinqSc : MonoBehaviour
{
    bool gamestart = false;
    bool isinvenBt = false;
    bool iswallmode =false;
    [SerializeField] GameObject startBt;
    [SerializeField] Button wallBt;
    [SerializeField] GameObject wallModeBt;
    [SerializeField] Animation invneanim;
    [SerializeField] RectTransform Inventory;
    [SerializeField] RectTransform openinvents;
    [SerializeField] RectTransform closeinvents;
    [SerializeField] float invenRectTime;
    AnimationState anistate;
    WallmakeSc wallch;
    [SerializeField] Button menuopen;
    [SerializeField] Button Menuclose;
    [SerializeField] Button mainmenuGoBt;
    [SerializeField] Button MainMengo;
    [SerializeField] GameObject MenuPanel;
    [SerializeField] TMP_InputField nametextfield;
    private Vector2 velocity = Vector2.zero;
    
    bool isend= false;
    bool isinvening = false;
    
    void Awake()
    {
        
        anistate = invneanim["inventoryanim"];
        wallch = GetComponent<WallmakeSc>();
        menuopen.onClick.AddListener(OpenMenu);
        Menuclose.onClick.AddListener(OpenMenu);
        mainmenuGoBt.onClick.AddListener(Finishgame);
        MainMengo.onClick.AddListener(backmainmenu);
        nametextfield.onValueChanged.AddListener((w) => nametextfield.text = Regex.Replace(w, @"[^0-9a-zA-Z]", ""));
    }
    void Update()
    {
        OnWall();
        GameStartCh();
        if (Input.GetKeyDown(KeyCode.I))
        {
            OpenInventory();
        }

        if(isinvening== true&& isinvenBt == true)
        {
            Inventory.localPosition = Vector2.SmoothDamp(Inventory.localPosition, openinvents.localPosition, ref velocity,invenRectTime);
            if(Mathf.Abs(Inventory.localPosition.x - openinvents.localPosition.x) <10f)
            {
                isinvening = false;
            }
        }
        else if(isinvening == true && isinvenBt == false)
        {
            Inventory.localPosition = Vector2.SmoothDamp(Inventory.localPosition, closeinvents.localPosition, ref velocity, invenRectTime);
            if(Mathf.Abs(Inventory.localPosition.x - closeinvents.localPosition.x) <10f)
            {
               isinvening = false;
            }
        }

    }
    void OpenInventory()
    {
            if(isinvenBt ==false && wallch.wallModeOn == false && gamestart == false)
            {
                wallBt.enabled = false; 
               isinvenBt = true;
                startBt.SetActive(false);
                isinvening = true;
                //invneanim.Play("inventoryanim");
                //anistate.speed = 1;
            }
            else if(isinvenBt == true && wallch.wallModeOn == false && gamestart == false)
            {
            wallBt.enabled = true;
            isinvenBt = false;
            isinvening = true;
                startBt.SetActive(true);
                //invneanim.Play("inventoryanim");
                //anistate.speed = -1;
                //anistate.time = anistate.length;
            }
    }
    void GameStartCh()
    {
        if ( gamestart == false && GameManager.instance.IsGamStart == true)//?????? ??????????????
        {
            gamestart = true;
            wallBt.gameObject.SetActive(false);
            startBt.SetActive(false);
            wallModeBt.SetActive(false);
            if (isinvenBt == true)
            {
                OpenInventory();
            }
        }
        else if (gamestart == true && GameManager.instance.IsGamStart == false)
        {
            gamestart = false;
            startBt.SetActive(true);
            wallBt.gameObject.SetActive(true);
            wallModeBt.SetActive(true);
        }
    }
    void OnWall()
    {
       if (iswallmode ==false && wallch.wallModeOn == true )
        {
            iswallmode = true;
            startBt.SetActive(false);
        }
       else if (iswallmode == true && wallch.wallModeOn == false)
        {
            iswallmode = false;
            startBt.SetActive(true);
        }
    }
    void OpenMenu()
    {
        MenuPanel.SetActive(!MenuPanel.activeSelf);
        Time.timeScale = MenuPanel.activeSelf ? 0:1;
    }
    
    void backmainmenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(0);
    }
    void Finishgame()
    {
        if (isend == true) return;

        isend = true;

        if(GameManager.instance.ChartIn == true)
        {
            if (nametextfield.text == "")
            {
                nametextfield.text = "aaa";
            }

            LoadDatas.instance.RankAdd(nametextfield.text, (int)GameManager.instance.Score);
        }
      
        backmainmenu();
    }
}
