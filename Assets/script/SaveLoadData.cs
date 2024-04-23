using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class SaveLoadData 
{
    bool[,] wallData;
    Dictionary<string, (string, Vector3Int)> MonsterData;
    int LevelData;
    
    public void Save(SaveLoadData _save)
    {
        wallData = _save.wallData;
        MonsterData = _save.MonsterData;
        LevelData = _save.LevelData;

       string path = JsonUtility.ToJson(_save);

    }
}
