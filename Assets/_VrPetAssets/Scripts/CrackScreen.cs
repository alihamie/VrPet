using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrackScreen : MonoBehaviour
{
	public bool isEnabled = true;
	public GameObject tv;
	public GameObject crack;

	private void OnCollisionEnter(Collision collision)
	{
		if(isEnabled)
			crack.SetActive(true);
	}
}
