using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class InventorySc : MonoBehaviour
{
    Transform[] invenSlots;
    [SerializeField]GameObject item;
    [SerializeField] GameObject Slot;
    void Start()
    {
        int count = GameManager.instance.MaxInventory;
        invenSlots = new Transform[count];
        GameObject[] monster= GameManager.instance.LoadInInventory();//���ӸŴ����� ����Ǿ��ִ� ���� �ҷ���
        for (int i = 0; i < count; i++)
        {
            invenSlots[i] = Instantiate(Slot,transform).transform;//�ڽ����� �κ��丮 ���λ���
            if (monster[i] != null)
            {
                Dragable items =  Instantiate(item, invenSlots[i]).GetComponent<Dragable>();//������ü�� �������ڽ� ����
                items.SetMonster(monster[i]);
            }

        }
    }
    void Update()
    {
        
    }
     
  /// <summary>
  ///  �κ��丮�� ����ִ� ������ ��ġ�� �׾������� ��������Ʈ ó�� ���� ������ �ҷ��ö� �ѹ�
  /// </summary>
  /// <param name="_inventory"></param>
    public void SetInvetory(GameObject _Monster)
    {
        int count = invenSlots.Length;
        for (int i = 0; i < count; i++)
        {
            if (invenSlots[i].childCount == 0)
            {
                Dragable items = Instantiate(item, invenSlots[i]).GetComponent<Dragable>();//������ü�� �������ڽ� ����
                items.SetMonster(_Monster);
                break;
            }
        }
    }
    void InvenSave()//���ӵ��� ����
    {
        //  string jsonData = JsonUtility.ToJson(inven,true);// ���� true�� ����� �б� �������·� ��ȭ
        // string path = Path.Combine(Application.dataPath + "itemData.json");//��ü = ��ΰ� �ü������ �ٸ����־ +�� �����ϰ�Combine���� ��ü
        // File.WriteAllText(path, jsonData);// (���,������)
    }
    void InvenLoad()//���� ���۽� �ҷ��� üũ
    {
        string path = Path.Combine(Application.dataPath, "itemData.json");
        string jsonData = File.ReadAllText(path);
        if (jsonData != null)
        {
            // inven = JsonUtility.FromJson<Dictionary<string,string>>(jsonData);
        }
    }
}
[System.Serializable]
class itemData
{
    public GameObject GameObject;
    public string Name;  
}

