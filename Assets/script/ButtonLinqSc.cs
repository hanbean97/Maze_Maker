using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonLinqSc : MonoBehaviour
{
    bool gamestart = false;
    bool isinvenBt = false;
    bool iswallmode =false;
    [SerializeField] GameObject startBt;
    [SerializeField] Button wallBt;
    [SerializeField] GameObject wallModeBt;
    [SerializeField] Animation invneanim;
    AnimationState anistate;
    WallmakeSc wallch;
    [SerializeField] Button mainmenuscene;
    [SerializeField] Button mainmenuGoBt;
    void Awake()
    {
        anistate = invneanim["inventoryanim"];
        wallch = GetComponent<WallmakeSc>();
        mainmenuscene.onClick.AddListener(backmainmenu);
        mainmenuGoBt.onClick.AddListener(backmainmenu);
    }
    void Update()
    {
        OnWall();
        GameStartCh();
        if (Input.GetKeyDown(KeyCode.I))
        {
            OpenInventory();
        }
    }
    void OpenInventory()
    {
            if(isinvenBt ==false && wallch.wallModeOn == false && gamestart == false)
            {
                wallBt.enabled = false; 
               isinvenBt = true;
                startBt.SetActive(false);
                invneanim.Play("inventoryanim");
                anistate.speed = 1;
            }
            else if(isinvenBt == true && wallch.wallModeOn == false && gamestart == false)
            {
            wallBt.enabled = true;
            isinvenBt = false;
                startBt.SetActive(true);
                invneanim.Play("inventoryanim");
                anistate.speed = -1;
                anistate.time = anistate.length;
            }
    }
    void GameStartCh()
    {
        if ( gamestart == false && GameManager.instance.IsGamStart == true)//한번만 실행시키기위해
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
    void backmainmenu()
    {
        SceneManager.LoadSceneAsync(0);
    }
}
