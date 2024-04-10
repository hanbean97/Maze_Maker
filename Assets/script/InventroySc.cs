using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventroySc : MonoBehaviour
{
    List<string> inven = new List<string>();
    [SerializeField]List<itemData> itemList = new List<itemData>();
    void Start()
    {
        
    }
    void Update()
    {
        
    }






}

struct itemData
{
    public GameObject GameObject;
    public string Name;  
}

