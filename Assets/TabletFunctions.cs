using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyInputVR.Core;
using EasyInputVR.StandardControllers;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class TabletFunctions : MonoBehaviour
{
    public FoxTargetManager targetManager;
    public OVRScreenFade fader;
    public Transform car, player, tv, playerCouch, playerCar, tvCouch, tabletCouch, controller, sidePanel;

    [SerializeField]
    private AmbientSoundManager ambientManager;
    [SerializeField]
    private List<GameObject> offMeshLinks = new List<GameObject>();
    [SerializeField]
    private AudioMixer audioMixer;
    [SerializeField]
    private Slider masterSlider, musicSlider, ambientSlider;
    [SerializeField]
    private UpdateSliderCount masterCount, musicCount, ambientCount;
    private StandardLaserPointer laser;
    private bool currentlyActive = true, couchCurrentlyActive, couchTrans, carTrans, flippedOut = true;
    private Dictionary<string, Vector3> homePositions = new Dictionary<string, Vector3>();
    private Dictionary<string, Quaternion> homeRotations = new Dictionary<string, Quaternion>();
    private float sidePanelAngle = -90f;
#if UNITY_EDITOR
    public bool debugBool1, debugBool2;
    //public float debugFloat;
#endif

    private void Start()
    {
        EasyInputHelper.On_ClickStart += localClickStart;
        fader.FadeOutExit += localFadeOutExit;
        laser = controller.GetComponent<StandardLaserPointer>();
        homePositions.Add("Tablet", transform.position);
        homeRotations.Add("Tablet", transform.rotation);
        homePositions.Add("Player", player.position);
        homeRotations.Add("Player", player.rotation);
        homePositions.Add("Tv", tv.position);
        homeRotations.Add("Tv", tv.rotation);
        homePositions.Add("Car", car.position);
        homeRotations.Add("Car", car.rotation);
    }

    private void Update()
    {
        if (flippedOut && sidePanelAngle != -90f)
        {
            sidePanelAngle -= Time.deltaTime * 180f;

            if (sidePanelAngle < -90f)
            {
                sidePanelAngle = -90f;
            }

            sidePanel.localScale = new Vector3(((-sidePanelAngle / 2f) + 55f), 100f, 100f);
            sidePanel.localRotation = Quaternion.Euler(sidePanelAngle, -90f, 90f);
        }
        else if (!flippedOut && sidePanelAngle != 90f)
        {
            sidePanelAngle += Time.deltaTime * 180f;

            if (sidePanelAngle > 90f)
            {
                sidePanelAngle = 90f;
            }

            sidePanel.localScale = new Vector3(((-sidePanelAngle / 2f) + 55f), 100f, 100f);
            sidePanel.localRotation = Quaternion.Euler(sidePanelAngle, -90f, 90f);
        }

#if UNITY_EDITOR // This is debug, Unity Editor only stuff. When this script compiles for the .apk, this stuff will be left out.
        if (debugBool1)
        {
            debugBool1 = false;
            CouchTransition();
            //ForceMusicVolume(forceLevel);
        }

        if (debugBool2)
        {
            debugBool2 = false;
            CarTransition();
        }
#endif
    }

    private void localClickStart(ButtonClick button)
    {
        if (button.button == EasyInputConstants.CONTROLLER_BUTTON.GearVRTouchClick)
        {
            if (PlayerState.CURRENTSTATE == PlayerState.PLAYERSTATE.START)
            {
                if (currentlyActive)
                {
                    currentlyActive = false;
                    HideTablet();
                }
                else
                {
                    currentlyActive = true;
                    UnhideTablet();
                }
            }
            else if (PlayerState.CURRENTSTATE == PlayerState.PLAYERSTATE.DRIVING)
            {
                CarTransition();
            }
        }
    }

    public void CarTransition()
    { // Since there's really only one kind of fade out, using carTrans and couchTrans to tell the TabletFunctions script what kind of fadeout is happening helps keep all my ducks in a row.
        if (!fader.isFading)
        {
            fader.FadeOut();
            carTrans = true;
        }
    }

    public void CouchTransition()
    {
        if (!fader.isFading)
        {
            fader.FadeOut();
            couchTrans = true;
        }
    }

    public void HideTablet()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void UnhideTablet()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    public void ToggleCar()
    {
        if (!car.gameObject.activeSelf)
        {
            car.position = homePositions["Car"];
            car.rotation = homeRotations["Car"];
        }

        car.gameObject.SetActive(!car.gameObject.activeSelf);
    }

    public void ToggleSidePanel()
    {
        flippedOut = !flippedOut;
    }

    public void ResetScene()
    { // This should happen to reset whatever scene we find ourselves in. Hypothetically, anyway. Apparently blendshapes persist through scene loads or something?
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void localFadeOutExit()
    {
        fader.FadeIn();

        if (carTrans)
        {
            carTrans = false;

            if (PlayerState.CURRENTSTATE != PlayerState.PLAYERSTATE.DRIVING)
            {
                if (!car.gameObject.activeSelf)
                {
                    car.gameObject.SetActive(true);
                }

                player.position = playerCar.position;
                player.rotation = playerCar.rotation;
                player.parent = car;
                player.localScale = playerCar.localScale;
                PlayerState.CURRENTSTATE = PlayerState.PLAYERSTATE.DRIVING;
                HideTablet();
                laser.stopLaser();
                controller.GetChild(0).gameObject.SetActive(false);
            }
            else
            {
                if (couchCurrentlyActive)
                {
                    player.position = playerCouch.position;
                    player.rotation = playerCouch.rotation;
                }
                else
                {
                    player.position = homePositions["Player"];
                    player.rotation = homeRotations["Player"];
                }

                player.parent = null;
                player.localScale = new Vector3(1f, 1f, 1f);
                PlayerState.CURRENTSTATE = PlayerState.PLAYERSTATE.START;
                car.gameObject.SetActive(false);
                car.position = homePositions["Car"];
                car.rotation = homeRotations["Car"];
                laser.startLaser();
                controller.GetChild(0).gameObject.SetActive(true);
            }
        }
        else if (couchTrans)
        {
            couchTrans = false;

            if (!couchCurrentlyActive)
            {
                ForceMusicVolume(musicSlider.value * .25f);
                ForceAmbientVolume(ambientSlider.value * .25f);
                couchCurrentlyActive = true;
                player.position = playerCouch.position;
                player.rotation = playerCouch.rotation;
                transform.position = tabletCouch.position;
                transform.rotation = tabletCouch.rotation;
                tv.position = tvCouch.position;
                tv.rotation = tvCouch.rotation;
                foreach (GameObject link in offMeshLinks)
                {
                    if (link)
                    {
                        link.SetActive(false);
                    }
                }
            }
            else
            {
                ForceMusicVolume(musicSlider.value * 4f);
                ForceAmbientVolume(ambientSlider.value * 4f);
                couchCurrentlyActive = false;
                player.position = homePositions["Player"];
                player.rotation = homeRotations["Player"];
                transform.position = homePositions["Tablet"];
                transform.rotation = homeRotations["Tablet"];
                tv.position = homePositions["Tv"];
                tv.rotation = homeRotations["Tv"];
                foreach (GameObject link in offMeshLinks)
                {
                    if (link)
                    {
                        link.SetActive(true);
                    }
                }
            }
        }
    }

    private float LinearToDecibel(float linear)
    { // This is necessary because the volume slider on audiogroup only work in decibels, and decibels are logarithmic, not linear.
        float decibel;

        if (linear > .001)
        {
            decibel = 20f * Mathf.Log10(linear);
        }
        else
        {
            decibel = -80f;
        }

        return decibel;
    }

    public void SetMasterVolume(Slider slider)
    { // The audio sliders use both this function and the next one. They're making use of values that I previously exposed on the audio groups themselves.
        float level = LinearToDecibel(slider.value);
        audioMixer.SetFloat("MasterVolume", level);
    }

    public void SetMusicVolume(Slider slider)
    {
        float level = LinearToDecibel(slider.value);
        audioMixer.SetFloat("MusicVolume", level);
    }

    public void SetAmbientVolume(Slider slider)
    {
        ambientManager.UpdateVolume(slider.value);
    }

    private void ForceMusicVolume(float level)
    { // This is for the transition to and from the TV, to make it all silent. A bit jank, but it works.
        level = Mathf.Clamp01(level);
        musicSlider.value = level;
        musicCount.UpdateCount(musicSlider);
        SetMusicVolume(musicSlider);
    }

    private void ForceAmbientVolume(float level)
    {
        level = Mathf.Clamp01(level);
        ambientSlider.value = level;
        ambientCount.UpdateCount(ambientSlider);
        ambientManager.UpdateVolume(level);
    }
}
