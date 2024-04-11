using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamerMovingSc : MonoBehaviour
{
    Camera cam;
    public bool CamermoveMode;
    Vector3 beforemosPos;
    Vector2 beforeCamPos;
    Vector2 nowPos;
    Vector2 nowCam;
    Vector2 CamMinMax;

    [SerializeField]float Camspeed = 1.0f;
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
    private void CamMove()//카메라무브 기능
    {
        if (CamermoveMode)
        {
            if (Input.GetMouseButtonDown(0))
            {
                beforemosPos = Input.mousePosition;
                beforeCamPos = cam.transform.position;
               // CamMinMax = new Vector2(cam,);
            }
            if (Input.GetMouseButton(0))
            {
                nowPos = beforemosPos - Input.mousePosition; 
                nowCam = beforeCamPos + (nowPos* Camspeed);
                Vector3 curcamPos = cam.transform.position;
                curcamPos.x = nowCam.x;
                curcamPos.y = nowCam.y;
                cam.transform.position = curcamPos; 
            }                                        
            if(Input.GetAxis("Mouse ScrollWheel") != 0) 
            {
               cam.orthographicSize -= Input.GetAxis("Mouse ScrollWheel")* wheelspeed;
            }
        }
    }
    private (int, int) ratioCounter(int x, int y)//화면비
    {
        int r = 0;
        int a = x;
        int b = y;
        while(r == 0)//최대 공약수
        {
            r = a % b;
            a = b; 
            b = r;   
        }
        int Xratio =0;
        int Yratio =0;
        Xratio = a/r;
        Yratio = b/r;
        return (a,b);
    }



}
