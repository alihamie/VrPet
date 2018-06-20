using UnityEngine;
using System;
using System.Collections;
using EasyInputVR.Core;

namespace EasyInputVR.Misc
{

    [AddComponentMenu("EasyInputGearVR/Miscellaneous/Steering")]
    public class Steering : MonoBehaviour
    {
        Rigidbody myRigidbody;
        public Transform steeringWheel;
        public WheelCollider[] wheelColliders = new WheelCollider[4];
        public Transform[] wheelMeshes = new Transform[4];
        Vector3 myOrientation = Vector3.zero;

        Quaternion wheelAngle;
        Vector3 wheelPosition;

        const float sensitivity = .6f;
        const float maxSpeed = 7f;
        const float substepThreshold = 12f;
        const int substepBelow = 8, substepAbove = 8;

        bool gasPressed;
        bool brakePressed;
        float horizontal;
        float forwardSpeed = 0;
        float reverseTime = 0;

        //Vector3 actionVectorPosition;
        //Vector3 computerVector;

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
            //The first half second that the gas is pressed, the car moves at half speed in reverse. Otherwise, it's really easy to get stuck in corners or against walls.
            if (gasPressed && reverseTime > 0)
            {
                forwardSpeed = -maxSpeed/2;
                reverseTime -= Time.deltaTime;
            }
            else if (gasPressed && reverseTime <= 0)
            {
                forwardSpeed = maxSpeed;
            }
            else
            {
                forwardSpeed = 0;
                reverseTime = .5f;
            }

            //steering
            steerBall(myOrientation);
            
            //This makes the wheel meshes rotate and move in sync with the wheel colliders.
            for (int i = 0; i < 4; i++)
            {
                wheelColliders[i].GetWorldPose(out wheelPosition, out wheelAngle);
                wheelMeshes[i].position = wheelPosition;
                wheelMeshes[i].rotation = wheelAngle;
            }
        }

        private void FixedUpdate()
        {
            wheelColliders[0].motorTorque = forwardSpeed;
            wheelColliders[1].motorTorque = forwardSpeed;
            wheelColliders[2].motorTorque = forwardSpeed;
            wheelColliders[3].motorTorque = forwardSpeed;
        }

        public void steerBall(Vector3 myOrientation)
        {

            if (myOrientation != Vector3.zero)
            {
                horizontal = myOrientation.z;
                //vertical = myOrientation.x;

                //get into a -180 to 180 range
                horizontal = (horizontal > 180f) ? (horizontal - 360f) : horizontal;
                //vertical = (vertical > 180f) ? (vertical - 360f) : vertical;
            }
            else
            {
                horizontal = 0f;
                //vertical = 0f;
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