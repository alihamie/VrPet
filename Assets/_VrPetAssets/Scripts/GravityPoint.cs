using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityPoint : MonoBehaviour
{

	public int radius = 10;
	public int mass = 100;
	public float maxInverseDistance;

	void FixedUpdate()
	{
		Collider[] hits = Physics.OverlapSphere(transform.position, radius); //target spotted

		foreach(Collider coll in hits)
		{
			Rigidbody rigid = coll.GetComponentInParent<Rigidbody>();
			if (rigid != null && !coll.isTrigger)
			{
				Vector3 distance = coll.transform.position - transform.position;
				float force = Mathf.Clamp(Mathf.Pow(distance.sqrMagnitude, -1), 0, maxInverseDistance) * mass * rigid.mass;
				rigid.AddForce(-distance.normalized * force, ForceMode.Acceleration); //let the bass cannon kick it!
			}
		}
	}
}
