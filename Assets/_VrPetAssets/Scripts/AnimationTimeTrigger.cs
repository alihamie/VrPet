using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTimeTrigger : StateMachineBehaviour {
    public enum PossibleAnimTriggers
    {
        Float = 0,
        Int = 1,
        BoolOrTrigger = 2,
        DirectAnimationCall = 3
    }

    public PossibleAnimTriggers currentAnimTrigger = PossibleAnimTriggers.Float;
    public string conditionOrAnimationName = "";
    public float floatToSet = 0;
    public int intToSet = 0;
    public bool boolToSet = false;

    public float timeLimit = 0;
    private float timer;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer = 0;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (timer > timeLimit)
        {
            switch (currentAnimTrigger)
            {
                case PossibleAnimTriggers.Float:
                    animator.SetFloat(conditionOrAnimationName, floatToSet);
                    break;
                case PossibleAnimTriggers.Int:
                    animator.SetInteger(conditionOrAnimationName, intToSet);
                    break;
                case PossibleAnimTriggers.BoolOrTrigger:
                    animator.SetBool(conditionOrAnimationName, boolToSet);
                    break;
                case PossibleAnimTriggers.DirectAnimationCall:
                    animator.Play(conditionOrAnimationName);
                    break;
                default:
                    animator.Play(conditionOrAnimationName);
                    break;
            }
        }

        timer += Time.deltaTime;
    }
}
