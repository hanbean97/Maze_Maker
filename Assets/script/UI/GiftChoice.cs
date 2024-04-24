using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GiftChoice : MonoBehaviour
{
    [SerializeField] Button[] choiceBT;
    [SerializeField]Image[] slot;
    int[] choicearry;
    void Start()
    {
        for (int i = 0; i < choiceBT.Length; i++) 
        {
            int index =i;
            choiceBT[i].onClick.AddListener(() =>SelectPressBT(index));
        }

    }
    void Update()
    {
        
    }
    public void SetImage()
    {
        choicearry = new int[] { Random.Range(0, GameManager.instance.MonsterLists.Count), Random.Range(0, GameManager.instance.MonsterLists.Count), Random.Range(0, GameManager.instance.MonsterLists.Count) };
        for (int i = 0; i < 3; i++)
        {
            slot[i].sprite = GameManager.instance.MonsterLists[choicearry[i]].GetComponent<SpriteRenderer>().sprite;
        }
    }
    void SelectPressBT(int index)
    {
       // GameManager.instance choicearry[index];

    }
}
