using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using SimpleJSON;

public class WebDrop : MonoBehaviour
{
    public bool goJoe, goJim;
    public string url = "https://www.twitch.tv/twitchpresents";
    public FoxTargetManager soAndSo;
    public Transform forceTarget;
    Animator foxAnim;
    public GameObject fox;
    MalbersAnimations.Animal foxMind;
    MalbersAnimations.AnimalAIControl foxAI;

    // This is navmesh stuff.
    private NavMeshPath path;
    private float elapsed = 0.0f;
    public Vector3 rigidmanSpeed;

    void Start()
    {
        foxMind = fox.GetComponent<MalbersAnimations.Animal>();
        foxAnim = fox.GetComponent<Animator>();
        foxAI = fox.GetComponent<MalbersAnimations.AnimalAIControl>();

        // Navmesh stuff...
        path = new NavMeshPath();
    }

    // Update is called once per frame
    void Update()
    {
        Rigidbody rigidman = forceTarget.gameObject.GetComponent<Rigidbody>();

        if (rigidman)
        {
            rigidmanSpeed = rigidman.velocity;
        }
        else
        {
            rigidmanSpeed = Vector3.zero;
        }

        if (goJoe)
        {
            //StartCoroutine(GrabDrop());
            soAndSo.GoToFetchItem(forceTarget);
            //foxMind.SetJump();
            goJoe = false;
        }

        //Debug.Log(foxAnim.GetCurrentAnimatorStateInfo(0).ToString());

        if (goJim)
        {
            Debug.Log(foxAI.Agent.enabled.ToString());
            goJim = false;
        }

        // Update the way to the goal every second.
        elapsed += Time.deltaTime;
        if (elapsed > 1.0f)
        {
            elapsed -= 1.0f;
            NavMesh.CalculatePath(fox.transform.position, forceTarget.position, NavMesh.AllAreas, path);
        }
        for (int i = 0; i < path.corners.Length - 1; i++)
            Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
    }

    IEnumerator GrabDrop()
    {
        Debug.Log("So far so good.");
        WWW www = new WWW(url);
        yield return www;
        Debug.Log(www.text);
        var requestData = JSON.Parse(www.text);
        Debug.Log(requestData);
        Debug.Log(requestData.ToString());
    }
}
