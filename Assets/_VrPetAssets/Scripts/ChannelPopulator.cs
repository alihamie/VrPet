using System;
using System.Collections;
using UnityEngine;

public class ChannelPopulator : MonoBehaviour {
    private string[] channelResults;

    public Transform channelButtonParent;

	void Start () 
	{
        StartCoroutine(ChannelRequest());
    }
	
    private IEnumerator ChannelRequest()
    {
        WWW request = new WWW("http://www.lucasearth.com/class/z_Covalent/twitch_streamers.php");
        yield return request;
        channelResults = request.text.Split(new string[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < channelButtonParent.childCount - 1; i++)
        {
            Transform child = channelButtonParent.GetChild(i);
            child.GetComponent<ChannelButton>().channel = child.name = channelResults[i];
        }
    }
}
