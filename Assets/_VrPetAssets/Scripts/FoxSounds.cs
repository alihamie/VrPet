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

    // This function is called directly by the fox's animations so that multiple possible sounds can be chosen from the list of sound effects currently attached to this script on the fox.
    // Splitting the string to get the start and end of the range is how I worked around the fact that I can only pass a single argument when calling a function from an animation.
    public void VoiceFox(string startEndClipRange)
    {
        setText = startEndClipRange.Split();
        foxAudio.PlayOneShot(foxClips[Random.Range(int.Parse(setText[0]), int.Parse(setText[1]))], foxVolume);
    }
}