using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : move
{
    [SerializeField] int Hp;
   
   
    private void OnEnable()
    {
        this.PathFinding(AsrarAlgo.instance.startPos, AsrarAlgo.instance.targetPos);
    }
    private void Update()
    {
        Moving();
    }

}

