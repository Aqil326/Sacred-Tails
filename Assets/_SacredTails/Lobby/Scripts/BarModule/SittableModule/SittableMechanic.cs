using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Timba.SacredTails.Interaction
{
    /// <summary>
    ///     Allow to player see their character sitdown in chairs on the bar
    /// </summary>
    public class SittableMechanic : MonoBehaviour
    {
        ThirdPersonController thirdPersonController = null;
        [SerializeField] GameObject chair;
        private void OnTriggerEnter(Collider other)
        {
            if (thirdPersonController != null)
                return;
            if (other.CompareTag("OtherPlayer") || other.CompareTag("Player"))
            {
                if (thirdPersonController = other.GetComponent<ThirdPersonController>())
                {
                    foreach (var animation in thirdPersonController.animator)
                        animation.SetBool("Sit", true);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (thirdPersonController == null)
                return;
            if (other.CompareTag("OtherPlayer") || other.CompareTag("Player"))
            {
                if (thirdPersonController == other.GetComponent<ThirdPersonController>())
                {
                    foreach (var animation in thirdPersonController.animator)
                        animation.SetBool("Sit", false);
                    thirdPersonController = null;
                }
            }
        }
        // Start is called before the first frame update
        void Start()
        {
            originalPosition = chair.transform.position;
            originalRotation = chair.transform.rotation;
        }
        [SerializeField] Vector3 offset;
        [SerializeField] Vector3 originalPosition;
        [SerializeField] Quaternion originalRotation;
        // Update is called once per frame
        void Update()
        {
            if (thirdPersonController == null)
            {
                chair.transform.position = originalPosition;
                chair.transform.rotation = originalRotation;
                return;
            }
            Vector3 targetDirection = transform.position + thirdPersonController.transform.GetChild(1).transform.forward;
            chair.transform.LookAt(new Vector3(targetDirection.x, chair.transform.position.y, targetDirection.z));
            chair.transform.position = new Vector3(thirdPersonController.transform.position.x, 0, thirdPersonController.transform.position.z);
            chair.transform.position += chair.transform.forward * offset.z;
            chair.transform.position += chair.transform.right * offset.x;
        }
    }
}