using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Dropable : MonoBehaviour,IDropHandler
{
    RectTransform rect;
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null && eventData.pointerDrag.transform.name != "Scroll View")
        {
            Dragable items = eventData.pointerDrag.GetComponent<Dragable>();
            if (items.HaveMonster == null) return;
            items.Rec.position = rect.position;
            if(transform.childCount >0)
            {
                transform.GetChild(0).position = items.PointerParent.position;
                transform.GetChild(0).SetParent(items.PointerParent);
            }
           eventData.pointerDrag.transform.SetParent(transform);
        }
    }

   
}
