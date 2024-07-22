using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Timba.Games.SacredTails.LobbyNetworking;
using Timba.Patterns.ServiceLocator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Timba.SacredTails.CharacterStyle
{
    /// <summary>
    ///     Behavior of panel Character Style Controller
    /// </summary>
    public class CharacterStyleController : MonoBehaviour
    {
        #region ---Fields---
        [Header("References")]
        public CharacterRecolor characterRecolor;
        public CharacterStyleDatabase styleDB;
        public TMP_Text title;
        public Color cameraBackgroundColor;
        public ProfilePictureStyle pictureStyleDB;

        [Header("Prefabs")]
        [Header("Part")]
        public Transform partRowsParent;
        public Transform partRowPrefab;
        public Transform partSlotUnlockedPrefab;
        public Transform partSlotLockedPrefab;
        [Header("Color")]
        public Transform colorRowsParent;
        public Transform colorRowPrefab;
        public Transform colorSlotUnlockedPrefab;
        public Transform colorSlotLockedPrefab;
        public Transform colorSlotEmptyPrefab;

        [Header("Init Values")]
        public int numberOfColumnsPerRow;
        private int currentPartIndex = 0;
        private PartsOfCharacter currentPart;

        [Header("Cameras")]
        public CinemachineVirtualCamera lobbyCamera;
        public CinemachineVirtualCamera styleCamera;
        public LayerMask layerMaskForStyle;
        public PlayerMouseRotator playerMouseRotator;
        private Camera mainCamera;
        public CinemachineVirtualCamera auxStyleCamera;

        [Header("Customizable Buttons")]
        [SerializeField]
        private List<Button> customizableButtons;
        bool doOnce = false;

        public GameObject profilePicture;
        public Image pictureImg;
        public Image frameImg;
        public UnityEvent runEnableStyleMenuEvent;
        public UnityEvent runEnableStyleProfilePhoto;

        #endregion ---Fields---

        #region ---Methods---
        #region Init
        public void Awake()
        {
            mainCamera = Camera.main;
            customizableButtons.ForEach(customButton => customButton.onClick.AddListener(() => ColorOnSelectButton(customButton)));
            ServiceLocator.Instance.GetService<ILobbyNetworkManager>().OnConnected += (a) =>
            {
                currentPlayer = a;
                if (materialReskin == null)
                    materialReskin = currentPlayer.transform.GetChild(2).GetComponent<MaterialReskin>();
                if (materialReskin2 == null)
                    materialReskin2 = currentPlayer.transform.GetChild(1).GetComponent<MaterialReskin>();
                if (bodyStyle == null)
                    bodyStyle = currentPlayer.transform.GetChild(2).GetComponent<BodyStyle>();
                characterRecolor = currentPlayer.GetComponent<CharacterRecolor>();
                ApplyLastStyle();
            };
        }
        /*private void Start()
        {
            ProfilePicture.self.openStylesPanelBtn.onClick.AddListener(this.GetComponent<OpenCharacterStyle>().openCharacterStyleEvent.Invoke);
        }*/
        private ThirdPersonController currentPlayer;
        public void Init()
        {
            //OnSelectPart(currentPartIndex); // Select Face Editing panel 
            OnSelectPart(0); // Select Face Editing panel
            runEnableStyleMenuEvent.Invoke();

            profilePicture.SetActive(false);
            styleCamera.enabled = true;

            lobbyCamera.Priority = 0;
            auxStyleCamera.Priority = 0;
            styleCamera.Priority = 100;

            mainCamera.cullingMask = layerMaskForStyle;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = cameraBackgroundColor;

            currentPlayer.IsMovementBloqued = true;

            if (playerMouseRotator.rotationTarget == null)
                playerMouseRotator.rotationTarget = currentPlayer.transform;

            playerMouseRotator.rotationTarget.rotation = Quaternion.Euler(0, 180, 0);
            playerMouseRotator.canRotateWithMouse = true;

            //pictureImg.sprite = pictureStyleDB.picturesOptions[PlayerPrefs.GetInt("pictureImg", 0)];
            //frameImg.sprite = pictureStyleDB.framingOptions[PlayerPrefs.GetInt("frameImg", 0)];

            if (doOnce == true)
                return;
            doOnce = true;
            StartCoroutine(CallAtEnd());
        }

        public void InitAndOpenProfilePhoto()
        {
            //OnSelectPart(currentPartIndex); // Select Face Editing panel 
            //OnSelectPart(8); // Select Face Editing panel
            //runEnableStyleProfilePhoto.Invoke();
            StartCoroutine(SelectPage());

            profilePicture.SetActive(false);
            styleCamera.enabled = true;

            lobbyCamera.Priority = 0;
            auxStyleCamera.Priority = 0;
            styleCamera.Priority = 100;

            mainCamera.cullingMask = layerMaskForStyle;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = cameraBackgroundColor;

            currentPlayer.IsMovementBloqued = true;

            if (playerMouseRotator.rotationTarget == null)
                playerMouseRotator.rotationTarget = currentPlayer.transform;

            playerMouseRotator.rotationTarget.rotation = Quaternion.Euler(0, 180, 0);
            playerMouseRotator.canRotateWithMouse = true;

            //pictureImg.sprite = pictureStyleDB.picturesOptions[PlayerPrefs.GetInt("pictureImg", 0)];
            //frameImg.sprite = pictureStyleDB.framingOptions[PlayerPrefs.GetInt("frameImg", 0)];

            if (doOnce == true)
                return;
            doOnce = true;
            StartCoroutine(CallAtEnd());
        }

        private IEnumerator SelectPage()
        {
            Debug.Log("Select 8_1");
            yield return new WaitForSeconds(.5f);
            OnSelectPart(8);
            runEnableStyleProfilePhoto.Invoke();
            Debug.Log("Select 8_2");
        }
        #endregion Init

        #region Hide
        public void Hide()
        {
            styleCamera.Priority = 0;
            auxStyleCamera.Priority = 0;
            lobbyCamera.Priority = 1;

            //Everything
            mainCamera.cullingMask = -1;
            mainCamera.clearFlags = CameraClearFlags.Skybox;

            ThirdPersonController currentPlayer = ServiceLocator.Instance.GetService<ILobbyNetworkManager>().CurrentPlayer;
            currentPlayer.IsMovementBloqued = false;

            playerMouseRotator.canRotateWithMouse = false;
        }
        #endregion Hide

        #region ShowCategory
        public void SelectPartNext(bool left)
        {
            int maxIndex = 9;
            if (currentPartIndex > 0 && currentPartIndex < maxIndex)
                OnSelectPart(left ? currentPartIndex - 1 : currentPartIndex + 1);
            else if (currentPartIndex == 0)
                OnSelectPart(left ? maxIndex : 1);
            else if (currentPartIndex == maxIndex)
                OnSelectPart(left ? maxIndex-1 : 0);

            /*
             if (currentPartIndex > 0 && currentPartIndex < 7)
                OnSelectPart(left ? currentPartIndex - 1 : currentPartIndex + 1);
            else if (currentPartIndex == 0)
                OnSelectPart(left ? 7 : 1);
            else if (currentPartIndex == 7)
                OnSelectPart(left ? 6 : 0);
             */
        }

        public void OnSelectPart(int partSelected)
        {
            currentPartIndex = partSelected;
            ColorOnSelectButton(customizableButtons[currentPartIndex]);
            switch (currentPartIndex)
            {
                case 0:
                    ShowCategory(PartsOfCharacter.SKIN);
                    break;
                case 1:
                    ShowCategory(PartsOfCharacter.HAIR);
                    break;
                case 2:
                    ShowCategory(PartsOfCharacter.PRIMARY_COLOR);
                    break;
                case 3:
                    ShowCategory(PartsOfCharacter.LEGS);
                    break;
                case 4:
                    ShowCategory(PartsOfCharacter.HANDS);
                    break;
                case 5:
                    ShowCategory(PartsOfCharacter.SECONDARY_COLOR);
                    break;
                case 6:
                    ShowCategory(PartsOfCharacter.DETAILS);
                    break;
                case 7:
                    ShowCategory(PartsOfCharacter.COLORS);
                    break;
                case 8:
                    ShowCategory(PartsOfCharacter.PICTURE);
                    break;
                case 9:
                    ShowCategory(PartsOfCharacter.FRAME);
                    break;
            }

            int maxIndex = 9;
            if(currentPartIndex == maxIndex || currentPartIndex == maxIndex - 1)
            {
                auxStyleCamera.Priority = 1;
                styleCamera.Priority = 0;
                //styleCamera.enabled = false;
                profilePicture.SetActive(true);
            }
            else if(styleCamera.Priority != 1)
            {
                styleCamera.Priority = 1;
                auxStyleCamera.Priority = 0;
                profilePicture.SetActive(false);
                //styleCamera.enabled = true;
            }
        }

        public string ChangeTitle(PartsOfCharacter part)
        {
            string text = "";
            switch (part)
            {
                case PartsOfCharacter.SKIN:
                    text = "COLOR SKIN";
                    break;
                default:
                    text = $"{part.ToString()} STYLE";
                    break;
            }
            return text;
        }

        public void ShowCategory(PartsOfCharacter part)
        {
            title.text = ChangeTitle(part);
            currentPart = part;

            var unlockedStyles = PlayerDataManager.Singleton.localPlayerData.unlockedStyles;
            List<int> unlockedColors;
            if (unlockedStyles.ContainsKey(part))
                unlockedColors = unlockedStyles[part].unlockedColors;
            else
                unlockedColors = null;
        }
        #endregion ShowCategory

        private MaterialReskin materialReskin, materialReskin2;
        private BodyStyle bodyStyle;
        private RecolorBehavior recolorBehavior;
        /// <summary>
        ///     This fill all necesary data for character style with random values
        /// </summary>
        public void GenerateRandomOutfit()
        {
            recolorBehavior = FindObjectOfType<RecolorBehavior>(true);
            //Fill skin colors
            List<ColorIdRelation> colors = styleDB.GetColorsByPartType(PartsOfCharacter.SKIN);
            List<Color> skinColor = new List<Color>();
            foreach (var color in colors)
                skinColor.Add(color.color);
            //Fill parts of character colors
            UpdateColorPartOfCharacter(PartsOfCharacter.SKIN, skinColor[Random.Range(0, skinColor.Count)]);
            UpdateColorPartOfCharacter(PartsOfCharacter.HAIR, recolorBehavior.possibleColors[Random.Range(0, recolorBehavior.possibleColors.Count)]);
            UpdateColorPartOfCharacter(PartsOfCharacter.PRIMARY_COLOR, recolorBehavior.possibleColors[Random.Range(0, recolorBehavior.possibleColors.Count)]);
            UpdateColorPartOfCharacter(PartsOfCharacter.SECONDARY_COLOR, recolorBehavior.possibleColors[Random.Range(0, recolorBehavior.possibleColors.Count)]);
            UpdateColorPartOfCharacter(PartsOfCharacter.DETAILS, recolorBehavior.possibleColors[Random.Range(0, recolorBehavior.possibleColors.Count)]);
            UpdateColorPartOfCharacter(PartsOfCharacter.HANDS, Color.white);
            UpdateColorPartOfCharacter(PartsOfCharacter.LEGS, Color.white);
            UpdatePartOfCharacter(PartsOfCharacter.HAIR, Random.Range(0, 3));
            UpdatePartOfCharacter(PartsOfCharacter.SKIN, Random.Range(0, 2));
        }

        /// <summary>
        ///     This download the character style data and apply to the model in the game
        /// </summary>
        public void ApplyLastStyle()
        {
            List<PartsOfCharacter> parts = new List<PartsOfCharacter>()
        {
            PartsOfCharacter.SKIN,
            PartsOfCharacter.HAIR,
            PartsOfCharacter.PRIMARY_COLOR,
            PartsOfCharacter.SECONDARY_COLOR,
            PartsOfCharacter.DETAILS,
            PartsOfCharacter.HANDS,
            PartsOfCharacter.LEGS
        };
            foreach (var part in parts)
            {
                Color targetColor;
                ColorUtility.TryParseHtmlString("#" + PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle[part].colorHex, out targetColor);
                if (part == PartsOfCharacter.SKIN)
                {
                    UpdateGender();
                }
                characterRecolor.ChangeMaterialColors(part, targetColor);
            }
            ChangeHairBodyPart(PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle[PartsOfCharacter.HAIR].presetId);
        }

        /// <summary>
        ///     Toggle betwen male and female model
        /// </summary>
        public void UpdateGender()
        {
            if (PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle[PartsOfCharacter.SKIN].presetId == 1)
            {
                characterRecolor.transform.GetChild(1).gameObject.SetActive(true);
                characterRecolor.transform.GetChild(2).gameObject.SetActive(false);
            }
            else
            {
                characterRecolor.transform.GetChild(2).gameObject.SetActive(true);
                characterRecolor.transform.GetChild(1).gameObject.SetActive(false);
            }
        }

        public static void UpdateColorPartOfCharacter(PartsOfCharacter part, Color color, bool isFill = true)
        {
            if (PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle.ContainsKey(part))
                PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle[part] = new CharacterStyleInfo() { colorHex = ColorUtility.ToHtmlStringRGB(color), presetId = PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle[part].presetId };
            else
                PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle.Add(part, new CharacterStyleInfo() { colorHex = ColorUtility.ToHtmlStringRGB(color), presetId = 0 });
        }

        public static void UpdatePartOfCharacter(PartsOfCharacter part, int partIndex)
        {
            if (PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle.ContainsKey(part))
                PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle[part] = new CharacterStyleInfo() { colorHex = PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle[part].colorHex, presetId = partIndex };
            else
                PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle.Add(part, new CharacterStyleInfo() { colorHex = ColorUtility.ToHtmlStringRGB(Color.white), presetId = partIndex });
        }

        public void ChangeProfileImage(int targetPart)
        {
            //PlayerPrefs.SetInt("pictureImg", targetPart);
            ProfilePicture.self.pictureImg.sprite = pictureStyleDB.picturesOptions[targetPart];
            pictureImg.sprite = pictureStyleDB.picturesOptions[targetPart];

            PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle[PartsOfCharacter.PICTURE] = new CharacterStyleInfo() { colorHex = "FFFFFF", presetId = targetPart };
        }

        public void ChangeProfileFrame(int targetPart)
        {
            //PlayerPrefs.SetInt("frameImg", targetPart);
            ProfilePicture.self.frameImg.sprite = pictureStyleDB.framingOptions[targetPart];
            frameImg.sprite = pictureStyleDB.framingOptions[targetPart];

            PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle[PartsOfCharacter.FRAME] = new CharacterStyleInfo() { colorHex = "FFFFFF", presetId = targetPart };
        }

        public void UpdateCharacterStyle()
        {
            PlayerDataManager.Singleton.UpdateCharacterStyleForAnyReason();
        }
        /// <summary>
        ///     Show a different part of hair in the model
        /// </summary>
        /// <param name="targetPart"></param>
        public void ChangeHairBodyPart(int targetPart)
        {
            bodyStyle.bodyParts[0].SelectObject(targetPart, true);
            materialReskin.ChangePart(targetPart, 4);
            materialReskin2.ChangePart(targetPart, 4);
        }

        IEnumerator CallAtEnd()
        {
            yield return null;
            PopulateSkinColors();
        }

        [SerializeField] GameObject prefabFaceStyle, prefabEmptyStyle, prefabRowFaceStyle;
        [SerializeField] Transform rowParent;
        /// <summary>
        ///     Fill panel of color options using a scriptable object
        /// </summary>
        public void PopulateSkinColors()
        {
            List<ColorIdRelation> colors = styleDB.GetColorsByPartType(PartsOfCharacter.SKIN);
            List<Transform> boxes = new List<Transform>();
            for (int i = 0; i < Mathf.Ceil(colors.Count / 5f); i++)
            {
                GameObject box = Instantiate(prefabRowFaceStyle, rowParent);
                boxes.Add(box.transform);
                box.gameObject.SetActive(true);
            }
            int counter = 0;
            foreach (var color in colors)
            {
                CharacterStyleSlot partSlot = null;
                partSlot = Instantiate(prefabFaceStyle, boxes[counter / 5]).GetComponent<CharacterStyleSlot>();
                partSlot.gameObject.SetActive(true);
                partSlot.OnColorSelected += (a) =>
                {
                    characterRecolor.ChangeMaterialColors(PartsOfCharacter.SKIN, color.color);
                    UpdateColorPartOfCharacter(PartsOfCharacter.SKIN, color.color, false);
                };
                partSlot.InitSlot<CharacterStyleRelation>(color);
                counter++;
            }
            int childCount = boxes.Last().childCount;
            for (int i = 0; i < 5 - childCount; i++)
                Instantiate(prefabEmptyStyle, boxes.Last()).SetActive(true);
        }
        #region Customizable Buttons

        private void ColorOnSelectButton(Button buttonSelected)
        {
            customizableButtons.ForEach(customButton =>
            {
                ColorButton(customButton, new Color32(164, 161, 141, 255), new Color32(241, 236, 207, 255), Color.white);
            });

            ColorButton(buttonSelected, Color.white, Color.white, new Color32(241, 236, 207, 255));
        }

        private void ColorButton(Button button, Color32 normalColor, Color32 selectedColor, Color32 color)
        {
            ColorBlock colorBlock = button.colors;
            colorBlock.normalColor = normalColor;
            colorBlock.selectedColor = selectedColor;
            button.colors = colorBlock;
            button.GetComponent<Image>().color = color;
        }

        #endregion

        #endregion ---Methods---
    }
}