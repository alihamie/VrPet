using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoxSounds : MonoBehaviour
{

    public GameObject foxHead;
    public float foxVolume = .4f;
    public FoxSoundList[] soundListReferences;

    private AudioSource foxAudio;
    AudioClip[] _foxSound;

    void Start()
    {
        foxAudio = foxHead.GetComponent<AudioSource>();
    }

    // This function is called directly by the fox's animations so that multiple possible sounds can be chosen from the list of sound effects currently attached to this script on the fox.
    // Splitting the string to get the start and end of the range is how I worked around the fact that I can only pass a single argument when calling a function from an animation.
    public void VoiceFox(int listNumber)
    {
        _foxSound = soundListReferences[listNumber].FoxSound;
        foxAudio.PlayOneShot(_foxSound[Random.Range(0, _foxSound.Length)]);
    }
}