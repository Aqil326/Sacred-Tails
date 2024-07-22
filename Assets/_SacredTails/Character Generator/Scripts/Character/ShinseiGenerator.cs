using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Timba.SacredTails.Database;
using Sirenix.OdinInspector;
using System.IO;
using UnityEngine.UI;
using Timba.SacredTails.UiHelpers;
using System.Threading.Tasks;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.Photoboot;

namespace Timba.Games.CharacterFactory
{
    public class ShinseiGenerator : MonoBehaviour
    {
        public int shinseisToGenerate;
        public SpriteRenderer iconBackground;
        public ShinseiWardrobe photobooth;
        public Animator posedShinsei;
        public List<Shinsei> generatedShinseis;
        [PreviewField(300, ObjectFieldAlignment.Center)]
        public List<Sprite> shinseiIcons;
        public List<string> shinseiJsons;

        public int lastIndex = 0;

        [Button("Generate")]
        public void GetShinseiNFTData(string shinseiDNA, int shinseiIndex)
        {
            Shinsei generatedShinsei = new Shinsei()
            {
                shinseiName = shinseiIndex.ToString("D5"),
                generation = "Genesis",
                ShinseiDna = shinseiDNA,
                shinseiType = ServiceLocator.Instance.GetService<IDatabase>().ObtainShinseiType(shinseiDNA),
                shinseiRarity = ServiceLocator.Instance.GetService<IDatabase>().ObtainShinseiRarity(shinseiDNA),
                ShinseiOriginalStats = ServiceLocator.Instance.GetService<IDatabase>().GetShinseiStats(shinseiDNA),
            };
            generatedShinseis.Add(generatedShinsei);
            iconBackground.sprite = ServiceLocator.Instance.GetService<IUIHelpable>().AssignIcon(generatedShinsei.shinseiType).backgroundSprite;
            posedShinsei.SetInteger("Tier", (int)generatedShinsei.shinseiRarity);
            posedShinsei.SetInteger("Type", (int)generatedShinsei.shinseiType);

            StartCoroutine(WaitToGeneratePicture(generatedShinseis));
        }

        IEnumerator WaitToGeneratePicture(List<Shinsei> shinseis)
        {
            shinseiIcons = photobooth.GetGeneratedIcons(shinseis);
            yield return new WaitForEndOfFrame();
            SaveGeneratedData();
        }

        [Button("Generate")]

        public void GenerateShinseis(RarityType desiredTier = RarityType.Common, CharacterType desiredType = CharacterType.Sky)
        {
            if (desiredTier == RarityType.Common && desiredType == CharacterType.Celestial)
                return;

            int cacheIndex = 0;
            for (int i = lastIndex; i < lastIndex + shinseisToGenerate; i++)
            {
                string generatedShinseiDna = ServiceLocator.Instance.GetService<IDatabase>().GetRandomShinsei();
                Shinsei generatedShinsei = new Shinsei()
                {
                    shinseiName = i.ToString("D5"),
                    generation = "Genesis",
                    ShinseiDna = generatedShinseiDna,
                    shinseiType = ServiceLocator.Instance.GetService<IDatabase>().ObtainShinseiType(generatedShinseiDna),
                    shinseiRarity = ServiceLocator.Instance.GetService<IDatabase>().ObtainShinseiRarity(generatedShinseiDna),
                    ShinseiOriginalStats = ServiceLocator.Instance.GetService<IDatabase>().GetShinseiStats(generatedShinseiDna),
                };
                iconBackground.sprite = ServiceLocator.Instance.GetService<IUIHelpable>().AssignIcon(generatedShinsei.shinseiType).backgroundSprite;
                posedShinsei.SetInteger("Tier", (int)generatedShinsei.shinseiRarity);
                posedShinsei.SetInteger("Type", (int)generatedShinsei.shinseiType);

                if (!generatedShinseis.Contains(generatedShinsei) && generatedShinsei.shinseiType == desiredType && generatedShinsei.shinseiRarity == desiredTier)
                    generatedShinseis.Add(generatedShinsei);
                else
                    i--;

                cacheIndex = i;
            }

            lastIndex = cacheIndex + 1;

            shinseiIcons = photobooth.GetGeneratedSequence(generatedShinseis, 30);
        }

        [Button("Save")]
        private void SaveGeneratedData()
        {
            shinseisToGenerate = 0;
            GenerateShinseis();

            foreach (var shinsei in generatedShinseis)
            {
                if (!shinseiJsons.Contains(JsonUtility.ToJson(shinsei)))
                    shinseiJsons.Add(JsonUtility.ToJson(shinsei));
            }

            foreach (var icon in shinseiIcons)
            {
                string name = string.Format("{0}/" + icon.name + ".png", "GeneratedShinseis/Screenshots", 0);
                var pngShot = icon.texture.EncodeToPNG();
                File.WriteAllBytes(name, pngShot);

                foreach (string genJson in shinseiJsons)
                {
                    string path = string.Format("{0}" + icon.name + ".json", "GeneratedShinseis/JSON/");
                    File.WriteAllText(path, genJson);
                }
            }
        }
    }
}