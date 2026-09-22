using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonSc : Monster
{
  public void skeletonAttack()
    {
        if (targetEnemy != null)
        {
            targetEnemy.GetComponent<Enemy>().GetDamage(attackDamage);
        }
    }
}
