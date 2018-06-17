using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAudio : MonoBehaviour {

    public AudioClip clipOne, clipTwo;
    public AudioSource speakerForTheDead;
    public GameObject iSeeYou;
    public Vector3 hereIAm;
    public int theCount;
    public GameObject foxSayWhat;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        hereIAm = new Vector3(iSeeYou.transform.rotation.x, iSeeYou.transform.rotation.y, iSeeYou.transform.rotation.z);

        if (iSeeYou.transform.rotation.x < -.6f && theCount < 1)
        {
            AudioSource.PlayClipAtPoint(clipOne, foxSayWhat.transform.position);
            theCount = 100;
        }
        if (theCount > 0)
        {
            theCount--;
        }
	}
}
