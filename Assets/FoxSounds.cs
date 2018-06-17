using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoxSounds : MonoBehaviour {

    public GameObject foxHead;
    public float foxVolume = .4f;
    public AudioClip[] foxClips;

    private AudioSource foxAudio;
    private string[] sEText;

	// Use this for initialization
	void Start () {
        foxAudio = foxHead.GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public virtual void VoxFox(string startEnd)
    {
        sEText = startEnd.Split( );
        foxAudio.PlayOneShot(foxClips[Random.Range(int.Parse(sEText[0]), int.Parse(sEText[1]))], foxVolume);
    }
}
