using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyblind : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        SpriteRenderer sp = collision.GetComponentInChildren<SpriteRenderer>();
        if( collision.CompareTag("Enemy")|| collision.CompareTag("Monster") || collision.CompareTag("Weapon") )
        {
        sp.sortingOrder = 1;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        SpriteRenderer sp = collision.GetComponentInChildren<SpriteRenderer>();
        sp.sortingOrder = 4;
        if(collision.CompareTag("Weapon"))
        {
            sp.sortingOrder = 5;
        }
    }
}
