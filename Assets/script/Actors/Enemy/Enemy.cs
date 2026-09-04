using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Enemy : move
{

    enum Movestate
    {
        stop,
        pathmove,
        attackReady, 
        attack,
        none,
    }
    [SerializeField] Movestate nowmove;
    enum EnemyTypelist //어택 위치선정을 위한 이넘
    {
        meleeE,
        RangedE
    }
    [SerializeField]EnemyTypelist enemytype;
    public int Enermytypes { get { return (int)enemytype; } }
    [SerializeField] protected int attackDamage;
    [SerializeField] float Searchrange = 3;
    [SerializeField] float attackrange;
    protected Transform targetEnemy;
    public Transform Target { get { return targetEnemy; } set{ targetEnemy = value; } }
    Vector2Int targetPos;
    int count;
   
    Vector3 dir;
    bool startTileOn = false;
    bool endTileOn = false;
    bool ishit = false;
    bool fightend = false;
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
        SearchEnemy();
        FindingEnemy();
        NowStateMode();

        if (endTileOn == false)//마지막 라인에 도착하기 전까지
        {
            if ((transform.position.x > AsrarAlgo.instance.TargetPos.x - 1 && transform.position.x < AsrarAlgo.instance.TargetPos.x + 1) &&
                               (-transform.position.y > AsrarAlgo.instance.TargetPos.y - 1 && -transform.position.y < AsrarAlgo.instance.TargetPos.y + 1))
            {//마지막라인 도착
                endTileOn = true;
            }
            if (startTileOn == true && targetEnemy == null)
            {//스타트 라인이 아니고 적도 없을 때 

                Moving(AsrarAlgo.instance.TargetPos);
            }
            else if (startTileOn == true && targetEnemy != null)
            {// 적을 발견했고 그 자신이 스타트 라인이 아닐때 

                targetPos = new Vector2Int(Mathf.RoundToInt(targetEnemy.position.x),Mathf.RoundToInt(-targetEnemy.position.y));
                Moving(targetPos);
            }
            else if (startTileOn == false)
            {// 자신이 스타트라인일 때 스타트라인까지 이동 
                Moving();
                if (transform.position.y < 0 || (transform.position.x > 9 || transform.position.x< 6 ))
                {
                    startTileOn = true;
                }
            }
           
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
        dir = GameManager.instance.NowMonstertrs[i].position - transform.position;
        RaycastHit2D Searchrays = Physics2D.Raycast(transform.position, dir.normalized, Searchrange, LayerMask.GetMask("Wall", "Monster"));
        if (Searchrays && Searchrays.transform.CompareTag("Monster"))//몬스터에게 레이를 쏘고
        {

            if (targetEnemy != null)//지정된 타깃과 다른 타깃의 거리를 계산하고 가장 가까운 타깃을 지정
            {
                if (Vector2.Distance(transform.position, targetEnemy.position) > Vector2.Distance(transform.position, Searchrays.transform.position))
                {//기존 타겟이 있으면 더 가까운 타겟을 넣는다

                   targetEnemy = Searchrays.transform;
                }
            }
            else
            {
                    targetEnemy = Searchrays.transform;
            }
           
        }

            Transform meetTarget = GameManager.instance.MeetingTarget();
            if (targetEnemy == null && meetTarget != null)
            {
                targetEnemy = meetTarget;
                nowmove = Movestate.attackReady;
            }
        }
    }
    void FindingEnemy()//적공격
    {//이 부분에서 타깃 지정 슬롯 적용 타깃 주변에서
        if (startTileOn == false) return;//시작지점에서는 액션금지


        if (targetEnemy != null)
        {


            switch (enemytype)
            {
                case EnemyTypelist.meleeE://근접 사거리 안에 다가가기만 해도 공격 자리를 잡기위해 움직인다 
                    meleeEAttackReady();

                    if (Vector3.Distance(transform.position, targetEnemy.position) < attackrange)
                    {
                        attackGo();
                        fightend = true;
                    }

                    break;
                case EnemyTypelist.RangedE://원거리 목표사이에 벽이 없으면 공격 자리를 잡기위해 움직이지 않는다 

                    Vector3 targetdir = targetEnemy.position - transform.position;
                    RaycastHit2D Searchrays = Physics2D.Raycast(transform.position, targetdir.normalized, attackrange, LayerMask.GetMask("Wall", "Monster"));
                    if (Searchrays && Searchrays.transform.CompareTag("Monster"))
                    {
                        attackGo();
                        fightend = true;
                    }

                    break;
            }
        }

        if(targetEnemy == null && fightend == true)//전투가 끝날 때
        {
            fightend = false;
            int count = GameManager.instance.Nowenemytrs.Count;
            for (int i = 0; i < count; i++)
            {
                if (GameManager.instance.Nowenemytrs[i] == this)//이 유닛의 소환 순서 탐색후 그에 따라 출발 
                {
                    Invoke(nameof(delaystartgo), i * 0.5f);
                }
            }
        }

    }
    void delaystartgo()
    {
        nowmove = Movestate.pathmove;
    }

    /// <summary>
    /// 근접 어택레디 타겟에 다가간다 
    /// </summary>
    void meleeEAttackReady()
    {
        Vector2 dir = targetEnemy.transform.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        angle = (angle + 360f) % 360f;

        int slot = Mathf.RoundToInt(angle / 45f) % 8; // 0~45: 0 , 45~90: 1, 90~180:2 ...
        int a = 0;
        
        for (int i = 0; i < 3; i++)// 앞의 3방향만 탐새
        {

            Vector2 attackpointer = targetEnemy.position;

            a = a + (i * (i % 2 == 1 ? -1 : 1));//다음 탐색할 슬.
            int nowslot = (a + slot + 8) % 8;

            if (nowslot < 2 || nowslot > 6)
            {
                attackpointer.x += 1 * attackrange;
            }
            else if (nowslot > 2 && nowslot < 6)
            {
                attackpointer.x += -1 * attackrange;
            }

            if (nowslot > 4)
            {
                attackpointer.y += -1 * attackrange;
            }
            else if (nowslot > 0 && nowslot < 4)
            {
                attackpointer.y += 1 * attackrange;
            }
            Vector3 attackslot = attackpointer - new Vector2(transform.position.x, transform.position.y);
            float pointdis = Vector2.Distance(attackpointer, transform.position);
            RaycastHit2D ray = Physics2D.Raycast(transform.position, attackslot.normalized, pointdis, LayerMask.GetMask("Wall", "Monster"));

            if (!ray.transform.CompareTag("wall"))
            {

                anim.SetBool("Run", true);

                transform.position += attackslot.normalized * speed * Time.deltaTime;
                if (attackslot.normalized.x > 0)
                {
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                }

            }
           
        }
        if (Vector2.Distance(transform.position, targetEnemy.position) <= attackrange + 0.2f)
        {
            attackGo();
        }
    }


   void NowStateMode()
    {
        if (startTileOn == false) return;

        switch (nowmove)
        {
            case Movestate.stop:
                ismoveway = false;
                break;

            case Movestate.pathmove:// 미궁 길찾기 정보에 따라 이동
                ismoveway = true;
                Moving(AsrarAlgo.instance.TargetPos);
                attackStop();

                break;

            case Movestate.attackReady://레이를 쏴서 몬스터의 슬롯을 정하고 자리이동 이동 불가시 정지 ,해야할거 게임 매니저에서 전투시 정지 관리
                ismoveway = false;
                switch (enemytype)
                {
                    case EnemyTypelist.meleeE://근접 사거리 안에 다가가기만 해도 공격 자리를 잡기위해 움직인다 
                        meleeEAttackReady();

                    break;
                    case EnemyTypelist.RangedE://원거리 목표사이에 벽이 없으면 공격 자리를 잡기위해 움직이지 않는다

                        Vector3 targetdir = targetEnemy.position - transform.position;
                        RaycastHit2D Searchrays = Physics2D.Raycast(transform.position, targetdir.normalized, attackrange, LayerMask.GetMask("Wall", "Monster"));
                        if (Searchrays && Searchrays.transform.CompareTag("Monster"))
                        {
                            nowmove = Movestate.attack;
                        }

                    break;
                }

                if(targetEnemy == null)
                {
                    nowmove = Movestate.pathmove;
                }
                
                break;
            case Movestate.attack://지금 순서가 엉망진창 ->타겟 변경 되면 ? 이걸 스테이트로 조절하는게 아니라 공격따로 이동 따로 조절 전체 해당 함수 삭제하고 수정요/
                ismoveway = false;
                attackGo();

                if (targetEnemy == null)//타겟이 사라졌으면 이동 
                {
                    nowmove = Movestate.stop;

                    int count = GameManager.instance.Nowenemytrs.Count;
                    for (int i = 0; i < count; i++)
                    {
                        if (GameManager.instance.Nowenemytrs[i] == this)//이 유닛의 소환 순서 탐색후 그에 따라 출발 
                        {
                            Invoke(nameof(delaystartgo), i * 0.5f);
                        }
                    }
                }

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

    private void OnTriggerEnter2D(Collider2D collision)//이거 일단 지우고 다른 데지미 받는 형식으로 간/
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


    public void GetDamage(float Damage)
    {
        Hp -= Damage;
        ishit = true;
        HitMotion();
        Death();
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

