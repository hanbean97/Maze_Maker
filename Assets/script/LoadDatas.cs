using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadDatas : MonoBehaviour
{

    public static LoadDatas instance;
    List<(string,int)> ranking = new List<(string,int)>();
   public List<(string, int)> Rlists { get; set; }
    public int maxrank;
    void Start()
    {

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
        }

        DontDestroyOnLoad(this);    
    }

    public void RankAdd(string name, int score)
    {
        if (ranking[ranking.Count-1].Item2 < score)
        {
            if(score >1)
            {
                ranking.RemoveAt(ranking.Count-1);
            }
            ranking.Add((name,score));

        }
    }
   
}

