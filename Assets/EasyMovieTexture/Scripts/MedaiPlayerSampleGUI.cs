using UnityEngine;
using System.Collections;

public class MedaiPlayerSampleGUI : MonoBehaviour {

	public MediaPlayerCtrl scrMedia;
	public bool bFinish = false;

	void Start () {
		scrMedia.OnEnd += OnEnd;
	}

	#if !UNITY_WEBGL
	void OnGUI() {
		if( GUI.Button(new Rect(50,50,100,100),"Load"))
		{
			scrMedia.Load("EasyMovieTexture.mp4");
			bFinish = false;
		}
		if( GUI.Button(new Rect(50,200,100,100),"Play"))
		{
			scrMedia.Play();
			bFinish = false;
		}
		if( GUI.Button(new Rect(50,350,100,100),"stop"))
		{
			scrMedia.Stop();
		}
		if( GUI.Button(new Rect(50,500,100,100),"pause"))
		{
			scrMedia.Pause();
		}
		if( GUI.Button(new Rect(50,650,100,100),"Unload"))
		{
			scrMedia.UnLoad();
		}
		if( GUI.Button(new Rect(50,800,100,100), " " + bFinish))
		{
		}
		if( GUI.Button(new Rect(200,50,100,100),"SeekTo"))
		{
			scrMedia.SeekTo(10000);
		}
		if( scrMedia.GetCurrentState() == MediaPlayerCtrl.MEDIAPLAYER_STATE.PLAYING)
		{
			if( GUI.Button(new Rect(200,200,100,100),scrMedia.GetSeekPosition().ToString()))
			{
				scrMedia.SetSpeed(2.0f);
			}
			if( GUI.Button(new Rect(200,350,100,100),scrMedia.GetDuration().ToString()))
			{
				scrMedia.SetSpeed(1.0f);
			}
			if( GUI.Button(new Rect(200,450,100,100),scrMedia.GetVideoWidth().ToString()))
			{
			}
			if( GUI.Button(new Rect(200,550,100,100),scrMedia.GetVideoHeight().ToString()))
			{
			}
		}
		if( GUI.Button(new Rect(200,650,100,100),scrMedia.GetCurrentSeekPercent().ToString()))
		{
		}
        if (GUI.Button(new Rect(300, 650, 100, 100), scrMedia.GetCurrentState().ToString()))
        {
        }
	}
	#endif

	void OnEnd()
	{
		bFinish = true;
	}
}
