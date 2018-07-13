using UnityEngine;

public class CollisionSound : MonoBehaviour {
    AudioSource hitSoundSource;
    public AudioClip hitSoundSoft, hitSoundHard;
    public float velocityThreshold = 1f;
    public float volume = .4f;

    void Start()
    {
        hitSoundSource = transform.GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision hit)
    {
        if (hit.relativeVelocity.magnitude > velocityThreshold)
        {
            if (hit.gameObject.tag == "Soft" && hitSoundSoft != null)
            {
                hitSoundSource.PlayOneShot(hitSoundSoft, volume);
            }
            else
            {
                hitSoundSource.PlayOneShot(hitSoundHard, volume);
            }
        }
    }
}
