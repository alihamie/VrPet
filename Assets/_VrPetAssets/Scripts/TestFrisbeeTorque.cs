using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFrisbeeTorque : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        float h = Input.GetAxis("Horizontal") * 50f * Time.deltaTime;
        float v = Input.GetAxis("Vertical") * 50f * Time.deltaTime;
        GetComponent<Rigidbody>().AddTorque(transform.up * 2f);
        GetComponent<Rigidbody>().AddForce(transform.forward * 4f);
    }
}
