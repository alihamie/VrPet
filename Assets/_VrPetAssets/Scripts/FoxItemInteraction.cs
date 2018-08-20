using EasyInputVR.StandardControllers;
using MalbersAnimations;
using UnityEngine;

public class FoxItemInteraction : MonoBehaviour
{
    public Transform jaw;
    public AnimalAIControl aiControl;
    private Transform grabbedItem;
    public Transform player;
    public AnimalAIControl fox;
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

		if(item.name == "ShinyRedBall")
		{
			item.localPosition = new Vector3(0.06f, 0.05f, 0f);
			fox.TriggerJawOverride(1, 0);
		}

		if (item.name == "Frisbee")
		{
			item.localRotation = Quaternion.Euler(new Vector3(0, 180, -76));
			item.localPosition = new Vector3(0.05f, 0.07f, 0f);
			fox.TriggerJawOverride(1, 30);
		}

		if (item.name == "RemoteControl")
		{
			item.localRotation = Quaternion.Euler(new Vector3(0, 0, 72));
			item.localPosition = new Vector3(0.03f, 0.045f, 0f);
			fox.TriggerJawOverride(1, 10);
		}

		if (item.name == "PaperAirplane")
		{
			item.localRotation = Quaternion.Euler(new Vector3(180, 95, -185));
			item.localPosition = new Vector3(0f, 0.085f, 0.01f);
			fox.TriggerJawOverride(1, 30);
		}

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

        grabbedItem.parent = this.transform;
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
