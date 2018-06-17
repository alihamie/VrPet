using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientSound : MonoBehaviour {
    public AudioClip[] clips;
    public GameObject startPoint;
    public GameObject endPoint;
    private Vector3 startPos;
    private Vector3 endPos;
    public int delayTime = 200;
    public int delayRand = 50;
    public int delayCount = 200;
    public float volume = .5f;

    // Use this for initialization
    void Start () {
        startPos = startPoint.transform.position;
        endPos = endPoint.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (delayCount == 0)
        {
            AudioSource.PlayClipAtPoint(clips[Random.Range(0, clips.Length)], new Vector3 (Random.Range(startPos.x, endPos.x), Random.Range(startPos.y, endPos.y), Random.Range(startPos.z, endPos.z)), volume);
            delayCount = delayTime + Random.Range(0, delayRand);
        }
        else
        {
            delayCount--;
        }
	}
}
