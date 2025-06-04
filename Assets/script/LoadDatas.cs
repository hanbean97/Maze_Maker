using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadDatas : MonoBehaviour
{

    public static LoadDatas instance;
    List<(string,int)> ranking = new List<(string,int)>();
    public List<(string, int)> Rlists {get{ return ranking; }  }
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
        ranking.Add((name,score));
        ranking.Sort((a, b) => b.Item2.CompareTo(a.Item2));
        if (ranking.Count > maxrank)
        {
            ranking.RemoveAt(maxrank+1);
        }
    }
   
}

