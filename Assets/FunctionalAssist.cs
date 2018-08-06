using UnityEngine;

public static class FunctionalAssist {

    public static float AngleOffAroundAxis(Vector3 angleTo, Vector3 angleFrom, Vector3 axis)
    {
        Vector3 right = Vector3.Cross(axis, angleFrom).normalized;
        angleFrom = Vector3.Cross(right, axis).normalized;
        float angleOff = Mathf.Atan2(Vector3.Dot(angleTo, right), Vector3.Dot(angleTo, angleFrom)) * Mathf.Rad2Deg;
        return angleOff;
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
