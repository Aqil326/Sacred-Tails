using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Timba.SacredTails.OptionsModule
{
    public class GameSettingsController : MonoBehaviour
    {
        #region ----Fields----
        public GameObject settingsContainer;
        public GameObject settingsPanel;
        public RenderPipelineAsset[] qualityLevel;
        public Terrain terrain;
        public TMP_Dropdown graphicsDropdown;
        public TMP_Dropdown resolutionDropdown;
        public Toggle fullscreenToggle;
        public Toggle badWordFilterToggle;
        public Toggle frenchLayoutToggle;
        public Toggle usersCountToggle;
        public GameObject usersCountLayout;
        public TMP_Text versionText;
        #endregion ----Fields----

        #region ----Methods----	
        #region Init
        public void Start()
        {
            versionText.text = $"Sacred Tails V{Application.version}";

            terrain = FindObjectOfType<Terrain>();

            resolutionDropdown.RefreshShownValue();
            resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionOption", 0);
            ChangeResolution(resolutionDropdown.value);

            resolutionDropdown.RefreshShownValue();
            graphicsDropdown.value = PlayerPrefs.GetInt("GraphicsOption", 0);
            ChangeGraphicSettings(graphicsDropdown.value);

            fullscreenToggle.isOn = PlayerPrefs.GetInt("FullScreenOption", 0) == 1;
            ChangeFullScreen(fullscreenToggle.isOn);

            badWordFilterToggle.isOn = PlayerPrefs.GetInt("BadWordFilterOption", 1) == 1;
            ChangeBadWordFilter(badWordFilterToggle.isOn);

            usersCountToggle.isOn = PlayerPrefs.GetInt("UsersCountOption", 0) == 1;
            ChangeUsersCount(usersCountToggle.isOn);

            frenchLayoutToggle.isOn = PlayerPrefs.GetInt("KeyboardLayout", 0) == 1;
            //Check this first time open game
            if (Application.systemLanguage == SystemLanguage.French)
                if (PlayerPrefs.GetInt("KeyboardLayoutFirstTime", 0) == 0)
                {
                    ChangeKeyboardLayout(true);
                    frenchLayoutToggle.isOn = true;
                    PlayerPrefs.SetInt("KeyboardLayoutFirstTime", 1);
                }
            ChangeKeyboardLayout(frenchLayoutToggle.isOn);
        }

        public void SettingsButtonInteract(bool active)
        {
            settingsPanel.SetActive(active);
        }
        #endregion Init

        #region Fullscreen
        public void ChangeFullScreen()
        {
            PlayerPrefs.SetInt("FullScreenOption", fullscreenToggle.isOn ? 1 : 0);
            Screen.fullScreen = fullscreenToggle.isOn;
            Vector2Int resolution = GetResolution(resolutionDropdown.value, true);
            Screen.SetResolution(resolution.x, resolution.y, fullscreenToggle.isOn);
        }

        public void ChangeFullScreen(bool changeToogle)
        {
            PlayerPrefs.SetInt("FullScreenOption", changeToogle ? 1 : 0);
            Screen.fullScreen = fullscreenToggle.isOn;
            Vector2Int resolution = GetResolution(resolutionDropdown.value, true);
            Screen.SetResolution(resolution.x, resolution.y, fullscreenToggle.isOn);
        }
        #endregion Fullscreen

        #region Resolution
        public void ChangeResolution(int resolutionSettings)
        {
            PlayerPrefs.SetInt("ResolutionOption", resolutionSettings);

            Vector2Int resolution = GetResolution(resolutionSettings, true);
            Screen.SetResolution(resolution.x, resolution.y, fullscreenToggle.isOn);
        }

        public Vector2Int GetResolution(int option, bool saveOnPlayerPrefs = false)
        {
            Vector2Int resolution = new Vector2Int();
            switch (option)
            {
                case 0:
                    resolution = new Vector2Int(800, 600);
                    break;
                case 1:
                    resolution = new Vector2Int(1024, 768);
                    break;
                case 2:
                    resolution = new Vector2Int(1366, 768);
                    break;
                case 3:
                    resolution = new Vector2Int(1600, 900);
                    break;
                case 4:
                    resolution = new Vector2Int(1920, 1080);
                    break;
            }
            return resolution;
        }

        #endregion Resolution

        #region Graphics
        public void ChangeGraphicSettings(int value)
        {
            if (terrain == null)
                return;
            PlayerPrefs.SetInt("GraphicsOption", value);

            QualitySettings.SetQualityLevel(value);
            switch (value)
            {
                case 0:
                    terrain.shadowCastingMode = ShadowCastingMode.Off;
                    terrain.treeLODBiasMultiplier = 0.4f;
                    terrain.drawTreesAndFoliage = false;
                    break;
                case 1:
                    terrain.shadowCastingMode = ShadowCastingMode.Off;
                    terrain.treeLODBiasMultiplier = 0.7f;
                    terrain.drawTreesAndFoliage = true;
                    break;
                case 2:
                    terrain.shadowCastingMode = ShadowCastingMode.On;
                    terrain.treeLODBiasMultiplier = 1f;
                    terrain.drawTreesAndFoliage = true;
                    break;
            }
            QualitySettings.renderPipeline = qualityLevel[value];
        }
        #endregion Graphics

        #region Bad Word Filter
        public void ChangeBadWordFilter(bool changeToogle)
        {
            PlayerPrefs.SetInt("BadWordFilterOption", changeToogle ? 1 : 0);
        }

        public void ChangeKeyboardLayout(bool changeToogle)
        {
            if (PlayerDataManager.Singleton == null)
                return;
            PlayerPrefs.SetInt("KeyboardLayout", changeToogle ? 1 : 0);
            PlayerDataManager.Singleton.isFrenchKeyboardLayout = changeToogle;
        }
        #endregion Fullscreen

        #region Users Count
        public void ChangeUsersCount(bool changeToogle)
        {
            usersCountToggle.isOn = changeToogle;
            usersCountLayout.SetActive(usersCountToggle.isOn);
            PlayerPrefs.SetInt("UsersCountOption", changeToogle ? 1 : 0);
        }

        public void ChangeUsersCount()
        {
            usersCountLayout.SetActive(usersCountToggle.isOn);
            PlayerPrefs.SetInt("UsersCountOption", usersCountToggle.isOn ? 1 : 0);
        }

        #endregion Users Count
        #endregion ----Methods----	
    }
}