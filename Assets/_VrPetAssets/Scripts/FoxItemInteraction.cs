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

        if (this.grabbedItem != null && item.GetInstanceID() == grabbedItem.GetInstanceID())
        {
            return;
        }

        if (this.grabbedItem != null)
        {
            DropItem();
        }

        item.parent = this.jaw;

        item.position = this.jaw.position;

        //if (item.name == "Frisbee")
        //{
        //    Debug.Log("Oh boy, a frisbee, my favorite toy!");
        //    item.rotation = FunctionalAssist.RelativeRotation(-Vector3.up, jaw.InverseTransformDirection(item.up), Vector3.forward, Vector3.right, item.rotation);
        //}

        grabbedItem = item;

        item.gameObject.GetComponent<Rigidbody>().isKinematic = true;

        foreach (Collider col in grabbedItem.GetComponentsInChildren<Collider>()) col.enabled = false;

        ActionZone actionZone = item.GetComponent<ActionZone>();

        //if (actionZone != null)
        //{
        //    actionZone.CancelInvoke();
        //    actionZone.StopAllCoroutines();
        //    actionZone.enabled = false;
        //}

        aiControl.isWandering = false;
        //aiControl.target = player;

        StandardGrabReceiver grabReceiver = grabbedItem.GetComponent<StandardGrabReceiver>();

        if (grabReceiver)
        {
            grabReceiver.SetIsGrabbed(true);
        }
    }

    public void DropItem()
    {

        if (grabbedItem == null)
        {
            return;
        }

        grabbedItem.parent = null;

        grabbedItem.GetComponent<Rigidbody>().isKinematic = false;

        foreach (Collider col in grabbedItem.GetComponentsInChildren<Collider>()) col.enabled = true;

        if (aiControl != null)
        {
            aiControl.isWandering = true;
            aiControl.SetClosestGrabbableItem(null);
        }

        StandardGrabReceiver grabReceiver = grabbedItem.GetComponent<StandardGrabReceiver>();

        if (grabReceiver)
        {
            grabReceiver.SetIsGrabbed(false);
        }

        grabbedItem = null;
    }

}
