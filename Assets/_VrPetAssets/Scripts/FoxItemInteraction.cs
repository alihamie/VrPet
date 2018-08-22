using EasyInputVR.StandardControllers;
using System.Collections;
using MalbersAnimations;
using UnityEngine;

public class FoxItemInteraction : MonoBehaviour
{
    public Transform jaw;
    public AnimalAIControl aiControl;
    public Transform player;

    private Transform grabbedItem;
    private Collider[] itemColliders;

    public void GrabItem()
    {
        Transform item = aiControl.GetClosestGrabableItem();

        if (!item)
        {
            return;
        }

        if (grabbedItem && item == grabbedItem)
        {
            return;
        }

        if (grabbedItem)
        {
            DropItem();
        }

        itemColliders = item.GetComponentsInChildren<Collider>();
        item.parent = jaw;
        item.localPosition = Vector3.zero;
        grabbedItem = item;
        item.gameObject.GetComponent<Rigidbody>().isKinematic = true;

		if(item.name == "ShinyRedBall")
		{
			item.localPosition = new Vector3(0.06f, 0.05f, 0f);
			aiControl.TriggerJawOverride(1, 0);
		}

		if (item.name == "Frisbee")
		{
			item.localRotation = Quaternion.Euler(new Vector3(0, 180, -76));
			item.localPosition = new Vector3(0.05f, 0.07f, 0f);
			aiControl.TriggerJawOverride(1, 30);
		}

		if (item.name == "RemoteControl")
		{
			item.localRotation = Quaternion.Euler(new Vector3(0, 0, 72));
			item.localPosition = new Vector3(0.03f, 0.045f, 0f);
			aiControl.TriggerJawOverride(1, 10);
		}

		if (item.name == "PaperAirplane")
		{
			item.localRotation = Quaternion.Euler(new Vector3(180, 95, -185));
			item.localPosition = new Vector3(0f, 0.085f, 0.01f);
			aiControl.TriggerJawOverride(1, 30);
		}

		foreach (Collider col in itemColliders)
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
        if (!grabbedItem)
        {
            return;
        }

        grabbedItem.parent = transform;
        grabbedItem.GetComponent<Rigidbody>().isKinematic = false;
        aiControl.TriggerJawOverride(0);
        StartCoroutine(DelayedPhysics());

        foreach (Collider col in itemColliders)
        {
            col.enabled = true;
        }

        if (aiControl)
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

        itemColliders = null;
        grabbedItem = null;
    }

    IEnumerator DelayedPhysics()
    {
        Physics.IgnoreLayerCollision(8, 20);
        yield return new WaitForSeconds(1.5f);
        Physics.IgnoreLayerCollision(8, 20, false);
    }
}
