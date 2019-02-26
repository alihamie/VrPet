using EasyInputVR.Core;
using UnityEngine;

namespace EasyInputVR.Misc
{

    [AddComponentMenu("EasyInputGearVR/Miscellaneous/Steering")]
    public class Steering : MonoBehaviour
    {
        //Gameobjects and gameobject accessories. And some miscellaneous.
        Rigidbody myRigidbody;
        public Transform steeringWheel;
        public WheelCollider[] wheelColliders = new WheelCollider[4];
        public Transform[] wheelMeshes = new Transform[4];
        public AudioClip carHorn;
        Vector3 myOrientation = Vector3.zero;

        Quaternion wheelAngle;
        Vector3 wheelPosition;

        // The settings. They don't need to be changed.
        float maxSpeed = 8f;

        // The vars. These change. Sometimes a lot.
        public bool gasPressed;

        public float actualSpeed = 0;
        private float horizontal;

        // Below is the stuff that makes engine go vroom. I considered having it in a seperate script, but I don't know if I'll ever need to make things go vroom in sync with their speed outside this context.
        public AudioSource engineNoiseSource;
        private float maxVolume = .45f;
        private float minVolume = .2f;
        private float minPitch = .4f;

        void OnEnable()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            EasyInputHelper.On_Motion += localMotion;
#endif
            EasyInputHelper.On_ClickStart += localClickStart;
            EasyInputHelper.On_ClickEnd += localClickEnd;
            EasyInputHelper.On_Touch += localTouch;
            EasyInputHelper.On_TouchEnd += localTouchEnd;
            EasyInputHelper.On_TouchStart += localTouchStart;
        }

        void OnDestroy()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            EasyInputHelper.On_Motion -= localMotion;
#endif
            EasyInputHelper.On_Click -= localClickStart;
            EasyInputHelper.On_Click -= localClickEnd;
            EasyInputHelper.On_Touch -= localTouch;
            EasyInputHelper.On_TouchEnd -= localTouchEnd;
            EasyInputHelper.On_TouchStart -= localTouchStart;
        }

        private void OnDisable()
        {
            gasPressed = false;
            engineNoiseSource.Stop();
            actualSpeed = 0;
        }

        void Start()
        {
            myRigidbody = this.GetComponent<Rigidbody>();
            // This configures the number of physics substeps for each of the wheels.
            foreach (var wheels in wheelColliders)
            {
                wheels.ConfigureVehicleSubsteps(12f, 6, 6);
            }
        }

        void Update()
        {
            if (Mathf.Abs(actualSpeed) > 0 && !gasPressed)
            {
                actualSpeed *= (2 - Mathf.Min(Time.deltaTime, 2)) / 2;

                if (Mathf.Abs(actualSpeed) < .4f)
                {
                    actualSpeed = 0;
                }
                //float previousSign = Mathf.Sign(actualSpeed);
                //actualSpeed -= Time.deltaTime * previousSign;

                //if (previousSign != Mathf.Sign(actualSpeed))
                //{
                //    actualSpeed = 0;
                //}
            }

            //Steering
            steerBall(myOrientation);

            //This makes the wheel meshes rotate and move in sync with the wheel colliders.
            for (int i = 0; i < 4; i++)
            {
                wheelColliders[i].GetWorldPose(out wheelPosition, out wheelAngle);
                wheelMeshes[i].position = wheelPosition;
                wheelMeshes[i].rotation = wheelAngle;
            }

            //Here's where we go vroom.
            if (!engineNoiseSource.isPlaying && actualSpeed != 0)
            {
                engineNoiseSource.Play();
            }
            else if (engineNoiseSource.isPlaying && actualSpeed == 0)
            {
                engineNoiseSource.Stop();
            }

            //This makes the vroom change in pitch and volume along a specified range as the car speeds up.
            engineNoiseSource.pitch = ((Mathf.Abs(actualSpeed) / maxSpeed) * (1 - minPitch)) + minPitch;
            engineNoiseSource.volume = ((Mathf.Abs(actualSpeed) / maxSpeed) * (maxVolume - minVolume)) + minVolume;
        }

        void FixedUpdate()
        {
            for (int i = 0; i < 4; i++)
            {
                wheelColliders[i].motorTorque = actualSpeed;
            }
        }

        public void steerBall(Vector3 myOrientation)
        {

            if (myOrientation != Vector3.zero)
            {
                horizontal = myOrientation.z;
                horizontal = (horizontal > 180f) ? (horizontal - 360f) : horizontal;
            }
            else
            {
                horizontal = 0f;
            }

            // Rotates the steering wheel.
            steeringWheel.localEulerAngles = new Vector3(0, -horizontal + 180f, 0);
            steeringWheel.RotateAround(steeringWheel.position, transform.right, -60f);

            // This just angles the wheels where you wanna steer. Back wheel and front wheels turn counter of each other to help the car to deal with getting stuck against walls.
            horizontal *= .6f;
            wheelColliders[0].steerAngle = -horizontal;
            wheelColliders[2].steerAngle = -horizontal;
            wheelColliders[1].steerAngle = horizontal;
            wheelColliders[3].steerAngle = horizontal;

        }

        void localMotion(EasyInputVR.Core.Motion motion)
        {
            myOrientation = motion.currentOrientationEuler;
        }

        void localTouchStart(InputTouch touch)
        {
            gasPressed = true;
        }

        void localTouch(InputTouch touch)
        {
            actualSpeed = (touch.currentTouchPosition.y + .3f) * maxSpeed / 1.3f;
        }

        void localTouchEnd(InputTouch touch)
        {
            gasPressed = false;
        }

        void localClickStart(ButtonClick button)
        {
            if (carHorn)
            {
                engineNoiseSource.PlayOneShot(carHorn);
            }

            //if (button.button == EasyInputConstants.CONTROLLER_BUTTON.GearVRTrigger)
            //{
            //    gasPressed = true;
            //}
        }

        void localClickEnd(ButtonClick button)
        {
            //if (button.button == EasyInputConstants.CONTROLLER_BUTTON.GearVRTrigger)
            //{
            //    gasPressed = false;
            //}
        }
    }
}