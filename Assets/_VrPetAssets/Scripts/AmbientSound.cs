using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientSound : MonoBehaviour {
    AudioClip[] clips;
    public GameObject startPoint;
    public GameObject endPoint;
    Vector3 startPos;
    Vector3 endPos;
    public float delayTime = 2f;
    public float delayRand = .5f;
    public float delayCount = 2f;
    public float volume = .5f;
    public string clipFolderPath;

    void Start () {
        clips = Resources.LoadAll<AudioClip>(clipFolderPath);
        delayCount = Time.time + delayCount;
        startPos = startPoint.transform.position;
        endPos = endPoint.transform.position;
    }
	
	void Update () {
        if (Time.time > delayCount) {
            AudioSource.PlayClipAtPoint(clips[Random.Range(0, clips.Length)], new Vector3(Random.Range(startPos.x, endPos.x), Random.Range(startPos.y, endPos.y), Random.Range(startPos.z, endPos.z)), volume);
            delayCount = Time.time + delayTime + Random.Range(0, delayRand);
        }
    }
}
