using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class WallmakeSc : MonoBehaviour
{
    [SerializeField] bool wallmakemode;
    [SerializeField] Tilemap walltile;
    [SerializeField] TileBase tilebase;
    [SerializeField] bool wallmakedeletswitch = true;
    [SerializeField] Button wallButton;
    [SerializeField] Button wallModeButton;
    [SerializeField] TMP_Text wallModeText;
    Animation anim;
    AnimationState anistate; // 중요
    Color backgroundbasicColor;
    void Start()
    {
        wallButton.onClick.AddListener(wallmakemodeOn);
        wallModeButton.onClick.AddListener(Changewallmode);
        anim = wallModeButton.GetComponent<Animation>();
        anistate = anim["wallbuttontoggle"];
        backgroundbasicColor = Camera.main.backgroundColor;
    }
    void Update()
    {
        wallmode();
    }
    void wallmakemodeOn()
    {
        if (walltile.transform.Find("MissingWall") != null)
        {
            Debug.Log("미완성벽 있음");
            return;
        }

        wallmakemode=!wallmakemode ;
        if (wallmakemode== true)
        {
            wallmakedeletswitch = true;
            wallModeText.text = "Make";
            anim.Play("wallbuttontoggle");
            anistate.speed = 1;
            Camera.main.backgroundColor = Color.blue;
        }
        else
        {
            //anim.Rewind();
            anim.Play("wallbuttontoggle");
            anistate.speed = -1;
            anistate.time = anistate.length;
            Camera.main.backgroundColor = backgroundbasicColor;
        }
    }
    void Changewallmode()
    {
        wallmakedeletswitch = !wallmakedeletswitch;
        switch (wallmakedeletswitch)
        {
            case true:
                wallModeText.text = "Make";
                break;
                case false:
                wallModeText.text = "erase";
                break;
        }

    }

    void wallmode()
    {
        if (wallmakemode == false) return;

        switch (wallmakedeletswitch)
        {
            case true:
                MakeWall();
                break;
            case false:
                DeletWall();
                break;
        }
    }
    void MakeWall()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 mosPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D ray = Physics2D.Raycast(mosPos, Vector3.forward, 20);
            if (ray && ray.transform.CompareTag("Ground"))
            {
                Vector3Int mousPostile = walltile.WorldToCell(mosPos);
                walltile.SetTile(mousPostile, tilebase);
            }
        }
    }
    void DeletWall()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 mosPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D ray = Physics2D.Raycast(mosPos, Vector3.forward, 20, LayerMask.GetMask("Wall"));
            if (ray && ray.transform.CompareTag("Wall"))
            {
                Vector3Int mousPostile = walltile.WorldToCell(mosPos);
                walltile.SetTile(mousPostile, null);
            }
        }
    }
    

}
