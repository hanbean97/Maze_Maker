using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tutorials : MonoBehaviour
{
    [SerializeField] Button wallBt;
    [SerializeField] Button startBt;
    [SerializeField] TMP_Text maintext;
    int next =0;
    float timer;
    bool wallbuttonCheck =false;
    bool ismissingmessage =false;
    [SerializeField] Transform missingwallch;
    void Start()
    {

        if(GameManager.instance.Firstgame == true)
        {
            wallBt.onClick.AddListener(wallButtonch);
        }

    }
    void Update()
    {
        tutorialroot();

    }
    void tutorialroot()
    {
        if (GameManager.instance.Firstgame == false) return;
        
        
        switch(next)
        {
            case 0:
                root0();
                break;
            case 1:
                root1();
                break;
            case 2:
                root2();
                break;
            case 3:
                root3();
                break;
            case 4:
                root4();
                break;
            case 5:
                root5();
                break;

        }
    }
    void root0()
    {
        wallBt.enabled = false;
        startBt.enabled = false;
        maintext.gameObject.SetActive(true);
        maintext.text = "MazeMaker는 미로를 만들고 몬스터를 소환해서 몰려오는 적을 막는 게임입니다.";
        timer += Time.deltaTime;
        if(timer > 5)
        {
            next++;
            timer = 0;
        }
    }
    void root1()
    {
        maintext.text = "적들이 던전에 오래 있을수록 더 많은 점수를 얻습니다.";
        timer += Time.deltaTime;
        if (timer > 5)
        {
            next++;
            timer = 0;
        }
    }
    void wallButtonch()
    {
        if(missingwallch.transform.Find("MissingWall(Clone)") == null)
        {
            wallbuttonCheck = !wallbuttonCheck;
        }
        else
        {
            ismissingmessage = true;
            maintext.text = "미완성된 벽이 있습니다.";
        }
    }
    void root2()
    {
        wallBt.enabled = true;
        maintext.text = " Wall 버튼을 눌러 보세요.";
        if (wallbuttonCheck == true)
        {
            next++; 
        }
    }
    void root3()
    {
        if(ismissingmessage ==false)
        {
            maintext.text = "make버튼을 눌러 모드를 바꿀수 있습니다. make모드는 벽을 생성하고 erase모드는 벽을 지웁니다. \n 미로를 완성하셨다면 Wall버튼을 눌러주세요.";
        }
        if (wallbuttonCheck == false)
        {
            wallBt.onClick.RemoveListener(wallButtonch);
            next++;
        }
    }
    void root4()
    {
        maintext.text = "i 키를 눌러 인벤토리를 열어 보세요."; 
        if(Input.GetKeyDown(KeyCode.I))
        {
            next++;
        }
    }
    void root5()
    {
        maintext.text = "인벤토리 안의 몬스터를 드래그해 원하는 바닥에 몬스터를 소환하고 Start버튼을 눌러 게임을 시작하세요.";
        startBt.enabled = true;
        timer += Time.deltaTime;
        if (timer > 8)
        {
            GameManager.instance.Firstgame = false;
            maintext.gameObject.SetActive(false);
            timer = 0;
            PlayerPrefs.SetInt("NewGame",1);
        }
    }


}
