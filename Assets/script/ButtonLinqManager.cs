using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonLinqManager : MonoBehaviour
{
    WallmakeSc wallsc;
    InventroySc invensc;
    CamerMovingSc cammovesc;
    [SerializeField] Button wallBt;
    [SerializeField] Button invenscBt;
    bool nowWallMode;
    bool nowInvenMode;
    private void Awake()
    {
        wallsc = GetComponent<WallmakeSc>();
        invensc = GetComponent<InventroySc>();
        cammovesc = GetComponent<CamerMovingSc>();
        //wallBt.onClick.AddListener();
        //invenscBt.onClick.AddListener(invenOn);
    }
    void Start()
    {
        
    }
    void Update()
    {
        
    }
    void buttonMana()
    {

    }
    void invenOn()
    {
        wallsc.NowWall(false);
        
    }


}
