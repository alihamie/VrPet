using EasyInputVR.StandardControllers;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickGrabSimulator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public StandardGrabReceiver grabItem;
    public GameObject pointer;
    private bool hovering = false, previousHovering = false;

    private void Update()
    {
        if (hovering && previousHovering)
        {
            grabItem.Hover(grabItem.transform.position, pointer.transform);
        }
        else if (hovering != previousHovering)
        {
            previousHovering = hovering;

            if (hovering)
            {
                grabItem.HoverEnter(grabItem.transform.position, pointer.transform);
            }
            else
            {
                grabItem.HoverExit(grabItem.transform.position, pointer.transform);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }

    // See if Hover needs to be called instead, considering the original order of how this was all coded.
    public void OnPointerDown(PointerEventData eventData)
    {
        hovering = false;
        grabItem.HoverExit(grabItem.transform.position, pointer.transform);
    }
}
