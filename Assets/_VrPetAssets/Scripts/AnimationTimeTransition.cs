using UnityEngine;
using System.Collections;
using MalbersAnimations;

public class AnimationTimeTransition : MonoBehaviour, IAnimatorListener
{
    public FoxItemInteraction itemInteraction;

    private RelativeHeadRotate headRotate;
    private FoxSounds foxSounds;

    void Start()
    {
        headRotate = GetComponent<RelativeHeadRotate>();
        foxSounds = GetComponent<FoxSounds>();
    }

    public void OnAnimatorBehaviourMessage(string message, object value)
    {
        if (string.Equals(message, "End") ||
            string.Equals(message, "Sub-Action-T1") ||
            string.Equals(message, "Sub-Action-B1 "))
        {
            if (value is float)
            {
                StartCoroutine(WaitTime(message, (float)value));
            }
        }
        else if (string.Equals(message, "IsWandering"))
        {
            GetComponent<AnimalAIControl>().isWandering = true;
        }
        else if (string.Equals(message, "GrabItem"))
        {
            itemInteraction.GrabItem();
        }
        else if (string.Equals(message, "DropItem"))
        {
            itemInteraction.DropItem();
        }
        else if (message == "ChangeLookTarget")
        {
            headRotate.currentTarget = ((RelativeHeadRotate.TARGETS)(int)value);
        }
        else if (message == "VoiceFox")
        {
            foxSounds.VoiceFox((int)value);
        }
    }

    IEnumerator WaitTime(string message, float s)
    {
        yield return new WaitForSeconds(s);
        GetComponent<Animator>().SetBool(message, true);
    }
}
