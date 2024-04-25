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
        if (endTileOn == false)//마지막타일안에 들어가기전까지
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
                targetPos = new Vector2Int(Mathf.RoundToInt(targetEnemy.position.x),Mathf.RoundToInt(-targetEnemy.position.y));//의미없어보임
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
        }
        else if (endTileOn == true)
        {
            EndAction();//마지막타일에 도착했을때 실행
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
        else if(targetEnemy == null || Vector3.Distance(transform.position, targetEnemy.position) > attakrange)
        {
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
    void EndAction()
    {
       // GameManager.instance.EndPos
        nextdir = GameManager.instance.EndPos.position - transform.position;
        transform.position += nextdir.normalized *  Time.deltaTime;
        if(Vector3.Distance(transform.position,GameManager.instance.EndPos.position) < 0.2f ) 
        {
            GameManager.instance.EnemyFinshDungeon(transform);
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("AttackBox") && collision.gameObject.layer !=gameObject.layer)//닿은 히트 박스가 자기팀인지아닌지 체크
        {
            Hp -= collision.GetComponent<HitDamageSc>().GetDamage;
            if(collision.GetComponent<projectileSc>() !=null)// 투사체 스크립트있으면 삭제
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
            GameManager.instance.DeathEnemy(transform);
            Destroy(gameObject);
        }
    }
    void HitMotion()//공격받는 애니메이션+넉백
    {
        anim.SetTrigger("Hit");
    }
}

