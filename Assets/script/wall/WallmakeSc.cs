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
    bool wallmakedeletswitch = true;
    [SerializeField] Button wallButton;
    [SerializeField] Button wallModeButton;
    [SerializeField] TMP_Text wallModeText;
    Animation anim;
    AnimationState anistate; // ????
    Color backgroundbasicColor;
    [SerializeField] Color backgroundchangeColor;
    NoWayCheck waycheck;
    CamerMovingSc cammoves;
    [SerializeField] TMP_Text wallcCounString;
    public bool wallModeOn { get { return wallmakemode; } }
    [SerializeField, Header("?????????? ?? ??")] int wallposiblecount;
    Monster selectMon;
    SpriteRenderer selectSpr;
    [SerializeField] SpriteRenderer selectMark;
    Color selectColor = new Color(1,1,1,0.2f);
    bool isCatch = false;
    [SerializeField] TMP_Text checkWallMessage;
    public byte wallsuccess = 0;
    void Start()
    {

        wallButton.onClick.AddListener(wallmakemodeOnOff);
        wallModeButton.onClick.AddListener(Changewallmode);
        anim = wallModeButton.GetComponent<Animation>();
        anistate = anim["wallbuttontoggle"];
        backgroundbasicColor = Camera.main.backgroundColor;
        waycheck = GetComponent<NoWayCheck>();
        cammoves = GetComponent<CamerMovingSc>();
    }
    void Update()
    {
        wallmode();
        OnWallcountString();
        MonMoveMode();
    }
    void wallmakemodeOnOff()
    {
        if (walltile.transform.Find("MissingWall(Clone)") != null)
        {
            checkWallMessage.text = "미완성된 벽이 있습니다.";
            return;
        }
        if(waycheck.check() == false)
        {
            checkWallMessage.text = "모든길이 막혀 있습니다.";
            return;
        }
        wallcCounString.gameObject.SetActive(!wallcCounString.gameObject.activeSelf);
        wallmakemode =!wallmakemode ;
        NowWall(wallmakemode);

       if(wallsuccess<2)wallsuccess++;
    }
    void WallMakeModeOn()
    {
        cammoves.IsCamerMove();
        wallmakedeletswitch = true;
        wallModeText.text = "Make";
        anim.Play("wallbuttontoggle");
        anistate.speed = 1;
        Camera.main.backgroundColor = backgroundchangeColor;
    }

    void WallmakeModeOff()
    {
        cammoves.IsCamerMove();
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
            if (ray && ray.transform.CompareTag("Ground") && wallposiblecount >0)
            {
                Vector3Int mousPostile = walltile.WorldToCell(mosPos);
                walltile.SetTile(mousPostile, tilebase);
                wallposiblecount--;
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
                wallposiblecount++;
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
    public void GiveWallcountUp(int _givewall)
    {
        wallposiblecount += _givewall;
    }
    void OnWallcountString()
    {
        wallcCounString.text = $"Wall : {wallposiblecount}";
    }
    private void MonMoveMode()
    {
        if (wallmakemode == true || GameManager.instance.IsGamStart == true) return;

        if(Input.GetMouseButtonDown(0))//?? ?? ??? 
        {
            Vector2 mosPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D ray = Physics2D.Raycast(mosPos, Vector3.forward, 20, LayerMask.GetMask("Monster"));
            if (ray && ray.transform.CompareTag("Monster"))
            {
                GameManager.instance.ItemCatching = true;
                selectMon = ray.transform.GetComponent<Monster>();
                selectSpr = selectMon.MySprR;
                selectSpr.color = selectColor;
                selectMark.sprite = selectSpr.sprite;
                selectMark.gameObject.SetActive(true);
                isCatch = true;
            }
        }
        else if(Input.GetMouseButton(0))//???? ?? 
        {
            if(isCatch)
            selectMark.transform.position =new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, 0);
        }
        else if(Input.GetMouseButtonUp(0))//??? ?? ?
        {
            if (isCatch == false) return;
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int movePos = new Vector2Int(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.y));
            RaycastHit2D ray = Physics2D.Raycast(movePos, Vector3.forward, float.PositiveInfinity, LayerMask.GetMask("Ground", "Wall", "Monster"));
            if (ray && ray.transform.CompareTag("Ground"))
            {
                selectMon.transform.position = mousePos;
            }
            selectSpr.color =Color.white;
            selectSpr = null;
            selectMark.sprite =null;
            selectMon = null;
            selectMark.gameObject.SetActive(false);
            GameManager.instance.ItemCatching = false;
            isCatch = false;
        }

    }

}
