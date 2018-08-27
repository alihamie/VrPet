using UnityEngine;

public class ClockHours : MonoBehaviour {
    [SerializeField]
    private Transform bigHand, smallHand;
    float second;
    int hour, minute;


    void Start () 
	{
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
