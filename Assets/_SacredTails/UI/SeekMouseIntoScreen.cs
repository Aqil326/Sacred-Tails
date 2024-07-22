using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Timba.SacredTails.UiHelpers
{
    public class SeekMouseIntoScreen : MonoBehaviour
    {
        public int posOffsetX = 0, posOffsetY = 0;
        public int offsetX = 255, offsetY = 255;

        public void Enable(bool state)
        {
            gameObject.SetActive(state);
        }

        void LateUpdate()
        {
            Vector3 position = Input.mousePosition + new Vector3(posOffsetX, posOffsetY);
            position = new Vector2(Mathf.Clamp(position.x, offsetX, Screen.width - offsetX), Mathf.Clamp(position.y, offsetY, Screen.height - offsetY));
            transform.position = position;
        }
    }
}