using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedViews : MonoBehaviour
{
    [SerializeField] int setWidth = 1920;//���ϴ� ���� �ʺ�
    [SerializeField] int setHeight = 1080;//���ϴ� ���� ����
    int deviceWidth = Screen.width; // ���ʺ�
    int deviceHeight = Screen.height;//��� ����

    void Start()
    {
      //  SetReasolution1();
    }
    public void SetReasolution1()
    {
        //Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth), true);//SetResolution
        Screen.SetResolution((int)(((float)deviceWidth / deviceHeight) * setHeight), setHeight, true);//SetResolution ��������
        if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight)//����� �ػ󵵺� ��ū���
        {
            float newWidth = ((float)setWidth / setWidth) / ((float)deviceWidth / deviceHeight);//���ο�ʺ�
            Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f);// ���ο� Rect����
        }
        else // ������ �ػ� �� ��ū���
        {
            float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight); // ���ο����
            Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight);// ���ο� Rect����
        }
    }
}
