using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigDemon : Monster
{
    protected override void attackGo()
    {
        anim.SetBool("Attack",true);
    }
    protected override void attackStop()
    {
        anim.SetBool("Attack", false);
    }
    
}
