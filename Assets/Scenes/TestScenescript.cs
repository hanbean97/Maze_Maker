using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScenescript : MonoBehaviour
{
    [SerializeField] GameObject target;
    Vector3 targetTrans;
    [SerializeField] Sprite sprt;
    // Start is called before the first frame update
    void Start()
    {
        Vector2 dir = target.transform.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        angle = (angle + 360f) % 360f;
        int slot = Mathf.RoundToInt(angle / 45f) % 8;

        int a= 0;
        int b= -1;
        GameObject[] gamsob = new GameObject[8];
        for (int i = 0; i < 8; i++)
        {
            a= a+(i*b);
            b= -b;
            gamsob[i] = new GameObject();
            gamsob[i].AddComponent<SpriteRenderer>().sprite = sprt;
            gamsob[i].transform.position =new Vector3(i,i,0);
            Debug.Log(a);
        }
    }

    // Update is called once per frame
    void Update()
    {
    
     
    }
}
