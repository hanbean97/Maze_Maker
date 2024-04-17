using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoWayCheck : PathFind
{
    [SerializeField]Button Bt;
    private void Start()
    {
        Bt.onClick.AddListener(check);
    }
    private void Update()
    {
    }
    public void check()
    {
        AsrarAlgo.instance.Wallcheck();
        if(PathFinding(AsrarAlgo.instance.StartPos, AsrarAlgo.instance.TargetPos) == false)
        {
            Debug.Log("모든길이 막혀있음");
        }
    }
    private void OnDrawGizmos()
    {
        if(FinalNodeList != null && FinalNodeList.Count != 0)
        {
            for (int i = 0; i < FinalNodeList.Count-1; i++)
            {
                Gizmos.DrawLine(FinalNodeList[i].nodePosition, FinalNodeList[i+1].nodePosition);
            }
        }
    }
}
