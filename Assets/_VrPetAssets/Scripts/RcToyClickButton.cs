using UnityEngine.EventSystems;

public class RcToyClickButton: BaseToyClickButton {

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        tabletFunctionality.ToggleCarbutton();
    }
}
