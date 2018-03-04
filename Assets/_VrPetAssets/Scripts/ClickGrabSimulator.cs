using EasyInputVR.StandardControllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickGrabSimulator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{

    public StandardGrabReceiver grabItem;
    public GameObject pointer;
    bool hovering = false;

    private void Update()
    {
        if (hovering)
        {
            grabItem.Hover(grabItem.transform.position, pointer.transform);
        }
        else
        {
            grabItem.HoverExit(grabItem.transform.position, pointer.transform);
        }
    }

    public void ToggleHover()
    {
        hovering = !hovering;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ToggleHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToggleHover();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ToggleHover();
        grabItem.HoverExit(grabItem.transform.position, pointer.transform);
    }
}
