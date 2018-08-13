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

		foreach(Collider c in hits)
		{
			Rigidbody r = c.GetComponentInParent<Rigidbody>();
			if (r != null && !c.isTrigger)
			{
				Vector3 distance = c.transform.position - transform.position;
				float force = Mathf.Clamp(Mathf.Pow(distance.sqrMagnitude, -1), 0, maxInverseDistance) * mass * r.mass;
				r.AddForce(-distance.normalized * force, ForceMode.Acceleration); //let the bass cannon kick it!
			}
		}
	}
}
