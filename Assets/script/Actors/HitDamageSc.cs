using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitDamageSc : MonoBehaviour
{
    [SerializeField]float Damage;
    public float GetDamage { get { return Damage; } }
    bool hitthis =false;
    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.CompareTag("Monster") &&hitthis == false)
        {
            hitthis = true;
            other.GetComponent<Monster>().GetDamage(Damage);
            Destroy(this);
        }
    }
}
