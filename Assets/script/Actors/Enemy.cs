using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : move
{

    [SerializeField] float Searchrange = 3;
    [SerializeField] float attakrange;
    protected Transform targetEnemy;
    Vector2Int targetPos;
    int count;
    int nullcheckcount;
    Vector3 dir;
    bool startTileOn=false;
    bool endTileOn =false;
    private void OnEnable()
    {
        this.PathFinding(AsrarAlgo.instance.StartPos, AsrarAlgo.instance.TargetPos);
        
    }
    protected void Update()
    { 
        switch(endTileOn)
        {
            case false:
                if((transform.position.x>AsrarAlgo.instance.TargetPos.x && transform.position.x < AsrarAlgo.instance.TargetPos.x+1) &&
                    (-transform.position.y > AsrarAlgo.instance.TargetPos.y&& -transform.position.y< AsrarAlgo.instance.TargetPos.y+1)
                    )
                {
                    endTileOn = true; 
                    break;
                }

                if (startTileOn == true && targetEnemy == null)
                {
                    Moving(AsrarAlgo.instance.TargetPos);
                }
                else if (startTileOn == true && targetEnemy != null)
                {
                    targetPos = new Vector2Int(Mathf.RoundToInt(targetEnemy.position.x), -Mathf.RoundToInt(targetEnemy.position.y));//의미없어보임
                    Moving(targetPos);
                }
                else if (startTileOn == false)
                {
                    Moving();
                    if (-Mathf.RoundToInt(transform.position.y) > 0)
                    {
                        startTileOn = true;
                    }
                }
                SearchEnemy();
                FindingEnemy();
                break;
             case true:
                EndAction();
                break;
        }
      
    }

    void SearchEnemy()
    {
        if (startTileOn==false) return;

        count = GameManager.instance.Nowenemytrs.Count;
        for (int i = 0; i < count; i++)
        {
            if (i == 0)
            {
                nullcheckcount = 0;
            }
            dir = GameManager.instance.Nowenemytrs[i].position - transform.position;
            RaycastHit2D rays = Physics2D.Raycast(transform.position, dir.normalized, Searchrange, LayerMask.GetMask("Wall", "Monster"));
            if (rays && rays.transform.CompareTag("Monster"))
            {
                if (targetEnemy != null)
                {
                    if (Vector2.Distance(transform.position, targetEnemy.position) > Vector2.Distance(transform.position, rays.transform.position))
                    {
                        targetEnemy = rays.transform;
                    }
                }
                else if (targetEnemy == null)
                {
                    targetEnemy = rays.transform;
                }
            }
            else if (!rays)
            {
                nullcheckcount++;
            }
            if (nullcheckcount == count)
            {
                targetEnemy = null;
            }
        }
    }
    void FindingEnemy()
    {
        if (startTileOn == false) return;

        if (targetEnemy != null && Vector3.Distance(transform.position, targetEnemy.position) < attakrange)
        {
            attackGo();
            ismoveway = false;
        }
        else
        {
            attackStop();
            ismoveway = true;
        }
    }
   // protected interface move
  
     protected virtual void attackGo()
    {

    }
    protected virtual void attackStop()
    {

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("AttackBox") && LayerMask.Equals(collision, transform) == false)
        {
            Hp -= collision.transform.GetComponent<HitDamageSc>().GetDamage; 
            Death();
        }
    }
    void Death()
    {
        if (Hp <= 0 && isdeth ==false)
        {
            isdeth = true;
            GameManager.instance.DeathEnemy(transform);
            Destroy(gameObject);
        }
    }
    void EndAction()
    {

    }
}

