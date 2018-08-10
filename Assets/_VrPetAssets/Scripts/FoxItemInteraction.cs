using EasyInputVR.StandardControllers;
using MalbersAnimations;
using UnityEngine;

public class FoxItemInteraction : MonoBehaviour
{
    public Transform jaw;
    public AnimalAIControl aiControl;
    private Transform grabbedItem;
    public Transform player;
    public GameObject fox;
    private FixedJoint joint;

    public void GrabItem()
    {
        Transform item = aiControl.GetClosestGrabableItem();

        if (item == null)
        {
            return;
        }

        if (grabbedItem != null && item == grabbedItem)
        {
            return;
        }

        if (grabbedItem != null)
        {
            DropItem();
        }

        item.parent = jaw;
        item.localPosition = Vector3.zero;
        grabbedItem = item;
        item.gameObject.GetComponent<Rigidbody>().isKinematic = true;

        foreach (Collider col in grabbedItem.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        ActionZone actionZone = item.GetComponent<ActionZone>();
        StandardGrabReceiver grabReceiver = grabbedItem.GetComponent<StandardGrabReceiver>();
        if (grabReceiver)
        {
            grabReceiver.SetIsGrabbed(true);
		}
		grabReceiver.enabled = false;
	}

    public void DropItem()
    {
        if (grabbedItem == null)
        {
            return;
        }

        grabbedItem.parent = null;
        grabbedItem.GetComponent<Rigidbody>().isKinematic = false;

        foreach (Collider col in grabbedItem.GetComponentsInChildren<Collider>())
        {
            col.enabled = true;
        }

        if (aiControl != null)
        {
            aiControl.isWandering = true;
            aiControl.SetClosestGrabbableItem(null);
        }

        StandardGrabReceiver grabReceiver = grabbedItem.GetComponent<StandardGrabReceiver>();
		grabReceiver.enabled = true;
        if (grabReceiver)
        {
            grabReceiver.SetIsGrabbed(false);
        }
		
        grabbedItem = null;
    }

}
