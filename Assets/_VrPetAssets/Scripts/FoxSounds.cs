using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoxSounds : MonoBehaviour
{

    public GameObject foxHead;
    public float foxVolume = .4f;
    public AudioClip[] foxClips;

    private AudioSource foxAudio;
    private string[] setText;

    // Use this for initialization
    void Start()
    {
        foxAudio = foxHead.GetComponent<AudioSource>();
    }

    public virtual void VoxFox(string startEndClipRange)
    {
        setText = startEndClipRange.Split();
        foxAudio.PlayOneShot(foxClips[Random.Range(int.Parse(setText[0]), int.Parse(setText[1]))], foxVolume);
    }
}