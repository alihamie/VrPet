using MalbersAnimations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoxTargetManager : MonoBehaviour {

    public  AnimalAIControl animalAi;
    public  ActionZone playArea;
    public  ActionZone dropArea;

    public void GoToPlayArea()
    {
        animalAi.SetTarget(playArea.transform);
    }

    public void GoToDropArea()
    {
        animalAi.SetTarget(dropArea.transform);
    }

    public void GoToFetchItem(Transform item)
    {
        animalAi.GetComponentInParent<Animator>().SetBool("ThrewItem", true);
        animalAi.SetTarget(item);
    }

    public void WanderAgain()
    {
        animalAi.isWandering = true;
    }
}
