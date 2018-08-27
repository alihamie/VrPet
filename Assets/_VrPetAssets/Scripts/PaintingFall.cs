using UnityEngine;

public class PaintingFall : MonoBehaviour
{
    public float fallTriggerThreshold = 1.5f;

	void OnTriggerEnter(Collider coll)
	{
        Rigidbody colliderRigidboby = GetComponent<Rigidbody>();
        if (colliderRigidboby.velocity.sqrMagnitude > fallTriggerThreshold * fallTriggerThreshold)
        {
            GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<BoxCollider>().isTrigger = false;
        }
	}
}
