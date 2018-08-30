using UnityEngine;

public static class FunctionalAssist {

    public static float AngleOffAroundAxis(Vector3 angleTo, Vector3 angleFrom, Vector3 axis)
    {
        Vector3 right = Vector3.Cross(axis, angleFrom).normalized;
        angleFrom = Vector3.Cross(right, axis).normalized;
        float angleOff = Mathf.Atan2(Vector3.Dot(angleTo, right), Vector3.Dot(angleTo, angleFrom)) * Mathf.Rad2Deg;
        return angleOff;
    }

    public static Vector2 IrregularOvalBounds(Vector2 oval, float upBound, float downBound, float rightBound, float leftBound)
    {
        float angleDeclension = Vector2.SignedAngle(new Vector2(0, 1f), oval) / 90f;
        float ovalBound = 0;

        if (Mathf.Sign(angleDeclension) == 1)
        {
            if (angleDeclension < 1f)
            {
                ovalBound = Mathf.Lerp(upBound, rightBound, angleDeclension);
            }
            else
            {
                ovalBound = Mathf.Lerp(rightBound, -downBound, angleDeclension - 1f);
            }
        }
        else if (Mathf.Sign(angleDeclension) == -1)
        {
            if (angleDeclension < -1f)
            {
                ovalBound = Mathf.Lerp(upBound, -leftBound, -angleDeclension);
            }
            else
            {
                ovalBound = Mathf.Lerp(-leftBound, -downBound, -angleDeclension - 1f);
            }
        }

        if ((ovalBound * ovalBound) < oval.SqrMagnitude())
        {
            oval = oval * (Mathf.Abs(ovalBound) / oval.magnitude);
        }

        return oval;
    }

    //public static Quaternion RelativeRotation(Vector3 angleTo, Vector3 angleFrom, Vector3 axis1, Vector3 axis2, Quaternion startRotation)
    //{
    //    float tilt = AngleOffAroundAxis(angleTo, angleFrom, axis1);
    //    float pan = AngleOffAroundAxis(angleTo, angleFrom, axis2);

    //    Quaternion modifiedRotation = startRotation;

    //    modifiedRotation *= Quaternion.AngleAxis(tilt, axis1);
    //    modifiedRotation *= Quaternion.AngleAxis(pan, axis2);

    //    Debug.Log(startRotation);
    //    Debug.Log(modifiedRotation);

    //    return modifiedRotation;
    //}
}
