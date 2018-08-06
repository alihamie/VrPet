using UnityEngine;

public class FoxSounds : MonoBehaviour
{
    public float foxVolume = .4f;
    public Transform foxSoundMasterList;

    FoxSoundList[] soundListReferences;
    AudioSource foxAudio;
    AudioClip[] foxClips;
    int numberOfChildren;

    void Start()
    {
        foxAudio = GetComponent<MalbersAnimations.AnimalAIControl>().animalHead.GetComponent<AudioSource>();
    }

    //Then it's as simple as choosing an entry on the list to play with an animation even attached to an animation. Or you can just call it whenever you think is appropriate. I use playoneshot here, it won't interrupt itself.
    public void VoiceFox(int listNumber)
    {
        foxClips = foxSoundMasterList.GetChild(listNumber).GetComponent<FoxSoundList>().FoxSound;
        foxAudio.PlayOneShot(foxClips[Random.Range(0, foxClips.Length)], foxVolume);
    }
}