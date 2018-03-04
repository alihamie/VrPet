using UnityEngine;
using UnityEngine.SceneManagement;

public class TabletFunctionality : MonoBehaviour {


    TabletScreenManager screenManager;
    public Transform car;
    private Vector3 carInitialPosition;
    public bool isCarActive;
    public FoxTargetManager targetManager;
    // Use this for initialization
    void Start () {
        screenManager = this.GetComponent<TabletScreenManager>();
        carInitialPosition = car.transform.position;
        isCarActive = false;
	}
	

    public void ToggleCarbutton()
    {
        this.car.gameObject.SetActive(!this.car.gameObject.activeSelf);
        this.car.transform.position =  carInitialPosition;
        isCarActive = this.car.gameObject.activeSelf;
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
