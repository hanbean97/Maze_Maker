using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventroySc : MonoBehaviour
{
    bool nowInvenPossible;
    List<string> inven = new List<string>();
    [SerializeField]List<itemData> itemList = new List<itemData>();
    void Start()
    {
        
    }
    void Update()
    {
        
    }
    public void Nowinven(bool _answer)
    {
        nowInvenPossible = _answer;
    }




}

struct itemData
{
    public GameObject GameObject;
    public string Name;  
}

