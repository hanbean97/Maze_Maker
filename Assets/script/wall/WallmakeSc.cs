using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class WallmakeSc : MonoBehaviour
{
    [SerializeField] bool wallmakemode;
    [SerializeField] Tilemap walltile;
    [SerializeField] TileBase tilebase;
    bool wallmakedeletswitch = true;
    [SerializeField] Button wallButton;
    [SerializeField] Button wallModeButton;
    [SerializeField] TMP_Text wallModeText;
    Animation anim;
    AnimationState anistate; // 중요
    Color backgroundbasicColor;
    [SerializeField] Color backgroundchangeColor;
    void Start()
    {
        wallButton.onClick.AddListener(wallmakemodeOnOff);
        wallModeButton.onClick.AddListener(Changewallmode);
        anim = wallModeButton.GetComponent<Animation>();
        anistate = anim["wallbuttontoggle"];
        backgroundbasicColor = Camera.main.backgroundColor;
    }
    void Update()
    {
        wallmode();
    }
    void wallmakemodeOnOff()
    {
        if (walltile.transform.Find("MissingWall(Clone)") != null)
        {
            Debug.Log("미완성벽 있음");
            return;
        }

        wallmakemode=!wallmakemode ;
        NowWall(wallmakemode);
    }
    void WallMakeModeOn()
    {
        wallmakedeletswitch = true;
        wallModeText.text = "Make";
        anim.Play("wallbuttontoggle");
        anistate.speed = 1;
        Camera.main.backgroundColor = backgroundchangeColor;
    }

    void WallmakeModeOff()
    {
        anim.Play("wallbuttontoggle");
        anistate.speed = -1;
        anistate.time = anistate.length;
        Camera.main.backgroundColor = backgroundbasicColor;
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
    public void NowWall(bool _answer)
    {
        wallmakemode = _answer;
        if (wallmakemode == false)
        {
            WallmakeModeOff();
        }
        else
        {
            WallMakeModeOn();
        }
    }

}
