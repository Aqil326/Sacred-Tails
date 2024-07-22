using UnityEngine;
using UnityEngine.UI;

namespace Timba.SacredTails.UiHelpers
{
    [RequireComponent(typeof(Scrollbar))]
    public class KeepScrollBar : MonoBehaviour
    {
        //public GameObject scrollbar;

        private void LateUpdate()
        {
            setFixedHandleSize();
        }
        public void setFixedHandleSize()
        {
            GetComponent<Scrollbar>().size = 0;
        }
    }
}