using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HobbitSc : Enemy
{
    [SerializeField] GameObject hitfx;
    protected override void attackGo()
    {
        
    }
    protected override void attackStop()
    {

    }

    public void HobbittargetAttack()
    {
        if(targetEnemy != null)
        {
            Instantiate(hitfx, targetEnemy.position, Quaternion.identity);
            targetEnemy.GetComponent<Monster>().GetDamage(attackDamage);
        }
    }    
}
