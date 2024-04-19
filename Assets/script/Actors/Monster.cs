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
    bool attackgo;
    Vector3 dir;
    Transform targetEnemy;
    [SerializeField] Collider2D attackbox;

    void Start()
    {
        
    }
    void Update()
    {
        DefaltMovePattern();
        SearchEnemy();
        FindingEnemy();
    }
    void DefaltMovePattern()
    {
        if (targetEnemy == null)
        {
            monstertype = MonsterType.defalt;
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
            dir = transform.position - GameManager.instance.Nowenemytrs[i].position;
            RaycastHit2D rays = Physics2D.Raycast(transform.position, dir.normalized, Searchrange, ~LayerMask.GetMask("Ground","Monster","AttackBox"));
            if(rays&& rays.transform.CompareTag("Enemy"))
            {
                targetEnemy = rays.transform;
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
            attackgo = true;
        }
        else
        {
            attackgo= false;
        }

        if(targetEnemy != null && attackgo == false )
        {
            anim.SetBool("Attack", false);
            AroundSetPos(new Vector2Int (Mathf.RoundToInt(targetEnemy.position.x),Mathf.RoundToInt(targetEnemy.position.y)));
            Moving();
        }
        else if(attackgo == true)
        {
            anim.SetBool("Attack",true);
        }
    }
    protected void AttackOn()
    {
        attackbox.gameObject.SetActive(true);
    }
    protected void AttackOff()
    {
        attackbox.gameObject.SetActive(false);
    }
}
