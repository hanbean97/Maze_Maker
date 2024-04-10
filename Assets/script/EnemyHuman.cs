using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHuman : MonoBehaviour
{
    [SerializeField]AsrarAlgo algo;
    [SerializeField] float speed;
    int nownode;
    void Start()
    {
        
    }
    void Update()
    {
        if(algo.FinalNodeList.Count >0l)
        {
          //  algo.FinalNodeList[nownode].nodePosition
           // this.transform.position += new Vector3()*speed*Time.deltaTime;

        }
    }
}
