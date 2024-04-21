using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightSc : Enemy
{
    [SerializeField]Animation ani;
    protected override void attackGo()
    {
        ani.Play();
    }
    protected override void attackStop()
    {
        ani.Stop();
    }
}
