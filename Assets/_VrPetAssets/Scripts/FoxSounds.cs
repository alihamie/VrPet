using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoxSounds : MonoBehaviour
{

    public GameObject foxHead;
    public float foxVolume = .4f;
    public Transform foxSoundMasterList;

    FoxSoundList[] soundListReferences;
    AudioSource foxAudio;
    AudioClip[] _foxSound;
    int numberOfChildren;

    void Start()
    {
        //So, we get the Fox's AudioSource to the play the audio, we get the children of the Fox's SFX Master List and add their FoxSoundList component to "THE LIST"(TM).
        foxAudio = foxHead.GetComponent<AudioSource>();
        int numberOfChildren = foxSoundMasterList.childCount;
        soundListReferences = new FoxSoundList[numberOfChildren];
        for (int i = 0; i < numberOfChildren; i++)
        {
            soundListReferences[i] = foxSoundMasterList.GetChild(i).GetComponent<FoxSoundList>();
        }
    }

    //Then it's as simple as choosing an entry on the list to play with an animation even attached to an animation. Or you can just call it whenever you think is appropriate. I use playoneshot here, it won't interrupt itself.
    public void VoiceFox(int listNumber)
    {
        _foxSound = soundListReferences[listNumber].FoxSound;
        foxAudio.PlayOneShot(_foxSound[Random.Range(0, _foxSound.Length)]);
    }
}