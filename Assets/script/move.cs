using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class move : MonoBehaviour
{
    float moveing;
    Vector3 nomalmove;
    [SerializeField] float movespeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        nomalmove = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"),0);
        transform.position += nomalmove*movespeed*Time.deltaTime;

    }
}
