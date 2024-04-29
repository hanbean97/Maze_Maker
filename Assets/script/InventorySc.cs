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
        GameObject[] monster= GameManager.instance.LoadInInventory();//게임매니저에 저장되어있는 정보 불러옴
        for (int i = 0; i < count; i++)
        {
            invenSlots[i] = Instantiate(Slot,transform).transform;//자식으로 인벤토리 슬로생성
            if (monster[i] != null)
            {
                Dragable items =  Instantiate(item, invenSlots[i]).GetComponent<Dragable>();//슬롯전체에 아이템자식 생성
                items.SetMonster(monster[i]);
            }

        }
    }
    void Update()
    {
        
    }
     
  /// <summary>
  ///  인벤토리에 들어있는 아이템 위치와 그아이템의 스프라이트 처음 저장 정보를 불러올때 한번
  /// </summary>
  /// <param name="_inventory"></param>
    public void SetInvetory(GameObject _Monster)
    {
        int count = invenSlots.Length;
        for (int i = 0; i < count; i++)
        {
            if (invenSlots[i].childCount == 0)
            {
                Dragable items = Instantiate(item, invenSlots[i]).GetComponent<Dragable>();//슬롯전체에 아이템자식 생성
                items.SetMonster(_Monster);
                break;
            }
        }
    }
    void InvenSave()//게임도중 저장
    {
        //  string jsonData = JsonUtility.ToJson(inven,true);// 끝에 true는 사람이 읽기 좋은형태로 변화
        // string path = Path.Combine(Application.dataPath + "itemData.json");//합체 = 경로가 운영체제마다 다를수있어서 +를 사용안하고Combine으로 합체
        // File.WriteAllText(path, jsonData);// (경로,데이터)
    }
    void InvenLoad()//게임 시작시 불러와 체크
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

