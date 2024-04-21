using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move : PathFind
{
    [SerializeField]protected float Hp;
    int nextPoscount;
    Vector3 nowPosaround;
    [SerializeField] float speed;
    protected bool ismoveway;
    protected Animator anim;
    protected bool isdeth=false;
    
    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        ismoveway= true;
    }
    protected void Moving()//동적으로 타겟의 현재 위치를 받고 1노드씩 새로운 루트를 찾아 다가가는 방식
    {
        if (ismoveway)
        {
            anim.SetBool("Run", true);
            nowPosaround = this.FinalNodeList[nextPoscount].nodePosition - transform.position;
            transform.position += nowPosaround.normalized * speed * Time.deltaTime;
            if (nowPosaround.normalized.x > 0)
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
    protected void Moving(Vector2Int target)
    {
        if (ismoveway)
        {
            if (target.x > AsrarAlgo.instance.Size.x-1 || target.y > AsrarAlgo.instance.Size.y -1|| target.x < 0 ||target.y <0)
            {
                return;
            }


            if ((nextPoscount == 0 ||Vector2.Distance(this.transform.position, (Vector2Int)FinalNodeList[nextPoscount].nodePosition) < 0.1f))
            {
                AroundSetPos(target);
                nextPoscount++;
            }
            anim.SetBool("Run", true);
            nowPosaround = this.FinalNodeList[nextPoscount].nodePosition - transform.position;
            transform.position += nowPosaround.normalized * speed * Time.deltaTime;
            if (nowPosaround.normalized.x > 0)
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
        Vector2Int nowPosition = new Vector2Int(Mathf.RoundToInt(transform.position.x), -Mathf.RoundToInt(transform.position.y));
        this.PathFinding(nowPosition, target);
    }
   
}

