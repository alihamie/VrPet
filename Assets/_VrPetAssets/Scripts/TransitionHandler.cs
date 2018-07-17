using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations;
public class TransitionHandler : MonoBehaviour, IAnimatorListener {

    private Animator animator;
    public delegate void FadeOutExit();
    public FadeOutExit fadeOutExit;

	void Start () {
        animator = this.GetComponent<Animator>();
	}

    public void StartFadetransition()
    {
        animator.SetBool("FadeTransition", true);
    }

    public void FadeOutExitTime()
    {
        if (fadeOutExit != null)
        {
            fadeOutExit();
        }
    }

    public virtual void OnAnimatorBehaviourMessage(string message, object value)
    {
        this.InvokeWithParams(message, value);
    }
}
