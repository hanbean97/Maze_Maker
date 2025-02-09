using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowMon : Monster
{
    [SerializeField] GameObject AttakPreFab;
    [SerializeField] Transform ShootPointer;
    float angle;
    [SerializeField]float ShootTime;
    float shoottimer;
    protected override void attackGo()
    {
        shoottimer += Time.deltaTime;
        if (shoottimer > ShootTime)
        {
            angle = Quaternion.FromToRotation(Vector2.up, targetEnemy.position - transform.position).eulerAngles.z;
            Instantiate(AttakPreFab, ShootPointer.position, Quaternion.Euler(new Vector3(0, 0, angle)), transform);
            shoottimer = 0;
        }

    }
    protected override void attackStop()
    {
        shoottimer = 0;
    }
}
