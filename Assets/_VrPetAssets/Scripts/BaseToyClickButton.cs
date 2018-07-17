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
        screenManager.GoBack(this.transform);
        visibility.ToggleTablet();
        if (this.tabletFunctionality.car.gameObject.activeSelf && !this.transform.name.Equals("Exit"))
        {
            this.tabletFunctionality.ToggleCarbutton();
        }
    }

    public TabletScreenManager GetScreenManager()
    {
        return this.screenManager;
    }

    // Use this for initialization
    void Start () {
        screenManager = this.gameObject.GetComponentInParent<TabletScreenManager>();
        visibility = this.transform.root.GetComponent<TabletVisibility>();
        tabletFunctionality = this.transform.GetComponentInParent<TabletFunctionality>();
    }
}
