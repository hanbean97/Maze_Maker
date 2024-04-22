using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectileSc : MonoBehaviour
{
    [SerializeField] float speed;
    void Update()
    {
      transform.position += transform.rotation*Vector3.up*speed*Time.deltaTime;
        OutLine();
    }

    void OutLine()
    {
        if(transform.position.x<-2 || transform.position.x > AsrarAlgo.instance.Size.x+2|| -transform.position.y < -2|| -transform.position.y > AsrarAlgo.instance.Size.y+2)
        {
            Destroy(gameObject);
        }
    }
  
}
