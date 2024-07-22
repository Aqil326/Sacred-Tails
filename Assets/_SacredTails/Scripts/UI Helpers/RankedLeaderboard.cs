using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Timba.SacredTails.UiHelpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankedLeaderboard : MonoBehaviour
{
    [Header("Leaderboard elements")]
    [SerializeField] private LeaderboardElement LeaderboardElement;
    [SerializeField] private Transform parentOfElements;
    [SerializeField] private SelectableButtonGroup divisionSelectableGroup;

    [Header("Player data")]
    [SerializeField] private int initialPlayerPoints = 1000;
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private TMP_Text playerPoints;
    [SerializeField] private TMP_Text playerVictories;
    [SerializeField] private TMP_Text playerPosition;
    [SerializeField] private Image divisionIconImage;
    [SerializeField] private Sprite[] divisionIcons;
    
    List<LeaderboardElement> elementsInTable = new List<LeaderboardElement>();

    private RankDivisions selectedDivision = RankDivisions.BRONZE;
    private string actualLoggedUser;

    [HideInInspector] public bool isProcessingLeaderboard = false;

    public class RankedLeaderboardEntry
    {
        public string name;
        public int totalPoints = 0;
        public int victories = 0;
    }

    public enum RankDivisions
    {
        BRONZE,
        SILVER,
        GOLD,
        CHAMPIONS
    }

    void OnEnable()
    {
        isProcessingLeaderboard = false;
        SetActualDivision(0, true);
    }

    public void SetPlayer(LoginResult loginResult)
    {
        string auxPlayFabId = "";
        string auxPlayerName = "";

        if (loginResult.InfoResultPayload != null)
        {
            auxPlayFabId = loginResult.InfoResultPayload.AccountInfo.PlayFabId;
            auxPlayerName = loginResult.InfoResultPayload.AccountInfo.Username;
        }
        else
        {
            auxPlayFabId = loginResult.PlayFabId;
            auxPlayerName = "";
        }

        //actualLoggedUser = loginResult.InfoResultPayload.AccountInfo.Username;
        //actualLoggedUser = loginResult.InfoResultPayload != null ? loginResult.InfoResultPayload.AccountInfo.Username : "ouo";
        actualLoggedUser = auxPlayerName;
        playerName.text = actualLoggedUser;

        PlayfabManager.Singleton.GetLeaderboardAroundPlayer(auxPlayFabId/*loginResult.InfoResultPayload.AccountInfo.PlayFabId*/, 1, "RankedLeaderboard", (leaderboard) =>
        {
            Debug.Log("Player leaderboard + " + leaderboard.Leaderboard[0].StatValue);
            if(leaderboard.Leaderboard[0].StatValue == 0)
            {
                //Give initial value to ranked leaderboard
                PlayfabManager.Singleton.UpdatePlayerStatistics("RankedLeaderboard",initialPlayerPoints);
            }
        });
    }

    public void GetLeaderboard(bool initializeLoggedPlayer = false)
    {
        isProcessingLeaderboard = true;
        //Clean table before generating
        foreach (var element in elementsInTable)
        {
            Destroy(element.gameObject);
        }
        elementsInTable.Clear();

        PlayfabManager.Singleton.GetLeaderboardEntries(0, 100, "RankedLeaderboard", (leaderboard) =>
        {
            PlayfabManager.Singleton.GetLeaderboardEntries(0, 100, "RankedVictories", (victories) =>
            {
                Dictionary<string, int> rankedLeaderboard = leaderboard.Leaderboard.ToDictionary(item => item.DisplayName, item => item.StatValue);
                Dictionary<string, int> victoryData = victories.Leaderboard.ToDictionary(item => item.DisplayName, item => item.StatValue);

                // Combine both victories and leaderboard into a single list
                List<RankedLeaderboardEntry> leaderboardEntriesList = new List<RankedLeaderboardEntry>();
                foreach (var kvp in rankedLeaderboard)
                {
                    string displayName = kvp.Key;
                    int totalPoints = kvp.Value;
                    int victoriesValue = victoryData.ContainsKey(displayName) ? victoryData[displayName] : 0;

                    // Login user data in the leaderboard
                    if (displayName == actualLoggedUser && initializeLoggedPlayer)
                    {
                        playerPoints.text = totalPoints.ToString();
                        playerVictories.text = victoriesValue.ToString();
                        SetActualDivision((int)GetDivisionByPoints(totalPoints), generateTable: false);
                        divisionIconImage.sprite = divisionIcons[(int)selectedDivision];
                    }

                    RankedLeaderboardEntry rankedEntry = new RankedLeaderboardEntry
                    {
                        name = displayName,
                        totalPoints = totalPoints,
                        victories = victoriesValue
                    };

                    leaderboardEntriesList.Add(rankedEntry);
                }

                // Order the list by points and alphabetically
                leaderboardEntriesList = leaderboardEntriesList
                    .OrderByDescending(entry => entry.totalPoints)
                    .ThenBy(entry => entry.name)
                    .ToList();

                foreach (var result in leaderboardEntriesList)
                {
                    Debug.Log("Entry: " + result.name + " - " + result.totalPoints + " - " + result.victories);
                }

                GenerateTable(leaderboardEntriesList);
            });
        });
    }

    public void SetActualDivision(int division, bool isInitial = false, bool generateTable = true)
    {
        if (!isProcessingLeaderboard || !generateTable)
        {
            selectedDivision = (RankDivisions)division;
            divisionSelectableGroup.SelectButton(division, true);
            Debug.Log("Division changed to : " + selectedDivision.ToString());
            if (generateTable)
                GetLeaderboard(isInitial);
        }
    }

    public void SetActualDivision(int division)
    {
        if (!isProcessingLeaderboard)
        {
            selectedDivision = (RankDivisions)division;
            divisionSelectableGroup.SelectButton(division);
            Debug.Log("Division changed to : " + selectedDivision.ToString());
            GetLeaderboard(false);
        }
    }

    private void GenerateTable(List<RankedLeaderboardEntry> rankedEntries)
    {
        Debug.Log("Generate table");
        //Generate table
        int tablePosition = 1;
        foreach(var entry in rankedEntries)
        {
            try
            {
                if(selectedDivision != GetDivisionByPoints(entry.totalPoints))
                {
                    continue;
                }
                if (actualLoggedUser == entry.name)
                {
                    playerPosition.text = tablePosition.ToString();
                }
                LeaderboardElement newTableElement = Instantiate(LeaderboardElement, parentOfElements);
                newTableElement.SetRankedElementData(entry.name, entry.victories.ToString(), entry.totalPoints.ToString(), tablePosition.ToString());
                newTableElement.gameObject.SetActive(true);
                elementsInTable.Add(newTableElement);
                tablePosition++;
            }
            catch(Exception e)
            {
               Debug.LogWarning(e.Message);
            }
        }

        isProcessingLeaderboard = false;
    }

    private RankDivisions GetDivisionByPoints(int points)
    {
        if(points <= 499)
        {
            return RankDivisions.BRONZE;
        }
        else if(points >= 500 && points <= 999)
        {
            return RankDivisions.SILVER;
        }
        else if(points >= 1000 && points <= 1499)
        {
            return RankDivisions.GOLD;
        }
        else
        {
            return RankDivisions.CHAMPIONS;
        }
    }
}
