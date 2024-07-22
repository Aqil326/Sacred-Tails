using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Timba.SacredTails.CharacterStyle
{
    /// <summary>
    ///     Allow player rotate character when is on Character Style Panel
    /// </summary>
    public class PlayerMouseRotator : MonoBehaviour
    {
        private Vector3 mPrevPos = Vector3.zero;
        private Vector3 mPosDelta = Vector3.zero;
        private Camera cam;
        [HideInInspector] public bool canRotateWithMouse = false;
        [HideInInspector] public bool isClickingOnArea = false;
        /*[HideInInspector]*/
        public Transform rotationTarget;
        [SerializeField] private float speedTurn;

        private float turn;

        private void Start()
        {
            cam = Camera.main;

        }

        public void IsClickingOnArea(bool isClicking)
        {
            isClickingOnArea = isClicking;
        }

        private void Update()
        {
            if (!canRotateWithMouse)
                return;
            if (!isClickingOnArea)
                return;

            turn = Input.GetAxis("Mouse X");

            if (Input.GetMouseButton(0))
                rotationTarget.Rotate(Vector3.up, -turn * speedTurn * Time.deltaTime, Space.World);
        }
    }
}