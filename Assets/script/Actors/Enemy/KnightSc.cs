using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightSc : Enemy
{
    [SerializeField]Animation ani;
    [SerializeField]GameObject AttackBox;
    
    protected override void Update()
    {
        base.Update();
    }
    protected override void attackGo()
    {
        ani.Play();
    }
    protected override void attackStop()
    {
        ani.Stop();
        ani.Rewind();
        if (AttackBox.activeSelf == true)
        {
            AttackBox.SetActive(false);
        }
    }
    void knightAttack()
    {
        if (targetEnemy != null)
        {
            targetEnemy.GetComponent<Monster>().GetDamage(attackDamage);//겟데미지 몬스터 용사 따로 만들
        }
       
    }

}
