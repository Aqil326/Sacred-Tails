using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Timba.SacredTails.UiHelpers
{
    public class WoldPositionateUiElement : MonoBehaviour
    {
        public GameObject uiElement;
        bool IsRegistered = false;
        [SerializeField] bool DisableInGameplay = true;

        private void LateUpdate()
        {
            if (!IsRegistered)
                if (WoldPositionUIHandler.instance != null)
                {
                    WoldPositionUIHandler.instance.RegisterUiElement(this);
                    IsRegistered = true;
                }
        }

        private void OnDestroy()
        {
            if (WoldPositionUIHandler.instance == null)
                return;

            WoldPositionUIHandler.instance.UnregisterUiElement(this);
        }
    }
}