using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Timba.SacredTails.Database;
using Sirenix.OdinInspector;
using System;
using Timba.Patterns.ServiceLocator;
using Timba.Games.CharacterFactory;

namespace Timba.SacredTails.Photoboot
{
    /// <summary>
    /// This allow take pictures to the Shinsei, using Camera and Coroutines 
    /// </summary>
    public class ShinseiWardrobe : MonoBehaviour, IIconGeneration
    {
        [SerializeField] private CharacterSlot shinseiModel;
        [SerializeField] private Camera mainCam;
        public Sprite generatedSprite;
        private RenderTexture renderTex;


        [Button("generate icons")]
        public void GenerateShinseiIcons(List<Shinsei> shinseiParty, Action callback = null)
        {
            renderTex = mainCam.targetTexture;
            StartCoroutine(ChangeShinsei(shinseiParty, callback));
        }

        public List<Sprite> GetGeneratedIcons(List<Shinsei> shinseiParty)
        {
            List<Sprite> shinseiIcons = new List<Sprite>(); 
            renderTex = mainCam.targetTexture;
            StartCoroutine(ChangeShinsei(shinseiParty));
            foreach (var shinsei in shinseiParty)
                shinseiIcons.Add(shinsei.shinseiIcon);

            return shinseiIcons;
        }

        public List<Sprite> GetGeneratedSequence(List<Shinsei> shinseiParty, int targetFrames)
        {
            List<Sprite> shinseiSequence = new List<Sprite>();
            renderTex = mainCam.targetTexture;
            StartCoroutine(CaptureFrames(shinseiParty, shinseiSequence, targetFrames));

            return shinseiSequence;
        }

        IEnumerator CaptureFrames(List<Shinsei> shinseiParty, List<Sprite> shinseiIcons, int targetFrames)
        {
            foreach (var slot in shinseiParty)
            {
                shinseiModel.SetCharacterCode(ServiceLocator.Instance.GetService<IDatabase>().GetShinseiStructure(slot.ShinseiDna), true);
                shinseiModel.UpdateVisual();
                while (targetFrames > 0)
                {
                    yield return new WaitForEndOfFrame();
                    slot.shinseiIcon = photograph();
                    shinseiIcons.Add(slot.shinseiIcon);
                    targetFrames--;
                }
            }
        }

        IEnumerator ChangeShinsei(List<Shinsei> shinseiParty, Action callback = null)
        {

            foreach (var slot in shinseiParty)
            {
                shinseiModel.SetCharacterCode(ServiceLocator.Instance.GetService<IDatabase>().GetShinseiStructure(slot.ShinseiDna), true);
                shinseiModel.UpdateVisual();
                yield return new WaitForEndOfFrame();
                slot.shinseiIcon = photograph();
                slot.shinseiIcon.name = slot.shinseiName;
            }
            callback?.Invoke();
        }

        [Button("Photo")]
        private Sprite photograph()
        {
            var tex = new Texture2D(renderTex.width, renderTex.height, TextureFormat.ARGB32, false, true);
            RenderTexture.active = renderTex;
            tex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            tex.Apply();
            generatedSprite = TexToSprite(tex);
            return generatedSprite;
        }

        public Sprite TexToSprite(Texture2D tex)
        {
            Sprite iconSprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            return iconSprite;
        }

        public bool IsReady()
        {
            return true;
        }
    }
}