using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackMode : MonoBehaviour
{
    [SerializeField] Collider2D Attackcoll;
    
    void AttackOnOff()
    {
        Attackcoll.gameObject.SetActive(!Attackcoll.gameObject.activeSelf);
    }
}
