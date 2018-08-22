using EasyInputVR.Core;
using UnityEngine;
using UnityEngine.UI;

namespace EasyInputVR.Misc
{
    public class TVRemoteInterface : MonoBehaviour
    {

        public TVUIStateManager stateManager;
        public Transform mainMenuButtonsParent;
        public ChannelButtonManager buttonManager;
        public AudioSource tvSpeaker;
        public AudioClip selectNoise, choiceNoise;
        public Transform rightHandAnchor;

        private StandardControllers.StandardGrabReceiver remote;
        private GameObject controllerModel;
        private StandardControllers.StandardLaserPointer laser;

        int touchedNumber = -1;
        int previousTouchedNumber = -1;

        Vector2 touchPosition;
        Vector2 previousTouchPosition;
        float swipeRotation, totalSwipe = 0;
        bool previousGrabMode, touching, padClick, previousPadClick;

        void Start()
        {
            remote = transform.GetComponent<StandardControllers.StandardGrabReceiver>();
            controllerModel = rightHandAnchor.GetChild(0).GetChild(0).gameObject;
            laser = rightHandAnchor.GetChild(0).GetComponent<StandardControllers.StandardLaserPointer>();
        }

        void OnDisable() // This section of code is just to make sure we properly disassociate this script from those events.
        {
            EasyInputHelper.On_TouchStart -= localTouchStart;
            EasyInputHelper.On_TouchEnd -= localTouchEnd;
            EasyInputHelper.On_Touch -= localTouch;
            EasyInputHelper.On_ClickStart -= clickStart;
            EasyInputHelper.On_ClickEnd -= clickEnd;
        }

        void Update()
        {
            // This bit is just to hide the laser and the controller's model while the remote is grabbed. Hiding the laser isn't entirely cosmetic, so if that become an issue I'll need to change that script instead.
            if (remote.grabMode)
            {
                if (!previousGrabMode)
                {
                    EasyInputHelper.On_TouchStart += localTouchStart;
                    EasyInputHelper.On_TouchEnd += localTouchEnd;
                    EasyInputHelper.On_Touch += localTouch;
                    EasyInputHelper.On_ClickStart += clickStart;
                    EasyInputHelper.On_ClickEnd += clickEnd;
                    previousGrabMode = remote.grabMode;
                    laser.stopLaser();
                    controllerModel.SetActive(false);
                    PlayerState.CURRENTSTATE = PlayerState.PLAYERSTATE.REMOTE;
                }

                transform.rotation = rightHandAnchor.rotation;

                if (padClick && !previousPadClick)
                {
                    TouchClickEvaluator();
                    previousPadClick = padClick;
                }
                else if (!padClick && previousPadClick)
                {
                    previousPadClick = padClick;
                }
            }
            else
            {
                if (previousGrabMode)
                {
                    EasyInputHelper.On_TouchStart -= localTouchStart;
                    EasyInputHelper.On_TouchEnd -= localTouchEnd;
                    EasyInputHelper.On_Touch -= localTouch;
                    EasyInputHelper.On_ClickStart -= clickStart;
                    EasyInputHelper.On_ClickEnd -= clickEnd;
                    previousPadClick = padClick = previousGrabMode = false;
                    laser.startLaser();
                    controllerModel.SetActive(true);
                    stateManager.ChangeState(-1);
                    PlayerState.CURRENTSTATE = PlayerState.PLAYERSTATE.START;
                }
            }
        }

        void localTouchStart(InputTouch touch)
        {
            touchPosition = touch.currentTouchPosition;
        }

        void localTouch(InputTouch touch)
        {
            previousTouchPosition = touchPosition;
            touchPosition = touch.currentTouchPosition; // We're dividing by Time.deltaTime on the next line because otherwise lag would cause us to strongly overestimate the signedAngle generated.
            swipeRotation = Vector2.SignedAngle(previousTouchPosition, touchPosition) / Time.deltaTime;

            if (stateManager.activeChildNumber == 0)
            {
                MainMenuNumberToucher();
            }

            if (stateManager.activeChildNumber == 1)
            {
                totalSwipe += (touchPosition.x - previousTouchPosition.x);

                if (totalSwipe > 1.2f || totalSwipe < -1.2f)
                {
                    ChannelMenuNumberSwiper((int)Mathf.Sign(totalSwipe));
                    totalSwipe = 0;
                }
            }
        }

        void localTouchEnd(InputTouch touch)
        {
            totalSwipe = 0f;
        }

        void clickStart(ButtonClick button)
        {
            if (button.button == EasyInputConstants.CONTROLLER_BUTTON.GearVRTouchClick && remote.grabMode)
            {
                padClick = true;
            }
        }

        void clickEnd(ButtonClick button)
        {
            if (button.button == EasyInputConstants.CONTROLLER_BUTTON.GearVRTouchClick && remote.grabMode)
            {
                padClick = false;
            }
        }

        void MainMenuNumberToucher()
        {
            // This is where we figure out where each UI button is on the controller's touchpad. It starts at 0, which is centered on the positive y axis, and goes around clockwise. 6-8 is probably as high as we can go before this method of controlling selection is entirely unuseable.

            var theta = -Mathf.PI / mainMenuButtonsParent.childCount;

            touchedNumber = Mathf.CeilToInt((-Vector2.SignedAngle(new Vector2(-Mathf.Sin(theta), -Mathf.Cos(theta)), touchPosition) + 180f) * mainMenuButtonsParent.childCount / 360f) - 1;

            if (touchedNumber == -1)
            {
                touchedNumber = 0;
            }

            if (previousTouchedNumber != touchedNumber)
            {
                if (touchedNumber > -1)
                {
                    mainMenuButtonsParent.GetChild(touchedNumber).localScale = new Vector3(.4f, .4f, .4f);
                    mainMenuButtonsParent.GetChild(touchedNumber).GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                    tvSpeaker.PlayOneShot(selectNoise);
                }
                if (previousTouchedNumber > -1)
                {
                    mainMenuButtonsParent.GetChild(previousTouchedNumber).localScale = new Vector3(.35f, .35f, .35f);
                    mainMenuButtonsParent.GetChild(previousTouchedNumber).GetComponent<Image>().color = new Color32(255, 255, 255, 140);
                }

                previousTouchedNumber = touchedNumber;
            }
        }

        void ChannelMenuNumberSwiper(int currentSwipeDirection)
        {
            buttonManager.selectedInt -= currentSwipeDirection;
            tvSpeaker.PlayOneShot(selectNoise);
        }

        void TouchClickEvaluator()
        {
            if (stateManager.activeChildNumber == -1)
            {
                stateManager.ChangeState(0);
                tvSpeaker.PlayOneShot(choiceNoise);
            }
            else if (stateManager.activeChildNumber == 0)
            {
                mainMenuButtonsParent.GetChild(touchedNumber).GetComponent<Button>().onClick.Invoke();
                tvSpeaker.PlayOneShot(choiceNoise);
            }
            else if (stateManager.activeChildNumber == 1)
            {
                stateManager.ChangeState(-1);
                buttonManager.PlaySelected();
                tvSpeaker.PlayOneShot(choiceNoise);
            }
            else if (stateManager.activeChildNumber > 1)
            {
                stateManager.ChangeState(0);
                tvSpeaker.PlayOneShot(choiceNoise);
            }
        }
    }
}

