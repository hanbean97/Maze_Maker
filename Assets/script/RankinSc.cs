using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankinSc : MonoBehaviour
{
    [SerializeField] GameObject rank;
    private void Start()
    {
        Instantiate(rank);
    }
}
