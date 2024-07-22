using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

namespace Timba.Games.SacredTails.Lobby
{
    /// <summary>
    /// This class control the movement of the shinsei companion in the lobby
    /// </summary>
    public class ShinseiMovement : NetworkBehaviour
    {
        [SerializeField] Transform ownPlayer;
        [SerializeField] Animator animator;
        public NavMeshAgent navmeshAgent;
        // Start is called before the first frame update
        private void Start()
        {
            StartCoroutine(SeekPlayer());
            StartCoroutine(CheckAnimations());
        }
        void OnEnable()
        {
            StartCoroutine(SeekPlayer());
            StartCoroutine(CheckAnimations());
        }

        float currentAngularVelocity;
        IEnumerator CheckAnimations()
        {
            while (true)
            {
                Vector3 s = navmeshAgent.transform.InverseTransformDirection(navmeshAgent.velocity).normalized;
                currentAngularVelocity = Mathf.Lerp(currentAngularVelocity, s.x, Time.deltaTime);

                if (navmeshAgent.velocity.sqrMagnitude > 1f || currentAngularVelocity > .1f)
                {
                    if (navmeshAgent.velocity.sqrMagnitude > 4.5f * 4.5f)
                    {
                        animator.SetBool("Run", true);
                        animator.SetBool("Walk", false);
                    }
                    else
                    {
                        animator.SetBool("Run", false);
                        animator.SetBool("Walk", true);
                    }
                }
                else
                {
                    animator.SetBool("Run", false);
                    animator.SetBool("Walk", false);
                }
                yield return null;
            }
        }

        IEnumerator SeekPlayer()
        {
            if (ownPlayer == null)
                yield break;
            while (true)
            {
                yield return new WaitForSeconds(.2f);
                MoveAt(ownPlayer.transform.position);
            }
        }

        public void SetOwner(Transform targetOwner)
        {
            ownPlayer = targetOwner;
        }

        public void MoveAt(Vector3 targetPosition)
        {
            if (navmeshAgent.destination != targetPosition & navmeshAgent.isOnNavMesh)
                navmeshAgent.destination = targetPosition;
        }
    }
}