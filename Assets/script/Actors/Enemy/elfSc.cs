using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class elfSc : Enemy
{
    [SerializeField] SpriteRenderer bow;
    bool bowPlaying = false;
    bool resetattacktime=false;
    float timer=0;
    [SerializeField, Header("공격속도")] float attackspeed;
    [SerializeField,Header("차지모션공격속도보다 낮게설정해주세요")]float chargeMotiontime;
    [SerializeField] GameObject arrow;
    [SerializeField] Sprite bowChargeSprite;
    [SerializeField] Sprite bowDefaltSprite;
    float angle;
    private void Update()
    {
        base.Update();//문제?질문
        Charge();
    }
    protected override void attackGo()
    {
        bowPlaying = true;
        resetattacktime = true;
    }
    protected override void attackStop()
    {
        bowPlaying = false;
    }
    void Charge()
    {
       if (bowPlaying==true)
        {
            timer += Time.deltaTime;
            if(timer> attackspeed)
            {
                timer = 0;
                Shoot();
            }
            else if(timer> chargeMotiontime && timer< attackspeed)
            {
                bow.sprite = bowChargeSprite;
            }
        }

       if (resetattacktime == true&& bowPlaying ==false )
        {
            resetattacktime = false;
            timer = 0;
            bow.sprite = bowDefaltSprite;
        }
    }
    void Shoot()
    {
        bow.sprite = bowDefaltSprite;
        angle = Quaternion.FromToRotation(Vector2.up,targetEnemy.position-transform.position).eulerAngles.z;
        Instantiate(arrow, bow.transform.position, Quaternion.Euler(new Vector3(0, 0, angle)) ,transform);
    }
}
