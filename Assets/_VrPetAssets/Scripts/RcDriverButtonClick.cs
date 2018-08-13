using EasyInputVR.Core;
using UnityEngine;
using UnityEngine.EventSystems;


public class RcDriverButtonClick : BaseToyClickButton
{
	//for ben ===remove later===
	public bool triggerCar = false;

    public Transform mainCamera;
    public Transform driverCamera;
    public OVRScreenFade faderLeft;
    public OVRScreenFade fadeRight;

    private Vector3 initialCameraPosition;
    private Vector3 initialCameraScale;
    private Quaternion initialCameraRotation;
    private bool padClick;
    private bool prevPadClick;
    private bool imFading;

    public override void OnPointerDown(PointerEventData eventData)
    {
        faderLeft.FadeOut();
        fadeRight.FadeOut();
        imFading = true;
    }

    private void Start()
    {
        screenManager = this.gameObject.GetComponentInParent<TabletScreenManager>();
        visibility = this.transform.root.GetComponent<TabletVisibility>();
        tabletFunctionality = this.transform.GetComponentInParent<TabletFunctionality>();
        initialCameraPosition = mainCamera.position;
        initialCameraScale = mainCamera.localScale;
        initialCameraRotation = mainCamera.rotation;
    }

    void OnEnable()
    {
        EasyInputHelper.On_ClickStart += ClickStart;
        EasyInputHelper.On_ClickEnd += ClickEnd;
        faderLeft.FadeOutExit += FadeOutExit;
    }

    void OnDisable()
    {
        EasyInputHelper.On_ClickStart -= ClickStart;
        EasyInputHelper.On_ClickEnd -= ClickEnd;
        faderLeft.FadeOutExit -= FadeOutExit;
    }

    public void SwitchToDriverCamera()
    {
        if (!this.tabletFunctionality.isCarActive)
        {
            this.tabletFunctionality.ToggleCarbutton();
        }
        PlayerState.CURRENTSTATE = PlayerState.PLAYERSTATE.DRIVING;
        mainCamera.parent = tabletFunctionality.car;
        mainCamera.localScale = driverCamera.localScale;
        mainCamera.position = driverCamera.position;
        mainCamera.rotation = driverCamera.rotation;
    }

    public void SwitchToStartCamera()
    {
        PlayerState.CURRENTSTATE = PlayerState.PLAYERSTATE.START;
        mainCamera.parent = null;
        mainCamera.localScale = initialCameraScale;
        mainCamera.position = initialCameraPosition;
        mainCamera.rotation = initialCameraRotation;
        this.tabletFunctionality.ToggleCarbutton();
        this.tabletFunctionality.targetManager.WanderAgain();
    }

    private void Update()
    {
		if(triggerCar)
		{
			triggerCar = false;
			SwitchToDriverCamera();
		}

        //TODO make a global delegate to figure out quick pad Click instad of doing this everytime
        if (padClick == true && prevPadClick == false && PlayerState.CURRENTSTATE == PlayerState.PLAYERSTATE.DRIVING)
        {
            faderLeft.FadeOut();
            fadeRight.FadeOut();
            imFading = true;
        }

        prevPadClick = padClick;
    }


    void ClickStart(ButtonClick button)
    {
        if (button.button == EasyInputConstants.CONTROLLER_BUTTON.GearVRTouchClick)
        {
            padClick = true;
        }
    }

    void ClickEnd(ButtonClick button)
    {
        if (button.button == EasyInputConstants.CONTROLLER_BUTTON.GearVRTouchClick)
        {
            padClick = false;
        }
    }

    void OnDestroy()
    {
        EasyInputHelper.On_ClickStart -= ClickStart;
        EasyInputHelper.On_ClickEnd -= ClickEnd;
        faderLeft.FadeOutExit -= FadeOutExit;
    }

    public void FadeOutExit()
    {
        if (imFading)
        {
            if (PlayerState.CURRENTSTATE == PlayerState.PLAYERSTATE.DRIVING)
            {
                SwitchToStartCamera();
            }
            else
            {
                SwitchToDriverCamera();
            }

            faderLeft.FadeIn();
            fadeRight.FadeIn();

            imFading = false;
        }
    }
}