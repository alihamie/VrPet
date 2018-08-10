using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelativeHeadRotate : MonoBehaviour
{
    public Transform player;
    public float maxTilt = 45f, minTilt = -45f;
    public float maxPan = 60f, minPan = -20f;

    private Transform lookTarget, head;
    private float firstAngle, secondAngle;
    private Quaternion normalRotation, lastRotation;
    private Vector3 lookDirection;

    private MalbersAnimations.AnimalAIControl animalAI;

    public enum TARGETS
    {
        NONE = 0,
        PLAYER = 1,
        ITEM = 2,
    }

    //private void Start()
    //{

    //}

    void OnEnable()
    {
        animalAI = GetComponent<MalbersAnimations.AnimalAIControl>();
        head = animalAI.animalHead.transform;

        lastRotation = head.localRotation;
        StartCoroutine("RunLateFixedUpdate");
    }

    void OnDisable()
    {
        StopCoroutine("RunLateFixedUpdate");
    }

    IEnumerator RunLateFixedUpdate()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();
            LateFixedUpdate();
        }
    }

    // I'm little bit torn up about this because it's a hacky way to do something that should be possible to just do in some variation of LateUpdate and it might contribute disproportionately to the general render time since it needs to happen after LateUpdate. Maybe it'll be okay if this is the only thing that does that, though...
    void LateFixedUpdate()
    {
        normalRotation = head.localRotation;

        lookTarget = animalAI.target;

        if (lookTarget != null && !animalAI.isWandering)
        {
            lookDirection = head.InverseTransformPoint(lookTarget.position).normalized;

            if (lookDirection.magnitude > .8f)
            {
                // First casualty of adapting this to different animals is going to be the head and neck orientations. For other skeletons, these -WILL- be different. Ideally the axes will actually align with what they say on the tin...
                // Also, the head's up direction is -Vector3.right. Seriously, this f---ing armature...
                firstAngle = FunctionalAssist.AngleOffAroundAxis(lookDirection, Vector3.up, Vector3.right);
                lookDirection = Quaternion.AngleAxis(-firstAngle, Vector3.right) * lookDirection;
                secondAngle = FunctionalAssist.AngleOffAroundAxis(lookDirection, Vector3.up, -Vector3.forward);

                normalRotation *= Quaternion.AngleAxis(Mathf.Clamp(firstAngle, minTilt, maxTilt), Vector3.right);
                normalRotation *= Quaternion.AngleAxis(Mathf.Clamp(secondAngle, -maxPan, -minPan), -Vector3.forward);
            }
        }

        normalRotation = Quaternion.Lerp(normalRotation, lastRotation, .9f);

        lastRotation = head.localRotation = normalRotation;
    }

    public void ChangeTarget(TARGETS newTarget)
    {
        if (newTarget == TARGETS.NONE)
        {
            lookTarget = null;
        }
        else if (newTarget == TARGETS.PLAYER)
        {
            lookTarget = player;
        }
        else if (newTarget == TARGETS.ITEM)
        {
            lookTarget = animalAI.target;
        }
    }
}
