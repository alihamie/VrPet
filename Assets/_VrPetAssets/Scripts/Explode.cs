using UnityEngine;

public class Explode : MonoBehaviour
{
	public int fuse = 3;
	public int radius = 10;
	public int force = 100;
	public bool exploded = false;
	float startTime;

	void Start()
	{
		startTime = Time.time;
	}

	void FixedUpdate()
	{
		if (Time.time - startTime > fuse && !exploded) //if the time has come and we haven't come yet...
		{
			exploded = true;
			GetComponent<ParticleSystem>().Play(true);
			Collider[] hits = Physics.OverlapSphere(transform.position, radius); //target spotted

			foreach(Collider c in hits)
			{
				Rigidbody r = c.GetComponent<Rigidbody>();
				if (r != null)
					r.AddExplosionForce(force, transform.position, radius); //let the bass cannon kick it!
			}
		}
	}
}
