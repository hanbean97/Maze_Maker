using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : move
{
    [SerializeField] Vector2 size;
    [SerializeField] float Searchrange;
    [SerializeField] float attakrange;
    [SerializeField]MonsterType monstertype;
    int count =0;
    int nullcheckcount = 0;
    Vector3 dir;
    Transform targetEnemy;
    [SerializeField] Collider2D attackbox;
    Vector2Int targetPos;

    void Update()
    {
        DefaltMovePattern();
        SearchEnemy();
        FindingEnemy();
        if(targetEnemy != null)
        {
           targetPos = new Vector2Int(Mathf.RoundToInt(targetEnemy.position.x), -Mathf.RoundToInt(targetEnemy.position.y));
            Moving(targetPos);
        }
    }
    void DefaltMovePattern()
    {
        if (targetEnemy == null)
        {
            switch (monstertype)
            {
                case MonsterType.defalt:
                    
                    break;
                case MonsterType.None:
                    break;
            }
        }
    }

    void SearchEnemy()
    {
        count = GameManager.instance.Nowenemytrs.Count;
        for (int i =0; i<count; i++) 
        {
            if(i ==0)
            {
                nullcheckcount = 0;
            }
            dir = GameManager.instance.Nowenemytrs[i].position- transform.position;
            RaycastHit2D rays = Physics2D.Raycast(transform.position, dir.normalized, Searchrange, ~LayerMask.GetMask("Ground","Monster","OutLine"));
             if(rays&& rays.transform.CompareTag("Enemy"))
            {
                if (targetEnemy != null)
                {
                    if (Vector2.Distance(transform.position,targetEnemy.position) > Vector2.Distance(transform.position, rays.transform.position))
                    {
                    targetEnemy = rays.transform;
                    }
                }
                else if(targetEnemy == null)
                {
                    targetEnemy = rays.transform;
                }
            }
            else if(!rays)
            {
                nullcheckcount++;
            }
            if(nullcheckcount == count)
            {
                targetEnemy = null;
            }
        }
    }
    void FindingEnemy()
    {
        if( targetEnemy != null &&Vector3.Distance(transform.position,targetEnemy.position) <attakrange)
        {
            // anim.SetBool("Attack", true); 상위클래스에서 실행
            attackGo();
            ismoveway = false;
        }
        else
        {
            //  anim.SetBool("Attack",false);
            attackStop();
            ismoveway = true;
        }
    }

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
            if( collision.transform.GetComponent<projectileSc>() != null)
            {
                Destroy(collision.gameObject);
            }
            Death();
        }
    }
    void Death()
    {
        if (Hp > 0 && isdeth == false)
        {
            isdeth = true;
            GameManager.instance.DeathMonster(transform);
            Destroy(transform);
        }
    }
}
