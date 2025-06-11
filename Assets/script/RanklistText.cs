using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RanklistText : MonoBehaviour
{
    [SerializeField] GameObject rankprefab;
    [SerializeField] Transform contenst;
    void Start()
    {
        if (LoadDatas.instance.Rlists.Count == 0) return;

        for (int i =0; i<LoadDatas.instance.Rlists.Count; i++)
        {
            TextMeshProUGUI[] RankText = Instantiate(rankprefab, contenst).GetComponentsInChildren<TextMeshProUGUI>();
            RankText[0].text = LoadDatas.instance.Rlists[i].Item1;
            RankText[1].text = LoadDatas.instance.Rlists[i].Item2.ToString();
        }
    }
        
}
