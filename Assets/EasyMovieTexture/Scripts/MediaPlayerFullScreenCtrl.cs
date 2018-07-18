using UnityEngine;
using System.Collections;

public class MediaPlayerFullScreenCtrl : MonoBehaviour
{
    public GameObject objVideo;

    float iOrgWidth = 0;
    float iOrgHeight = 0;

    void Start()
    {
        Resize();
    }

    void Update()
    {
        if (iOrgWidth != Screen.width)
        {
            Resize();
        }

        if (iOrgHeight != Screen.height)
        {
            Resize();
        }
    }

    void Resize()
    {
        iOrgWidth = Screen.width;
        iOrgHeight = Screen.height;
        float fRatio = iOrgHeight / iOrgWidth;
        objVideo.transform.localScale = new Vector3(20.0f / fRatio, 20.0f / fRatio, 1.0f);
#if !UNITY_WEBGL
        objVideo.transform.GetComponent<MediaPlayerCtrl>().Resize();
#endif
    }
}
