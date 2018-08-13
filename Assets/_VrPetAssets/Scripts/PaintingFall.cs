using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintingFall : MonoBehaviour
{
	void OnTriggerEnter(Collider coll)
	{
		GetComponent<Rigidbody>().isKinematic = false;
		GetComponent<BoxCollider>().isTrigger = false;
	}
}
