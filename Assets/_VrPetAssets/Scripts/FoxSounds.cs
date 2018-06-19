using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoxSounds : MonoBehaviour
{

    public GameObject foxHead;
    public const float foxVolume = .4f;
    public AudioClip[] foxClips;

    private AudioSource foxAudio;
    private string[] setText;

    void Start()
    {
        foxAudio = foxHead.GetComponent<AudioSource>();
    }

    public void VoiceFox(string startEndClipRange)
    {
        setText = startEndClipRange.Split();
        foxAudio.PlayOneShot(foxClips[Random.Range(int.Parse(setText[0]), int.Parse(setText[1]))], foxVolume);
    }
}