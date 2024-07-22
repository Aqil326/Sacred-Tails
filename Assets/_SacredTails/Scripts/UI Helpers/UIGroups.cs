using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Timba.SacredTails.UiHelpers
{
    public class UIGroups : MonoBehaviour
    {
        public List<UiGroup> groups = new List<UiGroup>();
        public static UIGroups instance;
        public string lastActivate;

        private void Awake()
        {
            instance = this;
        }

        private void OnDestroy()
        {
            instance = FindObjectOfType<UIGroups>();
        }

        public void ShowOnlyThisGroup(string key)
        {
            int trueIndex = -1;
            groups.ForEach(a =>
            {
                if (a.key != key)
                    foreach (var canvas in a.canvasOfGroup)
                    {
                        canvas.alpha = 0;
                        canvas.blocksRaycasts = false;
                    }
                else
                    trueIndex = groups.IndexOf(a);

            });
            if (trueIndex != -1)
                foreach (var canvas in groups[trueIndex].canvasOfGroup)
                {
                    canvas.alpha = 1;
                    canvas.blocksRaycasts = true;
                }
            lastActivate = key;
        }

        public void ShowOnlyThisGroupWithDeactivating(string key)
        {
            int trueIndex = -1;
            groups.ForEach(a =>
            {
                if (a.key != key)
                    foreach (var canvas in a.canvasOfGroup)
                    {
                        canvas.alpha = 0;
                        canvas.gameObject.SetActive(false);
                        canvas.blocksRaycasts = false;
                    }
                else
                    trueIndex = groups.IndexOf(a);

            });
            if (trueIndex != -1)
                foreach (var canvas in groups[trueIndex].canvasOfGroup)
                {
                    canvas.alpha = 1;
                    canvas.gameObject.SetActive(true);
                    canvas.blocksRaycasts = true;
                }
            lastActivate = key;
        }

        public void NotifyDynamicPanel(CanvasGroup targetCanvas, string key)
        {
            List<UiGroup> UiGroups = groups.Where(a => a.key == key).ToList();
            if (UiGroups.Any())
            {
                groups[UiGroups.IndexOf(UiGroups[0])].canvasOfGroup.Add(targetCanvas);
            }
        }

        [System.Serializable]
        public class UiGroup
        {
            public string key;
            public List<CanvasGroup> canvasOfGroup = new List<CanvasGroup>();
        }
    }
}