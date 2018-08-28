using UnityEngine;

public class Explode : MonoBehaviour
{
	public int fuse = 3;
	public int radius = 10;
	public int force = 100;
	public bool exploded = false;

    float startTime;
    private ParticleSystem particles;

	void Start()
	{
        particles = GetComponent<ParticleSystem>();
        startTime = Time.time;
	}

	void Update()
	{
		if (Time.time - startTime > fuse && !exploded) //if the time has come and we haven't come yet...
		{
			exploded = true;
            if (particles)
            {
                particles.Play(true);
            }
			Collider[] hits = Physics.OverlapSphere(transform.position, radius); //target spotted

			foreach(Collider coll in hits)
			{
				Rigidbody rigid = coll.GetComponent<Rigidbody>();
				if (rigid != null)
                {
                    rigid.AddExplosionForce(force, transform.position, radius); //let the bass cannon kick it!
                }
			}
		}
	}
}
