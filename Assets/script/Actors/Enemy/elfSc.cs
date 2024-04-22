using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class elfSc : Enemy
{
    [SerializeField] SpriteRenderer bow;
    bool bowPlaying = false;
    bool resetattacktime=false;
    float timer=0;
    [SerializeField, Header("���ݼӵ�")] float attackspeed;
    [SerializeField,Header("������ǰ��ݼӵ����� ���Լ������ּ���")]float chargeMotiontime;
    [Header("Ȱ �̹���+ ȭ��")]
    [SerializeField] GameObject arrow;
    [SerializeField] Sprite bowChargeSprite;
    [SerializeField] Sprite bowDefaltSprite;
    [SerializeField] GameObject fakeArrow;
    float angle;
    protected override void Update()
    {
        base.Update();
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
                fakeArrow.SetActive(true);
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
        fakeArrow.SetActive(false);
    }
}
