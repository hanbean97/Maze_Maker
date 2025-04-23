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
        maintext.text = "MazeMaker�� �̷θ� ����� ���͸� ��ȯ�ؼ� �������� ���� ���� �����Դϴ�.";
        timer += Time.deltaTime;
        if(timer > 5)
        {
            next++;
            timer = 0;
        }
    }
    void root1()
    {
        maintext.text = "������ ������ ���� �������� �� ���� ������ ����ϴ�.";
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
            maintext.text = "�̿ϼ��� ���� �ֽ��ϴ�.";
        }
    }
    void root2()
    {
        wallBt.enabled = true;
        maintext.text = " Wall ��ư�� ���� ������.";
        if (wallbuttonCheck == true)
        {
            next++; 
        }
    }
    void root3()
    {
        if(ismissingmessage ==false)
        {
            maintext.text = "make��ư�� ���� ��带 �ٲܼ� �ֽ��ϴ�. make���� ���� �����ϰ� erase���� ���� ����ϴ�. \n �̷θ� �ϼ��ϼ̴ٸ� Wall��ư�� �����ּ���.";
        }
        if (wallbuttonCheck == false)
        {
            wallBt.onClick.RemoveListener(wallButtonch);
            next++;
        }
    }
    void root4()
    {
        maintext.text = "i Ű�� ���� �κ��丮�� ���� ������.";
        if(Input.GetKeyDown(KeyCode.I))
        {
            next++;
        }
    }
    void root5()
    {
        maintext.text = "�κ��丮 ���� ���͸� �巡���� ���ϴ� �ٴڿ� ���͸� ��ȯ�ϰ� Start��ư�� ���� ������ �����ϼ���.";
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
