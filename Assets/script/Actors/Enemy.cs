using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : move
{

    [SerializeField] float Sreachrange;
    Transform targetEnemy;
    int targetEnemyCount;
    private void OnEnable()
    {
        this.PathFinding(AsrarAlgo.instance.StartPos, AsrarAlgo.instance.TargetPos);
        
    }
    private void Update()
    {
        Moving();
        FindTarget();
    }
    void Death()
    {
        if (Hp > 0)
        {
            GameManager.instance.DeathEnemy(transform);
        }
    }

    void FindTarget()
    {
        targetEnemyCount = GameManager.instance.NowMonstertrs.Count;
        for (int i =0; i<targetEnemyCount; i++)
        {
            Vector3 dir = transform.position- GameManager.instance.NowMonstertrs[i].position ;
            RaycastHit2D ray = Physics2D.Raycast(transform.position, dir.normalized, Sreachrange);
            
        }
    }
}

