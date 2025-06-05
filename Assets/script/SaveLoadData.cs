using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public  class SaveLoadData 
{
    public List<(string, int)> RankLists;
    public SaveLoadData()
    {
        RankLists = LoadDatas.instance.Rlists;
    }
}
