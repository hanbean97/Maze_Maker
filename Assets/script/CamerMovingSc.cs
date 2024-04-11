using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamerMovingSc : MonoBehaviour
{
    Camera cam;
    public bool CamermoveMode;
    Vector2 beforemosPos;
    Vector2 beforeCamPos;
    Vector2 nowPos;
    Vector2 nowCam;
    [SerializeField]float speed = 1.0f;
    [SerializeField] float wheelspeed = 1.0f;
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        CamMove();
    }
    void CamMove()//카메라무브 기능
    {
        if (CamermoveMode)
        {
            if (Input.GetMouseButtonDown(0))
            {
                beforemosPos = cam.ScreenToWorldPoint(Input.mousePosition);
                beforeCamPos = cam.transform.position;
            }
            if (Input.GetMouseButton(0))
            {
                nowPos = beforemosPos - (Vector2)cam.ScreenToWorldPoint(Input.mousePosition); ;
                nowCam = beforeCamPos + nowPos;
                cam.transform.position = new Vector3(nowCam.x, nowCam.y, cam.transform.position.z);
            }
            if(Input.GetAxis("Mouse ScrollWheel") != 0) 
            {
               cam.orthographicSize -= Input.GetAxis("Mouse ScrollWheel")* wheelspeed;
            }
        }
    }
}
