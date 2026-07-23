using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Enemy : move
{
    enum EnemyTypelist
    {
        meleeE,
        RangedE
    }
    [SerializeField]EnemyTypelist enemytype;
    enum Movestate
    {
        pathmove,
        attackReady,
        attack
    }
    Movestate nowmove = Movestate.pathmove;
    [SerializeField] float Searchrange = 3;
    [SerializeField] float attackrange;
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
   /* protected override void Start()
    {
        base.Start();

    }*/
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
            if (i == 0)//몬스터가 없는걸 스스로 알기 위해서
            {
                nullcheckcount = 0;
            }
            dir = GameManager.instance.NowMonstertrs[i].position - transform.position;
            RaycastHit2D rays = Physics2D.Raycast(transform.position, dir.normalized, Searchrange, LayerMask.GetMask("Wall", "Monster"));
            if (rays && rays.transform.CompareTag("Monster"))//몬스터에게 레이를 쏘고
            {
                if (targetEnemy != null)//지정된 타깃과 다른 타깃의 거리를 계산하고 가장 가까운 타깃을 지정
                {
                    if (Vector2.Distance(transform.position, targetEnemy.position) > Vector2.Distance(transform.position, rays.transform.position))
                    {
                        targetEnemy = rays.transform;
                    }
                }
                else if (targetEnemy == null)//지금 지정된 타깃이 없다면
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
    void FindingEnemy()//적공격
    {//이 부분에서 타깃 지정 슬롯 적용 타깃 주변에서
        if (startTileOn == false) return;//시작지점에서는 액션금지


        if(targetEnemy != null && Vector3.Distance(transform.position, targetEnemy.position) <= Searchrange)//서치 사거리에 들었고 공격 위치를 잡을 땟
        {
            nowmove = Movestate.attackReady;
            Vector2 dir = targetEnemy.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            angle = (angle + 360f) % 360f;

            int slot = Mathf.RoundToInt(angle / 45f) % 8; // 0~45: 0 , 45~90: 1, 90~180:2 ...

            for (int i = 0; i < 8; i++)
            {
                new Vector2();
                           
                
            }

            if (Vector3.Distance(transform.position, targetEnemy.position) < attackrange)
            {
                 nowmove = Movestate.attack;
                
            }

        }
        else if(targetEnemy == null || Vector3.Distance(transform.position, targetEnemy.position) > Searchrange)//타깃이 널이거나 사거리 밖일 때
        {
            ismoveway = true; // A*알고리즘 이동 ON
            attackStop();
            nowmove = Movestate.pathmove;
        }

        NowStateMode();

    }

    void NowStateMode()
    {
        switch(nowmove)
        {
            case Movestate.pathmove:
                ismoveway = true;
                attackStop();
                break;
            case Movestate.attackReady://레이를 쏴서 몬스터의 슬롯을 정하고 자리이동
                ismoveway = false;
                int a = 0;
                for (int i = 0; i < 8; i++)
                {
                    a = a + (i * (i % 2 == 1 ? -1 : 1));
                    
                }
                break;
            case Movestate.attack:
                ismoveway = false;
                attackGo();
                break;
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
        SoundManager.instance.PlaySfx(SoundManager.Sfx.hit);
    }
}

