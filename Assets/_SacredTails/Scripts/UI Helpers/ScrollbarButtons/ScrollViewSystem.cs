using UnityEngine;
using UnityEngine.UI;

namespace Timba.SacredTails.UiHelpers
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollViewSystem : MonoBehaviour
    {
        private ScrollRect scrollRect;
        [SerializeField] private ScrollButton leftButton;
        [SerializeField] private ScrollButton rightButton;

        [SerializeField] private float speedScroll;

        public void Start()
        {
            scrollRect = GetComponent<ScrollRect>();
        }
        private void Update()
        {
            if (leftButton.isDown)
                LeftScroll();
            if (rightButton.isDown)
                RightScroll();
        }

        public void LeftScroll()
        {
            bool isNewValueOnLimits = scrollRect.horizontalNormalizedPosition - speedScroll >= 0;
            float valueOfScroll = isNewValueOnLimits ? scrollRect.horizontalNormalizedPosition - speedScroll : 0;
            scrollRect.horizontalNormalizedPosition = valueOfScroll;
        }
        public void RightScroll()
        {
            bool isNewValueOnLimits = scrollRect.horizontalNormalizedPosition + speedScroll <= 1;
            float valueOfScroll = isNewValueOnLimits ? scrollRect.horizontalNormalizedPosition + speedScroll : 1;
            scrollRect.horizontalNormalizedPosition = valueOfScroll;
        }
    }
}