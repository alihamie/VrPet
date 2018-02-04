using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabletFunctionality : MonoBehaviour {


    TabletScreenManager screenManager;
	// Use this for initialization
	void Start () {
        screenManager = this.GetComponent<TabletScreenManager>();
	}
	
	// Update is called once per frame
	void Update () {


		
	}

    public void GamePlayButton()
    {
        if (screenManager != null)
        {
            screenManager.NavigateToGameDetailScreen();
        }
    }

    public void BackButton()
    {
        if (screenManager != null)
        {
            screenManager.GoBack();
        }
    }
}
