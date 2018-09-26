using UnityEngine;
using UnityEngine.UI;

public class UpdateSliderCount : MonoBehaviour
{
    private Text text;

    public void UpdateCount(Slider slider)
    {
        if (!text)
        { // Normally I would set this under Awake() or Start()... but it's entirely possible that this will be called without ever going to the config menu. So... it's done here instead. Silly but true.
            text = GetComponent<Text>();
        }

        text.text = (Mathf.RoundToInt(slider.value * 100)).ToString();
    }
}
