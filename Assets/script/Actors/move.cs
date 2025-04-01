using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move : PathFind
{
    protected float Hp;
    [SerializeField] float maxhp;
    int nextPoscount;
    protected Vector3 nextdir;
    [SerializeField] float speed;
    protected bool ismoveway;
    protected Animator anim;
    protected bool isdeth=false;
    private void Awake()
    {
        Hp = maxhp;
    }
    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        ismoveway= true;
    }
    protected void Moving()//기본 움직이는 함수
    {
        if (ismoveway)
        {
            anim.SetBool("Run", true);
            nextdir = this.FinalNodeList[nextPoscount].nodePosition - transform.position;
            transform.position += nextdir.normalized * speed * Time.deltaTime;
            if (nextdir.normalized.x > 0)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            if (Vector2.Distance(this.transform.position, (Vector2Int)FinalNodeList[nextPoscount].nodePosition) < 0.1f)
            {
                if (nextPoscount < FinalNodeList.Count - 1)
                {
                    nextPoscount++;
                }
            }
        }
        else
        {
            anim.SetBool("Run", false);
        }
    }
    protected void Moving(Vector2Int target)//동적으로 움직이는 함수
    {
        if (ismoveway)
        {
            if (target.x > AsrarAlgo.instance.Size.x-1 || target.y > AsrarAlgo.instance.Size.y -1|| target.x < -1 ||target.y <-1)
            {
                return;
            }
            AroundSetPos(target);

           
            anim.SetBool("Run", true);
            if(this.FinalNodeList.Count != 1)
            nextdir = this.FinalNodeList[1].nodePosition - transform.position;

            transform.position += nextdir.normalized * speed * Time.deltaTime;
            if (nextdir.normalized.x > 0)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }
        else
        {
            anim.SetBool("Run", false);
        }
    }
    protected void AroundSetPos(Vector2Int target)
    {
        nextPoscount = 0;
        Vector2Int nowPosition = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(-transform.position.y));
        this.PathFinding(nowPosition, target);
    }
    public void Heal(float _healvalue)//몬스터 체력회복
    {
        Hp += _healvalue;
        if(Hp>maxhp)
        {
            Hp = maxhp;
        }
    }
    public void Heal()//몬스터 체력회복
    {
          Hp = maxhp;
    }
}

