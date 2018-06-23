using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionSound : MonoBehaviour {
    AudioSource hitSoundSource;
    public AudioClip hitSound;
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
            hitSoundSource.PlayOneShot(hitSound, volume);
        }
    }
}
