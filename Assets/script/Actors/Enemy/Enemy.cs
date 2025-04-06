using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Enemy : move
{

    [SerializeField] float Searchrange = 3;
    [SerializeField] float attakrange;
    protected Transform targetEnemy;
    public Transform Target { get { return targetEnemy; } set{ targetEnemy = value; } }
    Vector2Int targetPos;
    int count;
    int nullcheckcount;
    Vector3 dir;
    bool startTileOn = false;
    bool endTileOn = false;
    private void OnEnable()
    {
        this.PathFinding(AsrarAlgo.instance.StartPos, AsrarAlgo.instance.TargetPos);

    }
    protected virtual void Update()
    {
        if (endTileOn == false)//?????????????? ??????????????
        {
            if ((transform.position.x > AsrarAlgo.instance.TargetPos.x - 1 && transform.position.x < AsrarAlgo.instance.TargetPos.x + 1) &&
                               (-transform.position.y > AsrarAlgo.instance.TargetPos.y - 1 && -transform.position.y < AsrarAlgo.instance.TargetPos.y + 1))
            {
                endTileOn = true;
            }
            if (startTileOn == true && targetEnemy == null)
            {
                
                Moving(AsrarAlgo.instance.TargetPos);
            }
            else if (startTileOn == true && targetEnemy != null)
            {
                targetPos = new Vector2Int(Mathf.RoundToInt(targetEnemy.position.x),Mathf.RoundToInt(-targetEnemy.position.y));//????????????
                Moving(targetPos);
            }
            else if (startTileOn == false)
            {
                Moving();
                if (transform.position.y < 0 || (transform.position.x > 9 || transform.position.x< 6 ))
                {
                    startTileOn = true;
                }
            }
            SearchEnemy();
            FindingEnemy();
        }
        else if (endTileOn == true)
        {
            EndAction();
        }
    }
    void SearchEnemy()
    {
        if (startTileOn == false) return;
        count = GameManager.instance.NowMonstertrs.Count;
        for (int i = 0; i < count; i++)
        {
            if (i == 0)
            {
                nullcheckcount = 0;
            }
            dir = GameManager.instance.NowMonstertrs[i].position - transform.position;
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
            else if (!rays || rays.transform.CompareTag("Wall"))
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
            ismoveway = false;
            attackGo();
        }
        else if(targetEnemy == null || Vector3.Distance(transform.position, targetEnemy.position) > attakrange)
        {
            ismoveway = true;
            attackStop();
        }
    }

    protected virtual void attackGo()
    {

    }
    protected virtual void attackStop()
    {

    }
    void EndAction()
    {
       // GameManager.instance.EndPos
        nextdir = GameManager.instance.EndPos.position - transform.position;
        transform.position += nextdir.normalized *  Time.deltaTime;
        if(Vector3.Distance(transform.position,GameManager.instance.EndPos.position) < 0.2f ) 
        {
            gameObject.SetActive(false);
            GameManager.instance.EnemyFinshDungeon(transform);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("AttackBox") && collision.gameObject.layer !=gameObject.layer)//???? ???? ?????? ???????????????? ????
        {
            Hp -= collision.GetComponent<HitDamageSc>().GetDamage;
            if(collision.GetComponent<projectileSc>() !=null)// ?????? ?????????????? ????
            {
                Destroy(collision.gameObject);
            }
            HitMotion();
            Death();
        }
    }
    void Death()
    {
        if (Hp <= 0 && isdeth == false)
        {
            isdeth = true;
            gameObject.SetActive(false);
            GameManager.instance.DeathEnemy(transform);
        }
    }
    void HitMotion()//???????? ??????????+????
    {
        anim.SetTrigger("Hit");
    }
}

