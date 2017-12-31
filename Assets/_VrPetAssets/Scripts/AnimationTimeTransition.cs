using UnityEngine;
using System.Collections;
using MalbersAnimations;

public class AnimationTimeTransition : MonoBehaviour, IAnimatorListener
{

    public FoxItemInteraction itemInteraction;

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
    }


    IEnumerator WaitTime(string message, float s)
    {

        yield return new WaitForSeconds(s);
        GetComponent<Animator>().SetBool(message, true);
    }

}
