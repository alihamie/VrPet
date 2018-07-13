using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using SimpleJSON;
using System.Text;
using System;

public class Twitch_EMT : MonoBehaviour
{

    /*PRIVATE INFO DO NOT CHANGE THESE URLS OR VALUES*/
    // Note from Alex: This can be used to point at both youtube and twitch. It's as simple as changing the serverURI. Also, note the dependency on the Heroku service.
    private static readonly string[] serverSourceURI = { "https://unity-dev-youtube.herokuapp.com/api/info?url=https://www.twitch.tv/", "https://unity-dev-youtube.herokuapp.com/api/info?url=https://www.youtube.com/watch?v=" };
    private const string formatURI = "&format=best&flatten=true";
    /*END OF PRIVATE INFO*/
    [Header("Easy movie texture MediaPlayerCtrl component")]
    public MediaPlayerCtrl easyPlayer;

    public string liveResult;
    public TVUIStateManager stateManager;

    public string videoId = "twitchpresents";
    string previousVideoId;
    private string videoUrl;
    //start playing the video
    public bool playOnStart = false;
    public Texture blankTexture;

    public void Start()
    {
        if (playOnStart)
        {
            PlayVideo(videoId);
        }
    }

    public void PlayVideo(string _videoId, int sourceValue = 0)
    {
        videoId = _videoId;
        if (previousVideoId == videoId)
        {
            easyPlayer.Play();
            easyPlayer.isPlaying = true;
        }
        else if (previousVideoId == null)
        {
            StartCoroutine(LiveRequest(videoId, serverSourceURI[sourceValue]));
            previousVideoId = videoId;
        }
        else
        {
            easyPlayer.UnLoad();
            StartCoroutine(LiveRequest(videoId, serverSourceURI[sourceValue]));
            previousVideoId = videoId;
        }
    }

    IEnumerator LiveRequest(string videoID, string serverURI)
    {
        WWW request = new WWW(serverURI + "" + videoID + "" + formatURI);
        yield return request;
        var requestData = JSON.Parse(request.text);
        liveResult = requestData["videos"][0]["url"];
        StartCoroutine(LivePlay());
    }

    IEnumerator LivePlay() //The prepare not respond so, i forced to play after some seconds
    {
        yield return new WaitForSeconds(0.5f);
        string uri = "";
        uri = liveResult;
        easyPlayer.m_strFileName = uri;
        StartCoroutine(Play());
    }

    public IEnumerator Play()
    {
        yield return new WaitForSeconds(4);
        // These is just a sanity check to see if something is actually happening, or if we're just going through the motions.
        if (liveResult != null)
        {
            easyPlayer.Play();
            easyPlayer.isPlaying = true;
        }
        else
        {
            stateManager.ChangeState(2);
            GetComponent<Renderer>().material.mainTexture = blankTexture;
        }
    }

    // This is never called, but it might be. Eventually.
    public void Pause()
    {
        easyPlayer.isPlaying = false;
        easyPlayer.Stop();
    }

    public void End()
    {
        easyPlayer.UnLoad();
        GetComponent<Renderer>().material.mainTexture = blankTexture;
        previousVideoId = null;
        easyPlayer.isPlaying = false;
    }
}