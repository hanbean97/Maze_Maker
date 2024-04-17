using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyblind : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        SpriteRenderer sp = collision.GetComponentInChildren<SpriteRenderer>();
        sp.sortingOrder = 1;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        SpriteRenderer sp = collision.GetComponentInChildren<SpriteRenderer>();
        sp.sortingOrder = 4;
    }
}
