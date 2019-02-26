using UnityEngine;
using UnityEngine.SceneManagement;

public class TabletFunctionality : MonoBehaviour
{
    TabletScreenManager screenManager;
    public Transform car;
    private Vector3 carInitialPosition;
    public bool isCarActive;
    public FoxTargetManager targetManager;

    void Start()
    {
        screenManager = GetComponent<TabletScreenManager>();
        carInitialPosition = car.transform.position;
        isCarActive = false;
    }

    public void ToggleCarbutton()
    {
        car.gameObject.SetActive(!car.gameObject.activeSelf);
        car.transform.position = carInitialPosition;
        isCarActive = car.gameObject.activeSelf;
        if (isCarActive)
        {
            chaseCar();
        }
    }

    public void ResetScene()
    {
        SceneManager.LoadScene("RobertInteraction1");
    }

    private void chaseCar()
    {
        targetManager.GoToFetchItem(car);
    }
}
