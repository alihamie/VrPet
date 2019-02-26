using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseToyClickButton : MonoBehaviour, IPointerDownHandler {

    protected TabletScreenManager screenManager;
    protected TabletVisibility visibility;
    protected TabletFunctionality tabletFunctionality;

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        screenManager.GoBack(transform);
        visibility.ToggleTablet();
        if (tabletFunctionality.car.gameObject.activeSelf && !transform.name.Equals("Exit"))
        {
            tabletFunctionality.ToggleCarbutton();
        }
    }

    public TabletScreenManager GetScreenManager()
    {
        return screenManager;
    }

    void Start () {
        screenManager = gameObject.GetComponentInParent<TabletScreenManager>();
        visibility = transform.root.GetComponent<TabletVisibility>();
        tabletFunctionality = transform.GetComponentInParent<TabletFunctionality>();
    }
}
