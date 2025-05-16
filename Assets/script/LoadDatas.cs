using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadDatas : MonoBehaviour
{

    public static LoadDatas instance;
    List<RankingClass> ranking = new List<RankingClass>();
    public List<RankingClass> rank { get { return ranking; }
        set {
            RankingClass rankclass = new RankingClass();

             //   ranking.Add(rankclass);
        } }

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

   
}

public class RankingClass
{
    public string name;
    public int score;
}