using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Dragable : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
{
    Transform canvas;
    Transform beforeParent;
    RectTransform rect;
    CanvasGroup canvasGroup;

    private void Awake()
    {
        canvas = transform.root.transform.Find("inventory");
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponentInParent<CanvasGroup>();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        beforeParent = transform.parent;
        transform.SetParent(canvas);
        transform.SetAsLastSibling();//맨밑으로 옮겨 가장 위에 그려지게 
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.position =eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (transform.parent == canvas )
        {
            transform.SetParent(beforeParent);
            rect.position = beforeParent.GetComponent<RectTransform>().position;
        }
        canvasGroup.blocksRaycasts = true;
    }

 
}
