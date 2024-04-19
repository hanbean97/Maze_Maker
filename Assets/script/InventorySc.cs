using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class InventorySc : MonoBehaviour
{
    bool nowInvenPossible;
    [SerializeField]List<itemData> itemList = new List<itemData>();// ��� ������ ����
     Dictionary<string,string> inven = new Dictionary<string,string>(); //�÷��̾ ������ �ִ� ������ ���� �����Ұ�
    [SerializeField]Image inventoryImager;
    Animation inventoryanim;
    [SerializeField]int inventorySize;
    
    void Start()
    {
        defaltInventorySetting();
    }
    void Update()
    {
        
    }
    public void Nowinven(bool _answer)
    {
        nowInvenPossible = _answer;
    }
    void InvenSave()//���ӵ��� ����
    {
        string jsonData = JsonUtility.ToJson(inven,true);// ���� true�� ����� �б� �������·� ��ȭ
        string path = Path.Combine(Application.dataPath + "itemData.json");//��ü = ��ΰ� �ü������ �ٸ����־ +�� �����ϰ�Combine���� ��ü
        File.WriteAllText(path, jsonData);// (���,������)
    }
    void InvenLoad()//���� ���۽� �ҷ��� üũ
    {
        string path = Path.Combine(Application.dataPath, "itemData.json");
        string jsonData = File.ReadAllText(path);
        if(jsonData != null)
        {
            inven = JsonUtility.FromJson<Dictionary<string,string>>(jsonData);
        }
    }
    void defaltInventorySetting()
    {
        for(int i=0; i< inventorySize;i++)
        {
            inven.Add($"{i}","");// �κ��丮 �ʱ� ���� ""�� ������ ������ ����.
        }
    }
}
[System.Serializable]
class itemData
{
    public GameObject GameObject;
    public string Name;  
}

