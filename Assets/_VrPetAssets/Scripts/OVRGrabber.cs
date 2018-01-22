/************************************************************************************

Copyright   :   Copyright 2014 Oculus VR, LLC. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.3 (the "License");
you may not use the Oculus VR Rift SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

http://www.oculus.com/licenses/LICENSE-3.3

Unless required by applicable law or agreed to in writing, the Oculus VR SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

using MalbersAnimations;
using MalbersAnimations.Utilities;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Allows grabbing and throwing of objects with the OVRGrabbable component on them.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class OVRGrabber : MonoBehaviour
{
    // Grip trigger thresholds for picking up objects, with some hysteresis.
    public float grabBegin = 0.55f;
    public float grabEnd = 0.35f;
    public LookAt lookAt;
    public AnimalAIControl aiAgent;
    public Transform UiTablet;
    // Demonstrates parenting the held object to the hand's transform when grabbed.
    // When false, the grabbed object is moved every FixedUpdate using MovePosition. 
    // Note that MovePosition is required for proper physics simulation. If you set this to true, you can
    // easily observe broken physics simulation by, for example, moving the bottom cube of a stacked
    // tower and noting a complete loss of friction.
    [SerializeField]
    protected bool m_parentHeldObject = false;

    // Child/attached transforms of the grabber, indicating where to snap held objects to (if you snap them).
    // Also used for ranking grab targets in case of multiple candidates.
    [SerializeField]
    protected Transform m_gripTransform = null;
    // Child/attached Colliders to detect candidate grabbable objects.
    [SerializeField]
    protected Collider[] m_grabVolumes = null;

    // Should be OVRInput.Controller.LTouch or OVRInput.Controller.RTouch.
    [SerializeField]
    protected OVRInput.Controller m_controller;

    [SerializeField]
    protected Transform m_parentTransform;

    protected bool m_grabVolumeEnabled = true;
    protected Vector3 m_lastPos;
    protected Quaternion m_lastRot;
    protected Quaternion m_anchorOffsetRotation;
    protected Vector3 m_anchorOffsetPosition;
    protected float m_prevFlex;
	protected OVRGrabbable m_grabbedObj = null;
    protected Vector3 m_grabbedObjectPosOff;
    protected Quaternion m_grabbedObjectRotOff;
	protected Dictionary<OVRGrabbable, int> m_grabCandidates = new Dictionary<OVRGrabbable, int>();
	protected bool operatingWithoutOVRCameraRig = true;
    protected OVRGrabbable currentGrabbaleObject;
    public Transform ReadyToPlayArea;

    /// <summary>
    /// The currently grabbed object.
    /// </summary>
    public OVRGrabbable grabbedObject
    {
        get { return m_grabbedObj; }
    }

    public void SetCurrentGrabableObject(OVRGrabbable oVRGrabbable)
    {
        this.currentGrabbaleObject = oVRGrabbable;
    }

	public void ForceRelease(OVRGrabbable grabbable)
    {
        bool canRelease = (
            (m_grabbedObj != null) &&
            (m_grabbedObj == grabbable)
        );
        if (canRelease)
        {
            GrabEnd();
        }
    }

    protected virtual void Awake()
    {
        m_anchorOffsetPosition = transform.localPosition;
        m_anchorOffsetRotation = transform.localRotation;

		// If we are being used with an OVRCameraRig, let it drive input updates, which may come from Update or FixedUpdate.

		OVRCameraRig rig = null;
		if (transform.parent != null && transform.parent.parent != null)
			rig = transform.parent.parent.GetComponent<OVRCameraRig>();
		
		if (rig != null)
		{
			rig.UpdatedAnchors += (r) => {OnUpdatedAnchors();};
			operatingWithoutOVRCameraRig = false;
		}
    }

    protected virtual void Start()
    {
        this.UpdateParent();
    }

    private void UpdateParent()
    {
        m_lastPos = transform.position;
        m_lastRot = transform.rotation;
        if (m_parentTransform == null)
        {
            if (gameObject.transform.parent != null)
            {
                m_parentTransform = gameObject.transform.parent.transform;
            }
            else
            {
                m_parentTransform = new GameObject().transform;
                m_parentTransform.position = Vector3.zero;
                m_parentTransform.rotation = Quaternion.identity;
            }
        }
    }
	void FixedUpdate()
	{
		if (operatingWithoutOVRCameraRig)
			OnUpdatedAnchors();
	}

    // Hands follow the touch anchors by calling MovePosition each frame to reach the anchor.
    // This is done instead of parenting to achieve workable physics. If you don't require physics on 
    // your hands or held objects, you may wish to switch to parenting.
    void OnUpdatedAnchors()
    {
        Vector3 handPos = OVRInput.GetLocalControllerPosition(m_controller);
        Quaternion handRot = OVRInput.GetLocalControllerRotation(m_controller);
        if (m_parentTransform == null)
        {
            this.UpdateParent();
        }
        Vector3 destPos = m_parentTransform.TransformPoint(m_anchorOffsetPosition + handPos);
        Quaternion destRot = m_parentTransform.rotation * handRot * m_anchorOffsetRotation;

        if (!m_parentHeldObject)
        {
            MoveGrabbedObject(destPos, destRot);
        }
        m_lastPos = transform.position;
        m_lastRot = transform.rotation;

        bool triggeredPrimaryIndex= OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, m_controller);
        bool touchPadTrigger = OVRInput.GetDown(OVRInput.Button.PrimaryTouchpad, m_controller);

#if (UNITY_EDITOR)

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckForGrabOrRelease(triggeredPrimaryIndex);
            return;
        }
#endif

        if (touchPadTrigger)
        {
            ToggleTablet();
        }

        if (triggeredPrimaryIndex)
        {
            CheckForGrabOrRelease(triggeredPrimaryIndex);
        }
    }
    public void ToggleTablet()
    {
        UiTablet.gameObject.SetActive(!UiTablet.gameObject.activeInHierarchy);
    }

    void OnDestroy()
    {
        if (m_grabbedObj != null)
        {
            GrabEnd();
        }
    }

    protected void CheckForGrabOrRelease(bool triggered)
    {
        if (this.currentGrabbaleObject == null)
        {
            Debug.unityLogger.Log("VRPET", "Grabable object is null");
            return;
        }

        if (!this.currentGrabbaleObject.isGrabbed && m_grabbedObj == null)
        {
            Debug.unityLogger.Log("VRPET", "GrabBegin");
            GrabBegin();
        }
        else
        {
            Debug.unityLogger.Log("VRPET", "GrabEnd");
            GrabEnd();
        }
    }


    public void GrabBegin(OVRGrabbable grabableObject)
    {
        grabableObject.IsGrabable = true;
        this.SetCurrentGrabableObject(grabableObject);
        GrabBegin();
    }

    public OVRGrabbable GetCurrentGrababbleObject()
    {
        return this.currentGrabbaleObject;
    }

    public virtual void GrabBegin()
    {
        if (this.currentGrabbaleObject == null || !this.currentGrabbaleObject.IsGrabable)
        {
            Debug.unityLogger.Log("VRPET", "OBject is not grabable or null");
            return;
        }

        OVRGrabbable  grabbable = this.currentGrabbaleObject;
        Collider grabbableCollider = grabbable.grabPoints[0];
        
        // Disable grab volumes to prevent overlaps
        // GrabVolumeEnable(false);
        if (grabbable != null)
        {
            if (grabbable.isGrabbed)
            {
                grabbable.grabbedBy.OffhandGrabbed(grabbable);
            }

            m_grabbedObj = grabbable;
            m_grabbedObj.GrabBegin(this, grabbableCollider);
            //lookAt.Target = m_grabbedObj.transform;
            aiAgent.SetTarget( ReadyToPlayArea);
            aiAgent.isWandering = false;
            m_lastPos = transform.position;
            m_lastRot = transform.rotation;

            // Set up offsets for grabbed object desired position relative to hand.
            if(m_grabbedObj.snapPosition)
            {
                Vector3 localForward = m_gripTransform.worldToLocalMatrix.MultiplyVector(m_gripTransform.forward);
                m_grabbedObjectPosOff = transform.localPosition;
                
                if (m_grabbedObj.snapOffset)
                {
                    Vector3 snapOffset = m_grabbedObj.snapOffset.position;
                    if (m_controller == OVRInput.Controller.LTouch || m_controller == OVRInput.Controller.LTrackedRemote) snapOffset.x = -snapOffset.x;
                    m_grabbedObjectPosOff += snapOffset;
                }
            }
            else
            {
                Vector3 relPos = m_grabbedObj.transform.position - transform.position;
                relPos = Quaternion.Inverse(transform.rotation) * relPos;
                m_grabbedObjectPosOff = relPos;
            }

            if (m_grabbedObj.snapOrientation)
            {
                m_grabbedObjectRotOff = m_gripTransform.localRotation;
                if(m_grabbedObj.snapOffset)
                {
                    m_grabbedObj.transform.Rotate(Vector3.up, 90);
                    m_grabbedObjectRotOff = m_grabbedObj.snapOffset.rotation * m_grabbedObjectRotOff;
                }
            }
            else
            {
                Quaternion relOri = Quaternion.Inverse(transform.rotation) * m_grabbedObj.transform.rotation;
                m_grabbedObjectRotOff = relOri;
            }

            // Note: force teleport on grab, to avoid high-speed travel to dest which hits a lot of other objects at high
            // speed and sends them flying. The grabbed object may still teleport inside of other objects, but fixing that
            // is beyond the scope of this demo.
            MoveGrabbedObject(m_lastPos, m_lastRot, true);
            if(m_parentHeldObject)
            {
                m_grabbedObj.transform.parent = transform;
            }
        }
    }

    protected virtual void MoveGrabbedObject(Vector3 pos, Quaternion rot, bool forceTeleport = false)
    {
        if (m_grabbedObj == null)
        {
            return;
        }

        Rigidbody grabbedRigidbody = m_grabbedObj.grabbedRigidbody;
        Vector3 grabbablePosition = pos + rot * m_grabbedObjectPosOff;
        Quaternion grabbableRotation = rot * m_grabbedObjectRotOff;

        if (forceTeleport)
        {
            grabbedRigidbody.transform.position = grabbablePosition;
            grabbedRigidbody.transform.rotation = grabbableRotation;
        }
        else
        {
            grabbedRigidbody.MovePosition(grabbablePosition);
            grabbedRigidbody.MoveRotation(grabbableRotation);
        }
    }

    protected void GrabEnd()
    {
        if (m_grabbedObj != null)
        {
			OVRPose localPose = new OVRPose { position = OVRInput.GetLocalControllerPosition(m_controller), orientation = OVRInput.GetLocalControllerRotation(m_controller) };
            OVRPose offsetPose = new OVRPose { position = m_anchorOffsetPosition, orientation = m_anchorOffsetRotation };
            localPose = localPose * offsetPose;

			OVRPose trackingSpace = transform.ToOVRPose() * localPose.Inverse();
			Vector3 linearVelocity = trackingSpace.orientation * OVRInput.GetLocalControllerVelocity(m_controller);
			Vector3 angularVelocity = trackingSpace.orientation * OVRInput.GetLocalControllerAngularVelocity(m_controller);

            GrabbableRelease(linearVelocity, angularVelocity);
            m_grabbedObj = null;
        }

        // Re-enable grab volumes to allow overlap events
        GrabVolumeEnable(true);
    }

    protected void GrabbableRelease(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        m_grabbedObj.GrabEnd(linearVelocity, angularVelocity);
        aiAgent.isWandering = false;
        aiAgent.SetTarget(m_grabbedObj.transform);
        
        if(m_parentHeldObject) m_grabbedObj.transform.parent = null;
        m_grabbedObj = null;
        SetCurrentGrabableObject(null);
    }

    protected virtual void GrabVolumeEnable(bool enabled)
    {
        if (m_grabVolumeEnabled == enabled)
        {
            return;
        }

        m_grabVolumeEnabled = enabled;
        for (int i = 0; i < m_grabVolumes.Length; ++i)
        {
            Collider grabVolume = m_grabVolumes[i];
            grabVolume.enabled = m_grabVolumeEnabled;
        }

        if (!m_grabVolumeEnabled)
        {
            m_grabCandidates.Clear();
        }
    }

	protected virtual void OffhandGrabbed(OVRGrabbable grabbable)
    {
        if (m_grabbedObj == grabbable)
        {
            GrabbableRelease(Vector3.zero, Vector3.zero);
        }
    }
}
