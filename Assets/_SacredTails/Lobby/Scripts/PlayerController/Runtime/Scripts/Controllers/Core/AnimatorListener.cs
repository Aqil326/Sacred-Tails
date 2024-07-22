using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AnimatorListener :  NetworkBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody RigidBody;
    int blendHash;
    // Start is called before the first frame update
    void Start()
    {
        blendHash = Animator.StringToHash("Velocity");
    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            animator.SetFloat(blendHash, RigidBody.velocity.magnitude);
        }
    }
}
