using UnityEngine;

public class BackgroundMusicHandler : MonoBehaviour {
    private AudioSource musicPlayer;
    //public MediaPlayerCtrl television;
    private float greatestOfComparedValues;

	void Start () {
        musicPlayer = GetComponent<AudioSource>();
    }

    //private void Update()
    //{
    //    // currently only one thing is being checked. However, it's structured with way so I can easily add other lines to check the volume of other objects.
    //    greatestOfComparedValues = television.isPlaying ? television.m_fVolume : 0f;

    //    // And here's where we inhibit the volume by the loudest thing we want to hear rather than the background music.
    //    musicPlayer.volume = Mathf.Lerp(musicPlayer.volume, (1 - greatestOfComparedValues) * .14f, .01f);
    //}
}
