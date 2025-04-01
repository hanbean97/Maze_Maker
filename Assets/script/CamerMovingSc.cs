using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CamerMovingSc : MonoBehaviour
{
    Camera cam;
    [SerializeField]bool CamermoveMode = true;
    Vector3 beforemosPos;
    Vector2 beforeCamPos;
    Vector2 nowPos;
    Vector2 nowCam;
    float distancewoldcam;
    float ratioWidth;
    [SerializeField] float wheelspeed = 1.0f;
     WallmakeSc wallmode;
    bool inventorych;
    
    void Start()
    {
        cam = Camera.main;
        wallmode= GetComponent<WallmakeSc>();
        inventorych = false;
    }

    // Update is called once per frame
    void Update()
    {
        itemcatch();
        CamMove();
    }
    private void CamMove()//카메라무브 기능
    {
        if (CamermoveMode && inventorych== false)
        {

            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    inventorych = true;
                    return;
                }
                beforemosPos = Input.mousePosition;
                beforeCamPos = cam.transform.position;
                distancewoldcam = Mathf.Abs((cam.ViewportToWorldPoint(new Vector3(0,0,0))- cam.ViewportToWorldPoint(new Vector3(1, 0, 0))).x);//캠기준 좌우 끝점 길이
                ratioWidth = distancewoldcam / cam.pixelWidth;//비율
            }
            else if (Input.GetMouseButton(0))
            {
                nowPos = beforemosPos - Input.mousePosition; 
                nowCam = beforeCamPos + (nowPos* ratioWidth);
                Vector3 curcamPos = cam.transform.position;
                curcamPos.x = nowCam.x;
                curcamPos.y = nowCam.y;
                cam.transform.position = curcamPos; 
            }                                        
        }
        if(Input.GetMouseButtonUp(0))
        {
            inventorych = false;
        }


            if(Input.GetAxis("Mouse ScrollWheel") != 0) 
            {
               cam.orthographicSize -= Input.GetAxis("Mouse ScrollWheel")* wheelspeed;
            }
    }
    void itemcatch()
    {
       if(GameManager.instance.ItemCatching == true || wallmode.wallModeOn == true)
        {
            CamermoveMode = false;
        }
       else
        {
            CamermoveMode = true;
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
    public void IsCamerMove()
    {
        CamermoveMode =!CamermoveMode;
    }
}
