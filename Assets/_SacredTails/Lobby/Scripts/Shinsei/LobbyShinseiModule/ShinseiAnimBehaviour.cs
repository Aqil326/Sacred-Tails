using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShinseiAnimBehaviour : StateMachineBehaviour
{

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var random = Random.value;
        if (random < 0.1)
            animator.SetFloat("Random", 3);
        else if (random < 0.2)
            animator.SetFloat("Random", 2);
        else if (random < 0.3)
            animator.SetFloat("Random", 1);
        else
            animator.SetFloat("Random", 0);
    }

}
