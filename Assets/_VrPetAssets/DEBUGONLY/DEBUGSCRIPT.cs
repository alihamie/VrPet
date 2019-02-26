using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations;

public class DEBUGSCRIPT : MonoBehaviour {
    public bool debugBool1, debugBool2, debugBool3;
    public AnimalAIControl fox;
    public ActionZone zone;
    public Animator jack;
    public Transform laserCube;

    private Vector3 jackPos;
    private bool previousDebugBool3;

    private void Start()
    {
        jackPos = zone.transform.position;
    }

    void Update () 
	{
		if (debugBool1)
        {
            fox.SetTarget(zone.transform);
            zone.transform.position = jackPos + new Vector3(2f, 0, 2.5f);
            debugBool1 = false;
        }

        if (debugBool2)
        {
            zone.onEnd.Invoke();
            debugBool2 = false;
        }

        if (debugBool3)
        {
            Vector3 between = fox.transform.position - transform.position;
            laserCube.position = (fox.transform.position + transform.position) / 2f;
            laserCube.rotation = Quaternion.LookRotation(between);
            laserCube.localScale = new Vector3(.02f, .02f, between.magnitude);
        }

        if (debugBool3 != previousDebugBool3)
        {
            laserCube.gameObject.SetActive(debugBool3);
            previousDebugBool3 = debugBool3;
        }
	}

    public void DebugSetTarget (Transform target)
    {
        fox.SetTarget(target, true, true);
    }

    public void FlipDebugBool3()
    {
        debugBool3 = !debugBool3;
    }
}
