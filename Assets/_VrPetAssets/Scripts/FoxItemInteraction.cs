using MalbersAnimations;
using UnityEngine;

public class FoxItemInteraction : MonoBehaviour
{

    public Transform jaw;
    public AnimalAIControl aiControl;
    private Transform grabbedItem;
    public Transform player;
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

        if (this.jaw != null)
        {
            item.parent = this.jaw;
            item.position = this.jaw.position;
            grabbedItem = item;
            Rigidbody rigidbody = item.gameObject.GetComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            ActionZone actionZone = item.GetComponent<ActionZone>();
            if (actionZone != null)
            {
                actionZone.CancelInvoke();
                actionZone.StopAllCoroutines();
                actionZone.enabled = false;
            }
            aiControl.isWandering = false;
            aiControl.target = player;
        }

    }

    public void DropItem()
    {

        if (grabbedItem == null)
        {
            return;
        }

        grabbedItem.parent = null;
        grabbedItem.GetComponent<Collider>().enabled = true;
        grabbedItem.GetComponent<Rigidbody>().isKinematic = false;
        
        if (aiControl != null)
        {
            aiControl.isWandering = true;
            grabbedItem = null;
            aiControl.SetClosestGrabbableItem(null);
        }
    }

}
