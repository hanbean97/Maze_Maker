using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnumSpace;



public class Monster : move
{
    [SerializeField] Vector2 size;
    [SerializeField] float Searchrange;
    [SerializeField] float attakrange;
    [SerializeField]MonsterType monstertype;
    public MonsterType MonT { get { return monstertype; } }
    int count =0;
    int nullcheckcount = 0;
    Vector3 dir;
    protected Transform targetEnemy;
    Vector2Int targetPos;
    Vector3Int mysponPos;
    public Vector3Int MyPos {get{ return mysponPos; }set{ mysponPos = value; } }
    SpriteRenderer spriteRenderer;
    public SpriteRenderer MySprR { get { return spriteRenderer; } }
    protected bool ishit;
    [SerializeField] float hitmotionTime;
    [SerializeField] Color hitcolor;
    Color baseColors;
    float hitTimer=0;
    private void OnEnable()
    {
        //mysponPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        spriteRenderer =GetComponentInChildren<SpriteRenderer>();
        baseColors = spriteRenderer.color;
    }
    void Update()
    {
        SearchEnemy();
        FindingEnemy();
        if(targetEnemy != null)
        {
           targetPos = new Vector2Int(Mathf.RoundToInt(targetEnemy.position.x), -Mathf.RoundToInt(targetEnemy.position.y));
           Moving(targetPos);
        }
        hitMotion();
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
            else if(!rays || rays.transform.CompareTag("Wall"))
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
            // anim.SetBool("Attack", true); ?????????????? ????
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("AttackBox") && collision.gameObject.layer != gameObject.layer)
        {
            Hp -= collision.GetComponent<HitDamageSc>().GetDamage;
            if(targetEnemy == null)
            {
                int count = GameManager.instance.Nowenemytrs.Count;
                for (int i = 0; i < count; i++)
                {
                    if (GameManager.instance.Nowenemytrs[i].gameObject.activeSelf == true)
                    {
                        targetEnemy = GameManager.instance.Nowenemytrs[i];
                    }
                }
            }
            if (collision.GetComponent<projectileSc>() != null)
            {
                Destroy(collision.gameObject);
            }
            ishit = true;
            Death();
        }
    }
    void Death()
    {
        if (Hp <= 0 && isdeth == false)
        {
            isdeth = true;
            GameManager.instance.DeathMonster(this.transform);
            Destroy(gameObject);
        }
    }
    protected virtual void hitMotion()
    {
        if(ishit ==true)
        {
            hitTimer += Time.deltaTime;

            spriteRenderer.color = hitcolor ;

            if(hitTimer > hitmotionTime)
            {
                spriteRenderer.color = baseColors;
                ishit = false;
                hitTimer = 0;
            }
        }
    }
    
}
