using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DonDestinform : MonoBehaviour
{
    public static DonDestinform Instance;
    bool DontnewGame = false;
    public bool DontNewGame { get { return DontnewGame; } set { DontnewGame = value; } }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
        }
        DontDestroyOnLoad(gameObject);

    }
    

}
