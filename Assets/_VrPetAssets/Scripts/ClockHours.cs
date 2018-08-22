using UnityEngine;

public class ClockHours : MonoBehaviour {
    Transform[] hours;
    private float distanceAdjust = 6.44f;
    [SerializeField]
    private Transform bigHand, smallHand;
    float second;
    int hour, minute;


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

        System.DateTime currentTime = System.DateTime.Now;
        hour = currentTime.Hour;
        minute = currentTime.Minute;
        second = currentTime.Second;
    }

    void Update()
    {
        second += Time.deltaTime;
        if (second >= 60f)
        {
            minute++;
            second -= 60f;
            if (minute > 59)
            { // Once an hour, we read the system time. That should be enough to keep us on track.
                System.DateTime currentTime = System.DateTime.Now;
                hour = currentTime.Hour;
                minute = currentTime.Minute;
                second = currentTime.Second;
            }
        }
        //System.DateTime currentTime = System.DateTime.Now;
        bigHand.localRotation = Quaternion.Euler(-90f, 0, (hour + (minute / 60f)) * 30f);
        smallHand.localRotation = Quaternion.Euler(-90f, 0, (minute + (second / 60f)) * 6f);
    }
}
