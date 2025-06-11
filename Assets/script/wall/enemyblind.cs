using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyblind : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("AttackBox")) return;

        SpriteRenderer sp = collision.GetComponentInChildren<SpriteRenderer>();
        if( collision.CompareTag("Enemy")|| collision.CompareTag("Monster") )
        {
        sp.sortingOrder = 1;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("AttackBox")) return;

        SpriteRenderer sp = collision.GetComponentInChildren<SpriteRenderer>();
        sp.sortingOrder = 4;
       
    }
}
