using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabletScreenManager : MonoBehaviour {

    // all screens back leads to this one so there is no need for previous screen
    public  Transform initialScreen;

    private Transform currentScreen;

    private const int GAMEDETAILSCREEN = 1;

    private Transform lastClickedScreen;


	void Awake ()
    {
        currentScreen = initialScreen;
        lastClickedScreen = initialScreen;
        currentScreen.gameObject.SetActive(true);
	}

    public void NavigateToGameDetailScreen()
    {
        currentScreen.gameObject.SetActive(false);
        Transform nextScreen = this.transform.GetChild(GAMEDETAILSCREEN);
        if (nextScreen != null)
        {
            nextScreen.gameObject.SetActive(true);
        }
        lastClickedScreen = currentScreen;
        currentScreen = nextScreen;
    }

    public void GoBack(Transform screenToGoBackfrom)
    {
        lastClickedScreen = screenToGoBackfrom;
        currentScreen.gameObject.SetActive(false);
        currentScreen = initialScreen;
        currentScreen.gameObject.SetActive(true);
    }

}
