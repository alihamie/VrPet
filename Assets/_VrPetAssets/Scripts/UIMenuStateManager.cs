﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMenuStateManager : MonoBehaviour {
    public int activeChildNumber = -1;
    [SerializeField]
    private int defaultState = 0;

    public void ChangeState(int stateNumber)
    {
        // If a child is asked for that doesn't exist, it just disables all the children.
        for (int i = 0; i < transform.childCount; i++)
        {
            if (i == stateNumber)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        // This here's just for bookkeeping purposes.
        if (stateNumber < transform.childCount && stateNumber >= 0)
        {
            activeChildNumber = stateNumber;
        }
        else
        {
            activeChildNumber = defaultState;
        }
    }
}
