using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniOrcSc : Monster
{
    [SerializeField] Vector2 attackRange = new Vector2(2,2);
    public void MinorcAttack()
    {
        if(targetEnemy != null)
        {
            Collider2D[] hittarget = Physics2D.OverlapBoxAll(targetEnemy.position, attackRange, 0f,LayerMask.GetMask("Enemy"));
            foreach(Collider2D hits in hittarget)
            {
                Enemy mon = hits.GetComponent<Enemy>();
                if(mon != null)
                {
                    mon.GetDamage(attackDamage);
                }
            }
        }
    }

}
