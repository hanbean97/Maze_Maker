using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GiftChoice : MonoBehaviour
{
    [SerializeField] Button[] choiceBT;
    [SerializeField]Image[] slotImage;
    TMP_Text[] MonsterName;
    int[] choicearry;
    int slotcount;
    void Awake()
    {
        slotcount = choiceBT.Length;
        MonsterName = new TMP_Text[slotcount];
        for (int i = 0; i < choiceBT.Length; i++) 
        {
            int index =i;
            choiceBT[i].onClick.AddListener(() =>SelectPressBT(index));
            MonsterName[i] = slotImage[i].GetComponentInChildren<TMP_Text>();
        }
    }
    void Update()
    {
    }
    public void SetImage()
    {
        choicearry = new int[] { Random.Range(0, GameManager.instance.MonsterLists.Count), Random.Range(0, GameManager.instance.MonsterLists.Count), Random.Range(0, GameManager.instance.MonsterLists.Count) };
        for (int i = 0; i < slotcount; i++)
        {
            slotImage[i].sprite = GameManager.instance.MonsterLists[choicearry[i]].GetComponentInChildren<SpriteRenderer>().sprite;
            MonsterName[i].text = $"{GameManager.instance.MonsterLists[choicearry[i]].name}";
        }
    }
    void SelectPressBT(int index)
    {
        GameManager.instance.GiftItem(choicearry[index]);
       gameObject.SetActive(false);
    }
}
