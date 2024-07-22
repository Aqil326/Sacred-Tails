using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Timba.Games.SacredTails.LobbyDatabase;
using UnityEngine.EventSystems;
using System.Globalization;
using Newtonsoft.Json;
using CoreRequestManager;
using Timba.Games.SacredTails.PopupModule;
using Timba.Games.SacredTails.LobbyNetworking;
using System;
using Timba.Patterns.ServiceLocator;

namespace Timba.SacredTails.TournamentBehavior
{
    /// <summary>
    /// UI Element of clickable button for join to specific tournament
    /// </summary>
    public class TournamentSlot : MonoBehaviour, IPointerClickHandler
    {
        public TextMeshProUGUI tournamentTitle, maxPlayers, initTime, hour, dayName, register;
        public Button joinBtn;
        [SerializeField] Image background;
        string tournamentName = "";
        public Action<TournamentSlot, string> OnClick;

        [Header("Style")]
        [SerializeField] Sprite originalSprite;
        [SerializeField] Sprite selectedSprite;
        [SerializeField] Color originalColor, selectedColor;
        [SerializeField] Image icon;

        public void DrawEntry(TournamentEntry tournamentEntry)
        {
            maxPlayers.text = $"({tournamentEntry.playersJoined} / {tournamentEntry.maxPlayer})";
            //2022-11-22T16:21:26.087Z
            DateTime tournamentDate = DateTime.Parse(tournamentEntry.initTimeStage_1).ToUniversalTime();
            DateTime tournamentDateLocal = tournamentDate.ToLocalTime();

            initTime.text = $"{tournamentDateLocal.Day}/{tournamentDateLocal.Month}/{tournamentDateLocal.Year}";
            hour.text = $"{(tournamentDateLocal.Hour > 12 ? tournamentDateLocal.Hour - 12 : tournamentDateLocal.Hour).ToString("00")}:{tournamentDateLocal.Minute.ToString("00")} {tournamentDateLocal.ToString("tt", CultureInfo.InvariantCulture)}";
            dayName.text = $"{tournamentDateLocal.DayOfWeek}";
            tournamentName = tournamentEntry.tournamentName;
            joinBtn.onClick.RemoveAllListeners();
            TournamentEntry temporalTournamentEntry = tournamentEntry;
            if (PlayerDataManager.Singleton.currentTournamentId == temporalTournamentEntry.tournamentId && PlayerDataManager.Singleton.isBot)
            {
                JoinTournament(PlayerDataManager.Singleton.currentTournamentId, tournamentDate, tournamentName);
                return;
            }

            joinBtn.onClick.AddListener(() =>
            {
                PopupManager popupManager = ServiceLocator.Instance.GetService<PopupManager>();
                Dictionary<PopupManager.ButtonType, Action> mainButtons = new Dictionary<PopupManager.ButtonType, Action>();
                mainButtons.Add(PopupManager.ButtonType.BACK_BUTTON, () => { popupManager.HideInfoPopup(); });
                mainButtons.Add(PopupManager.ButtonType.CONFIRM_BUTTON, () =>
                {
                    JoinTournament(temporalTournamentEntry.tournamentId, tournamentDate, tournamentName);
                    PlayerDataManager.Singleton.currentTournamentId = temporalTournamentEntry.tournamentId;
                    popupManager.HideInfoPopup();
                });
                popupManager.ShowInfoPopup("Subscribe to the Tournament for <sprite=45> 15 ST? \n <size=20><color=#FEED00>Leaving the tournament will cost 75% of your investment</color></size>", mainButtons);
            });
        }

        public void ShowSelected()
        {
            hour.color = selectedColor;
            initTime.color = selectedColor;
            maxPlayers.color = selectedColor;
            background.sprite = selectedSprite;
            register.color = selectedColor;
            icon.color = selectedColor;
        }

        public void ShowUnselected()
        {
            hour.color = originalColor;
            initTime.color = originalColor;
            maxPlayers.color = originalColor;
            background.sprite = originalSprite;
            register.color = originalColor;
            icon.color = originalColor;
        }

        public void JoinTournament(string tournamentKey, DateTime initTimeTournament, string tournamentName)
        {
            if (!PlayerDataManager.Singleton.isBot)
                ServiceLocator.Instance.GetService<PopupManager>().ShowInfoPopup("Trying to add to the tournament");
            ServiceLocator.Instance.GetService<ITournamentDatabase>().TournamentRequest(tournamentKey, TypeOfTournamentRequest.JOIN_TOURNAMENT, (succesResult) =>
            {
                PlayerDataManager.Singleton.currentTournamentStage = 1;
                //{"success":false,"code":3,"message":"Tournament is full","data":null}
                SacredTailsPSDto<JoinTournamentDto> succesDataFromJson = JsonConvert.DeserializeObject<SacredTailsPSDto<JoinTournamentDto>>(succesResult.FunctionResult.ToString());
                TournamentReadyController tournamentReadyController = ServiceLocator.Instance.GetService<ILobbyNetworkManager>().CurrentPlayer.tournamentReadyController;
                if (!succesDataFromJson.success)
                {
                    tournamentReadyController?.EnableReadyButton(false);
                    ServiceLocator.Instance.GetService<PopupManager>().ShowInfoPopup(succesDataFromJson.message);
                    return;
                }
                if (!PlayerDataManager.Singleton.isBot)
                {
                    PendingVariableNPC pending = FindObjectOfType<PendingVariableNPC>(true);
                    pending?.ShowVendor(1);
                    pending?.CheckTournamentInscription.DisableObjectsInTournament(false);
                }
                tournamentReadyController?.gameObject.SetActive(true);
                tournamentReadyController?.ShowTimerInitTournament(initTimeTournament);

                var searchAndShow = FindObjectOfType<SearchAndShow>();
                searchAndShow?.gameObject.SetActive(false);
                ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup($"You successfully subscribed to the tournament: {tournamentName}");
            });
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick.Invoke(this, tournamentName);
        }

        public class JoinTournamentDto
        {
            public string displayName;
            public TournamentPlayerDataDto tournamentPlayerDataDto;
        }
        public class TournamentPlayerDataDto
        {
            public string displayName;
            public int stage, currentBracket;
            public string lastPetitionTimeStamp;
        }
    }
}