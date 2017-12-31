using MalbersAnimations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoTime : MonoBehaviour {


    public float totalTime;
    private bool isHalfTime;
    public Transform windowEvent;
    public Transform sleepEvent;
    public AnimalAIControl aiControl;
    bool once = true;
	// Use this for initialization
	void Start () {
        totalTime = totalTime + Time.time;
	}
	
	// Update is called once per frame
	void Update () {

        if (!isHalfTime && Time.time >= totalTime / 2)
        {
            isHalfTime = true;
        }

        if (isHalfTime && once)
        {
            aiControl.GetComponentInParent<Animator>().SetBool("ThrewItem", true);
            aiControl.isWandering = false;
            aiControl.SetTarget(windowEvent);
            once = false;
        }


        if (Time.time >= totalTime)
        {
            aiControl.GetComponentInParent<Animator>().SetBool("ThrewItem", true);
            aiControl.isWandering = false;
            aiControl.SetTarget(sleepEvent);
            this.enabled = false;
        }

	}
}
