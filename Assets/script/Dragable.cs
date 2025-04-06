using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Dragable : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
{
    GameObject havemonster;
    public GameObject HaveMonster { get { return havemonster; } set { havemonster = value; } }
    Transform canvas;
    Transform beforeParent;
    public Transform PointerParent { get { return beforeParent; } set { beforeParent = value; } }
    RectTransform rect;
    public RectTransform Rec { get { return rect; } set { rect = value; } }
    Image img;
    private void Awake()
    {
        canvas = transform.root.transform.Find("inventory");
        rect = GetComponent<RectTransform>();
        img = GetComponent<Image>();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        beforeParent = transform.parent;
        transform.SetParent(canvas);
        transform.SetAsLastSibling();//???????? ???? ???? ???? ???????? 
       img.raycastTarget = false;
       GameManager.instance.ItemCatching = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.position =eventData.position;
     
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (transform.parent == canvas)
        {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2Int spownPos = new Vector2Int(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.y));
                RaycastHit2D ray = Physics2D.Raycast(spownPos, Vector3.forward, float.PositiveInfinity, LayerMask.GetMask("Ground", "Wall","Monster"));
                if (ray && ray.transform.CompareTag("Ground"))
                {
                    Monster spown = Instantiate(havemonster, (Vector2)spownPos, Quaternion.identity).GetComponent<Monster>();
                    spown.MyPos = new Vector3Int(spownPos.x, spownPos.y,0);
                    GameManager.instance.InvenOutDungeonMonster(spown.transform);
                    Destroy(gameObject);
                }
           
            transform.SetParent(beforeParent);
            rect.position = beforeParent.GetComponent<RectTransform>().position;
        }
        img.raycastTarget = true;
        GameManager.instance.ItemCatching = false;
    }
   
    public void SetMonster(GameObject _monster)
    {
            havemonster = _monster;
            img.sprite = _monster.GetComponentInChildren<SpriteRenderer>().sprite;
    }
}
