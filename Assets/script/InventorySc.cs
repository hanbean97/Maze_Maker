using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class InventorySc : MonoBehaviour
{
    bool nowInvenPossible;
    [SerializeField]List<itemData> itemList = new List<itemData>();// 모든 아이템 정보
     Dictionary<string,string> inven = new Dictionary<string,string>(); //플레이어가 가지고 있는 아이템 정보 저장할곳
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
    void InvenSave()//게임도중 저장
    {
        string jsonData = JsonUtility.ToJson(inven,true);// 끝에 true는 사람이 읽기 좋은형태로 변화
        string path = Path.Combine(Application.dataPath + "itemData.json");//합체 = 경로가 운영체제마다 다를수있어서 +를 사용안하고Combine으로 합체
        File.WriteAllText(path, jsonData);// (경로,데이터)
    }
    void InvenLoad()//게임 시작시 불러와 체크
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
            inven.Add($"{i}","");// 인벤토리 초기 설정 ""은 아이템 정보가 없다.
        }
    }
}
[System.Serializable]
class itemData
{
    public GameObject GameObject;
    public string Name;  
}

