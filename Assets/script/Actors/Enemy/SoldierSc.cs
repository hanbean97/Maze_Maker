using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierSc : Enemy
{

    protected override void Update()
    {
        base.Update();
    }
    protected override void attackGo()
    {
    }
    protected override void attackStop()
    {
    }

    public void SoldierAttack()
    {
        if(targetEnemy != null)
        {
            targetEnemy.GetComponent<Monster>().GetDamage(attackDamage);
        }
    }

}
