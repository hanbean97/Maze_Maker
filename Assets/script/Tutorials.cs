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
        maintext.text = "MazeMaker은 던전을 만들어 몰려오는 적을 막는 디펜스 게임입니다..";
        timer += Time.deltaTime;
        if(timer > 5)
        {
            next++;
            timer = 0;
        }
    }
    void root1()
    {
        maintext.text = "적이 최대한 많은시간 탐험하게 해야 더 많은 점수를 얻습니다.";
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
            maintext.text = "미완성벽이 있습니다.";
        }
    }
    void root2()
    {
        wallBt.enabled = true;
        maintext.text = " Wall 버튼을 눌러보세요.";
        if (wallbuttonCheck == true)
        {
            next++; 
        }
    }
    void root3()
    {
        if(ismissingmessage ==false)
        {
            maintext.text = "바닥을 클릭해 벽을 생성하세요. 벽삭제는 make버튼을 눌러 삭제모드로 변환이 가능합니다. \n미완성 벽없이 미로를 완성하셨다면 Wall버튼을 한번더 눌러주세요";
        }
        if (wallbuttonCheck == false)
        {
            wallBt.onClick.RemoveListener(wallButtonch);
            next++;
        }
    }
    void root4()
    {
        maintext.text = "i 키를 눌러 인벤토리를 열어보세요.";
        if(Input.GetKeyDown(KeyCode.I))
        {
            next++;
        }
    }
    void root5()
    {
        maintext.text = "인벤토리에 있는 몬스터를 드래그해 바닥에 설치하여 소환하시고 스타트 버튼을 눌러 디펜스를 시작하세요";
        startBt.enabled = true;
        timer += Time.deltaTime;
        if (timer > 7)
        {
            GameManager.instance.Firstgame = false;
            maintext.gameObject.SetActive(false);
            timer = 0;
            PlayerPrefs.SetInt("NewGame",1);
        }
    }


}
