using UnityEngine;

public class AntennaBend : MonoBehaviour {
    public Transform[] antennaBones;
    public GameObject car;
    Rigidbody carRigidbody;
    Quaternion[] antennaRotation;
    public float bendPercent = .4f;
    Vector3 localVelocity;

    //So, we grab the default rotation to use as a sort of default pose for the antenna's bones.
    void Start () {
        carRigidbody = car.GetComponent<Rigidbody>();
        antennaRotation = new Quaternion[antennaBones.Length];
        for (int i = 0; i < antennaBones.Length; i++)
        {
            antennaRotation[i] = antennaBones[i].localRotation;
        }
    }

    // Then we take the car's velocity relative to local space and apply the normalized forward and sideways velocity to the antenna.
    void LateUpdate()
    {
        if (carRigidbody.velocity.magnitude != 0)
        {
            localVelocity = transform.InverseTransformDirection(carRigidbody.velocity);
            for (int i = 0; i < antennaBones.Length; i++)
            {
                antennaBones[i].localRotation = Quaternion.Slerp(antennaRotation[i], Quaternion.Euler(-localVelocity.normalized.z * 25f, 0, -localVelocity.normalized.x * 25f), localVelocity.magnitude * bendPercent);
            }
        }
    }
}
