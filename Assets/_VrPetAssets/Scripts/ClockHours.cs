using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockHours : MonoBehaviour {
    Transform[] hours;
    private float distanceAdjust = 6.44f;
    [SerializeField]
    private Transform bigHand, smallHand;

	void Start () 
	{
        hours = new Transform[transform.GetChild(0).childCount];
        for (int i = 0; i < transform.GetChild(0).childCount; i++)
        {
            hours[i] = transform.GetChild(0).GetChild(i);
        }
        for (int i = 0; i < hours.Length; i++)
        {
            float theta = (2 * Mathf.PI / hours.Length) * i;
            hours[i].localPosition = new Vector3(Mathf.Sin(theta), Mathf.Cos(theta), 0f) * distanceAdjust;
        }
    }

    void Update()
    {
        System.DateTime currentTime = System.DateTime.Now;
        bigHand.localRotation = Quaternion.Euler(-90f, 0, (currentTime.Hour + (currentTime.Minute / 60f)) * 30f);
        smallHand.localRotation = Quaternion.Euler(-90f, 0, (currentTime.Minute + (currentTime.Second / 60f)) * 6f);
    }
}
