using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelativeHeadRotate : MonoBehaviour
{
    public Transform lookTarget, player;
    private float maxTilt = 80f, minTilt = -20f;
    private float maxPan = 60f, minPan = -60f;
    public float jawOffset = 0f;

    private Quaternion defaultJawRotation;
    private Transform head, jaw;
    private float firstAngle, secondAngle;
    private Quaternion lastRotation;
    private Vector3 lookDirection;

    private MalbersAnimations.AnimalAIControl animalAI;

    public enum TARGETS
    {
        NONE = 0,
        PLAYER = 1,
        ITEM = 2,
    }

    void OnEnable()
    {
        animalAI = GetComponent<MalbersAnimations.AnimalAIControl>();
        head = animalAI.animalHead.transform;
        jaw = head.GetChild(2);
        defaultJawRotation = jaw.rotation;

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
        Quaternion normalRotation = head.localRotation;

        //lookTarget = new Vector3(0, 1f, 0);
        //lookTarget = animalAI.target;

        if (lookTarget != null && !animalAI.isWandering)
        {
            lookDirection = head.InverseTransformPoint(lookTarget.position);

            if (lookDirection.sqrMagnitude > .25f)
            {
                // First casualty of adapting this to different animals is going to be the head and neck orientations. For other skeletons, these -WILL- be different. Ideally the axes will actually align with what they say on the tin...
                // Also, the head's up direction is -Vector3.right. Seriously, this f---ing armature... Though, worth nothing that both the up and right direction have to be negative for all this to work.
                firstAngle = FunctionalAssist.AngleOffAroundAxis(lookDirection, Vector3.up, Vector3.right);
                lookDirection = Quaternion.AngleAxis(-firstAngle, Vector3.right) * lookDirection;
                secondAngle = FunctionalAssist.AngleOffAroundAxis(lookDirection, Vector3.up, Vector3.forward);

                Vector2 boundedLook = FunctionalAssist.IrregularOvalBounds(new Vector2(firstAngle, secondAngle), maxTilt, minTilt, maxPan, minPan);

                normalRotation *= Quaternion.AngleAxis(boundedLook.x, Vector3.right);
                normalRotation *= Quaternion.AngleAxis(boundedLook.y, Vector3.forward);
            }
        }

        normalRotation = Quaternion.Lerp(normalRotation, lastRotation, .9f);

        if (jawOffset != 0)
        {
            jaw.rotation = defaultJawRotation;
            jaw.Rotate(new Vector3(0, 0, jawOffset), Space.Self);
        }

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
