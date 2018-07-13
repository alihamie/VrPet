using UnityEngine;
using UnityEngine.UI;

public class VolumeButtonSwitch : MonoBehaviour {
    public Sprite volumeOff, volumeOn;
    public MediaPlayerCtrl livePlayer;
    private Image buttonImage;

    private void Start()
    {
        buttonImage = GetComponent<Image>();
    }

    public void Click()
    {
        if (buttonImage.sprite == volumeOff)
        {
            buttonImage.sprite = volumeOn;
            livePlayer.SetVolume(1f);
        }
        else if (buttonImage.sprite == volumeOn)
        {
            buttonImage.sprite = volumeOff;
            livePlayer.SetVolume(0);
        }
    }
}
