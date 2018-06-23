using UnityEngine;
using System;
using System.Collections;
using EasyInputVR.Core;

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
        Vector3 myOrientation = Vector3.zero;

        Quaternion wheelAngle;
        Vector3 wheelPosition;

        // The settings. They don't need to be changed.
        float lowestSpeed = -4f;
        float maxSpeed = 8f;
        float timeUntilMaxSpeed = 6f;
        const float sensitivity = .6f;
        const float substepThreshold = 12f;
        const int substepBelow = 6, substepAbove = 6;

        // The vars. These change. Sometimes a lot.
        float normalizedSpeed = 0f;
        public bool gasPressed;
        float actualSpeed = 0;
        bool brakePressed;
        float horizontal;

        // Below is the stuff that makes engine go vroom. I considered having it in a seperate script, but I don't know if I'll ever need to make things go vroom in sync with their speed outside this context.
        public AudioSource engineNoiseSource;
        float maxVolume = .45f;
        float minVolume = .2f;
        float minPitch = .4f;

        void OnEnable()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            EasyInputHelper.On_Motion += localMotion;
#endif
            EasyInputHelper.On_ClickStart += localClickStart;
            EasyInputHelper.On_ClickEnd += localClickEnd;
        }

        void OnDestroy()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            EasyInputHelper.On_Motion -= localMotion;
#endif
            EasyInputHelper.On_Click -= localClickStart;
            EasyInputHelper.On_Click -= localClickEnd;
        }

        void Start()
        {
            myRigidbody = this.GetComponent<Rigidbody>();
            // This configures the number of physics substeps for each of the wheels.
            foreach (var wheels in wheelColliders)
            {
                wheels.ConfigureVehicleSubsteps(substepThreshold, substepAbove, substepBelow);
            }
        }

        void Update()
        {
            // So. All of this ensures that when the user hits the trigger, the car briefly goes in reverse before slowly accelerating to its max speed forward.
            if (gasPressed && normalizedSpeed >= (maxSpeed - lowestSpeed))
            {
                normalizedSpeed = maxSpeed - lowestSpeed;
            }
            else if (gasPressed && normalizedSpeed < (maxSpeed - lowestSpeed))
            {
                normalizedSpeed += ((maxSpeed - lowestSpeed)/timeUntilMaxSpeed) * Time.deltaTime;
            }
            else if (normalizedSpeed + lowestSpeed > 0)
            {
                normalizedSpeed -= ((maxSpeed - lowestSpeed) / timeUntilMaxSpeed) * Time.deltaTime * 2;
            }
            else
            {
                normalizedSpeed = 0;
            }

            // actualSpeed is different from normalizedSpeed because I need to be able to tell the difference between the car being at rest with 0 speed and the car merely passing through 0 coming from reverse to maxspeed.
            // And I need to be able to go from reverse to maxspeed because, as far as I know, the only input I can use to control the car is the trigger and the rotation of the controller.
            if (gasPressed)
            {
                actualSpeed = normalizedSpeed + lowestSpeed;
            }
            else
            {
                actualSpeed = 0;
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
            if (!engineNoiseSource.isPlaying && gasPressed)
            {
                engineNoiseSource.Play();
            }
            else if (engineNoiseSource.isPlaying && !gasPressed && normalizedSpeed == 0)
            {
                engineNoiseSource.Stop();
            }

            // This makes the vroom change in pitch and volume along a specified range as the car speeds up.
            engineNoiseSource.pitch = (Mathf.Abs(actualSpeed) / maxSpeed) * (1 - minPitch) + minPitch;
            engineNoiseSource.volume = (Mathf.Abs(actualSpeed) / maxSpeed) * (maxVolume - minVolume) + minVolume;
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
            horizontal *= sensitivity;
            wheelColliders[0].steerAngle = -horizontal;
            wheelColliders[2].steerAngle = -horizontal;
            wheelColliders[1].steerAngle = horizontal;
            wheelColliders[3].steerAngle = horizontal;

        }

        void localMotion(EasyInputVR.Core.Motion motion)
        {
            myOrientation = motion.currentOrientationEuler;
        }

        void localClickStart(ButtonClick button)
        {
            if (button.button == EasyInputConstants.CONTROLLER_BUTTON.GearVRTouchClick)
            {
                brakePressed = true;
            }
            else if (button.button == EasyInputConstants.CONTROLLER_BUTTON.GearVRTrigger)
            {
                gasPressed = true;
            }
        }

        void localClickEnd(ButtonClick button)
        {
            if (button.button == EasyInputConstants.CONTROLLER_BUTTON.GearVRTouchClick)
            {
                brakePressed = false;
            }
            else if (button.button == EasyInputConstants.CONTROLLER_BUTTON.GearVRTrigger)
            {
                gasPressed = false;
            }
        }
    }
}