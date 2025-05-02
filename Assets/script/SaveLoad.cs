using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveLoad 
{
    public static void SaveGame()
    {
        string path = Application.persistentDataPath + "Datas.crr";

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream fs = new FileStream(path, FileMode.Create);
        SaveLoadData Datas = new SaveLoadData();
        formatter.Serialize(fs, Datas);
        fs.Close();
    }
    public static SaveLoadData LoadGame()
    {
        string path = Application.persistentDataPath + "Datas.crr";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fs = new FileStream(path, FileMode.Open);
            SaveLoadData Datas = formatter.Deserialize(fs) as SaveLoadData;
            fs.Close();
            return Datas;
        }
        else
        {
            return null;
        }
    }
}
