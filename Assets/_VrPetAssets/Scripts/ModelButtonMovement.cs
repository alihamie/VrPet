using System.Collections;
using UnityEngine;

public class ModelButtonMovement : MonoBehaviour
{
    private enum PossibleMovementDirection
    {
        PosXAxis = 0,
        PosYAxis = 1,
        PosZAxis = 2,
        NegXAxis = 3,
        NegYAxis = 4,
        NegZAxis = 5
    }
    [SerializeField]
    private PossibleMovementDirection movementDirections;
    private Vector3 chosenDirection
    {
        get
        {
            switch (movementDirections)
            {
                case PossibleMovementDirection.PosXAxis:
                    return transform.right;
                case PossibleMovementDirection.PosYAxis:
                    return transform.up;
                case PossibleMovementDirection.PosZAxis:
                    return transform.forward;
                case PossibleMovementDirection.NegXAxis:
                    return -transform.right;
                case PossibleMovementDirection.NegYAxis:
                    return -transform.up;
                case PossibleMovementDirection.NegZAxis:
                    return -transform.forward;
                default:
                    return transform.forward;
            }
        }
    }
    private Vector3 homePosition;

    public bool toggledOn;
    public float triggeredDistance = -.06f, clickDistance = -.09f;
#if UNITY_EDITOR
    public bool invokeToggle, invokeClick;

    private void Update()
    {
        if (invokeToggle)
        {
            ToggleButton();
            invokeToggle = false;
        }

        if (invokeClick)
        {
            ClickButton();
            invokeClick = false;
        }
    }
#endif

    private void Awake()
    {
        homePosition = transform.localPosition;
    }

    private void OnEnable()
    {
        //if (homePosition == new Vector3(0, 0, 0))
        //{
        //    homePosition = transform.localPosition;
        //}

        if (!toggledOn)
        {
            transform.localPosition = homePosition;
        }
        else
        {
            transform.localPosition = homePosition + (chosenDirection * triggeredDistance);
        }
    }

    public void ToggleButton()
    {
        if (!toggledOn)
        {
            StartCoroutine(ButtonMovement(clickDistance, triggeredDistance - clickDistance));
            toggledOn = true;
        }
        else
        {
            StartCoroutine(ButtonMovement(clickDistance - triggeredDistance, -clickDistance));
            toggledOn = false;
        }
    }

    public void ClickButton()
    {
        StartCoroutine(ButtonMovement(clickDistance, -clickDistance));
    }

    private IEnumerator ButtonMovement(float firstPoint, float secondPoint)
    {
        float timeFirst = .4f, timeSecond = .8f, timer = 0, previousTimer = 0;

        while (timer < timeSecond)
        {
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;

            if (previousTimer < timeFirst)
            {
                if (timer < timeFirst)
                {
                    transform.localPosition += ((chosenDirection * firstPoint * Time.deltaTime) / timeFirst);
                }
                else
                {
                    transform.localPosition += ((chosenDirection * firstPoint * (timeFirst - previousTimer)) / timeFirst);
                    transform.localPosition += ((chosenDirection * secondPoint * (Mathf.Min(timer, timeSecond) - timeFirst)) / (timeSecond - timeFirst));
                }
            }
            else if (previousTimer < timeSecond)
            {
                if (timer < timeSecond)
                {
                    transform.localPosition += (chosenDirection * secondPoint * Time.deltaTime / (timeSecond - timeFirst));
                }
                else
                {
                    transform.localPosition += (chosenDirection * secondPoint * (timeSecond - previousTimer) / (timeSecond - timeFirst));
                }
            }

            previousTimer = timer;
        }
    }
}
