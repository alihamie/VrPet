using MalbersAnimations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpCatchTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
    public LayerMask myLayerMask;
	// Update is called once per frame
	void Update () {

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.up, out  hit, 500f , myLayerMask))
        {
            // offsetDistance = hit.distance;
            // Debug.DrawLine(transform.position, hit.point, Color.cyan);
            if (hit.collider.gameObject.tag == "DropArea")
            {

                Animal a = GetComponent<Animal>();
                a.Action = true;
                a.SetAction(21);
                this.enabled = false;
            }
        }
    }
}
