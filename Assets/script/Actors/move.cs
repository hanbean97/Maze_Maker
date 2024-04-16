using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move : PathFind
{ 
    int nextPoscount;
    Vector3 nowPosaround;
    [SerializeField] float speed;
    [SerializeField]bool ismoveway;
    bool findenemy;
    [SerializeField] Animator anim;
    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }
    protected void Moving()
    {
        if (ismoveway)
        {
            anim.SetBool("Run",true);
            nowPosaround = this.FinalNodeList[nextPoscount].nodePosition - transform.position;
            transform.position += nowPosaround.normalized * speed * Time.deltaTime;
            if(nowPosaround.normalized.x >0)
            {
                transform.rotation = Quaternion.Euler(0,0,0) ;
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }


            if (Vector2.Distance(this.transform.position, (Vector2Int)FinalNodeList[nextPoscount].nodePosition) < 0.1f)
            {
                if(nextPoscount < FinalNodeList.Count-1)
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
    protected void AroundSetPos()
    {
        nextPoscount = 0;
        Vector2Int nowPosition = new Vector2Int(Mathf.RoundToInt(transform.position.x),-Mathf.RoundToInt(transform.position.y));
        this.PathFinding(nowPosition, AsrarAlgo.instance.targetPos);
    }
    protected void SearchEnemy(int range,string enemyLayername)
    {
        Physics2D.OverlapCircle(transform.position,range,LayerMask.GetMask(enemyLayername));
    }
}
