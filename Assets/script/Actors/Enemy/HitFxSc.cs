using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitFxSc : MonoBehaviour
{
    // Start is called before the first frame update
   public void SelfDestroy()
   {
        Destroy(gameObject);
   }
}
