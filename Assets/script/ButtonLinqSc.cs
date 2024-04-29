using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonLinqSc : MonoBehaviour
{
    bool gamestrat = false;
    bool isinvenBt = false;
    bool iswallmode =false;
    [SerializeField] GameObject startBt;
    [SerializeField] GameObject wallBt;
    [SerializeField] GameObject wallModeBt;
    [SerializeField] Animation invneanim;
    AnimationState anistate;
    WallmakeSc wallch;
    void Awake()
    {
        anistate = invneanim["inventoryanim"];
        wallch = GetComponent<WallmakeSc>();
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
            if(isinvenBt ==false && wallch.wallModeOn == false && gamestrat == false)
            {
                isinvenBt = true;
                startBt.SetActive(false);
                invneanim.Play("inventoryanim");
                anistate.speed = 1;
            }
            else if(isinvenBt == true && wallch.wallModeOn == false && gamestrat == false)
            {
                isinvenBt = false;
                startBt.SetActive(true);
                invneanim.Play("inventoryanim");
                anistate.speed = -1;
                anistate.time = anistate.length;
            }
    }
    void GameStartCh()
    {
        if ( gamestrat == false && GameManager.instance.IsGamStart == true)//한번만 실행시키기위해
        {
            gamestrat = true;
            wallBt.SetActive(false);
            startBt.SetActive(false);
            wallModeBt.SetActive(false);
            if (isinvenBt == true)
            {
                OpenInventory();
            }
        }
        else if (gamestrat == true && GameManager.instance.IsGamStart == false)
        {
            gamestrat = false;
            startBt.SetActive(true);
            wallBt.SetActive(true);
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
}
