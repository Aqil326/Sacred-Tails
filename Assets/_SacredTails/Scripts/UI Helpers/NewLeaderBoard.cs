using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System;
using Newtonsoft.Json;
using PlayFab.CloudScriptModels;
using PlayFab;
using CoreRequestManager;
using Timba.SacredTails.UiHelpers;

public class NewLeaderBoard : MonoBehaviour
{
    [SerializeField] List<LeaderboardElement> createdElements = new List<LeaderboardElement>();
    [SerializeField] GameObject prefabLeaderboardElement;
    [SerializeField] Transform parentOfElements;
    private List<LeaderboardElement.ElementData> listData = new List<LeaderboardElement.ElementData>();
    private List<LeaderboardElement.ElementData> victorieData = new List<LeaderboardElement.ElementData>();
    [Header("UI")]
    [SerializeField] TextMeshProUGUI division;
    [SerializeField] TextMeshProUGUI name, points, victories, position, playerName, playerPoints, playerVictories, playerPosition, positionInitial;
    [SerializeField] ChangeIconLeague leguePlayer, legueGeneric;

    [SerializeField] int currentDivision;
    [SerializeField] private RewardsRankingSystemController rewardsController;
    [SerializeField] private bool isRewardWindow = false;
    public void ChangeIsRewardWindow(bool _isRewardWindows)
    {
        isRewardWindow = _isRewardWindows;
    }

    private void OnEnable()
    {
        name.text = PlayerDataManager.Singleton.localPlayerData.playerName;
        playerName.text = name.text;
        PlayfabManager.Singleton.GetStatistics((statistics) =>
        {
            List<string> names = statistics.Select(a => a.StatisticName).ToList();
            foreach(string name in names)
            {
                Debug.Log("Debug leaderboard : "+name);
            }
            CheckDivision(names, Division.Bronze, statistics);
            CheckDivision(names, Division.Silver, statistics);
            CheckDivision(names, Division.Gold, statistics);
            CheckDivision(names, Division.Champion, statistics);
            if (names.Contains("Victories"))
                if (statistics[names.IndexOf("Victories")].Value != -1)
                {
                    Debug.Log("Statistics value : " + statistics[names.IndexOf("Victories")].Value);
                    victories.text = statistics[names.IndexOf("Victories")].Value.ToString();
                    playerVictories.text = victories.text;
                }
        });
    }

    private void CheckDivision(List<string> names, Division division, List<PlayFab.ClientModels.StatisticValue> statistics)
    {
        if (names.Contains(division.ToString()))
            if (statistics[names.IndexOf(division.ToString())].Value != -1)
            {
                this.division.text = division.ToString();
                points.text = statistics[names.IndexOf(division.ToString())].Value.ToString();
                playerPoints.text = points.text;
                leguePlayer.ChangeIconUsingIndex((int)division);
                legueGeneric.ChangeIconUsingIndex((int)division);
                isFirstTime = true;
                GetListOfDivision((int)division);
            }
    }

    public void DrawList(List<LeaderboardElement.ElementData> elements)
    {
        foreach (var item in createdElements)
            item.gameObject.SetActive(false);
        for (int i = 0; i < elements.Count; i++)
        {
            if (createdElements.Count <= i)
                createdElements.Add(Instantiate(prefabLeaderboardElement, parentOfElements).GetComponent<LeaderboardElement>());
            createdElements[i].DrawElement(elements[i]);
            createdElements[i].button.onClick.RemoveAllListeners();
            string createdName = createdElements[i].ElementDataObject.name;
            string createdPoints = createdElements[i].ElementDataObject.points;
            string createdPosition = createdElements[i].ElementDataObject.position;
            createdElements[i].button.onClick.AddListener(() =>
            {
                name.text = createdName;
                points.text = createdPoints;
                position.text = createdPosition;
                legueGeneric.ChangeIconUsingIndex(currentDivision);
            });
            createdElements[i].gameObject.SetActive(true);
        }
    }
    bool isFirstTime = false;
    public void GetListOfDivision(int divisionIndex)
    {
        Division division = (Division)divisionIndex;
        currentDivision = divisionIndex;
        listData.Clear();
        victorieData.Clear();
        if (!isRewardWindow)
        {
            PlayfabManager.Singleton.GetLeaderboardEntries(0, 100, division.ToString(), (a) =>
            {
                StatisticRecursiveSearch(a, division.ToString(), 0, ProcessDivisionList, () =>
                {
                    PlayfabManager.Singleton.GetLeaderboardEntries(0, 100, "Victories", (a) =>
                    {
                        StatisticRecursiveSearch(a, "Victories", 0, ProcessVictorieList, () =>
                        {
                            createdElements = createdElements.OrderByDescending((element) => Int32.Parse(element.ElementDataObject.points)).ToList();
                            foreach (var elementObject in createdElements)
                            {
                                if (elementObject.gameObject.activeSelf)
                                {
                                    List<LeaderboardElement.ElementData> elementDatas = victorieData.Where(a => a.name == elementObject.ElementDataObject.name).ToList();

                                    string textVictories = "0";
                                    if (elementDatas.Count > 0)
                                    {
                                        LeaderboardElement.ElementData targetElementData = elementDatas.First();
                                        textVictories = targetElementData.victories;
                                    }
                                    elementObject.ChangeVictories(textVictories);
                                    //UpdateCallback for buttons
                                    string temporalVictoryText = textVictories;
                                    elementObject.button.onClick.AddListener(() =>
                                    {
                                        victories.text = temporalVictoryText;
                                    });

                                    //Position change in list
                                    int newPosition = createdElements.IndexOf(elementObject);
                                    elementObject.ChangePosition(newPosition + 1);
                                    elementObject.transform.SetSiblingIndex(newPosition);

                                    if (isFirstTime && elementObject.ElementDataObject.name == PlayerDataManager.Singleton.localPlayerData.playerName)
                                    {
                                        position.text = (createdElements.IndexOf(elementObject) + 1).ToString();
                                        playerPosition.text = position.text;
                                        positionInitial.text = position.text;
                                        isFirstTime = false;
                                    }
                                }
                            }
                        });
                    });
                });
            });
        }
        else
            rewardsController.InitRewardSystem(division);
    }

    public void StatisticRecursiveSearch(PlayFab.ClientModels.GetLeaderboardResult result, string division, int iterationNumber, Action<PlayFab.ClientModels.GetLeaderboardResult> Callback, Action OnLastIteration = null)
    {
        Callback.Invoke(result);
        //Recursivity
        if (result.Leaderboard.Count >= 99)
        {
            int minValue, maxValue;
            minValue = ((int)result.Leaderboard.Count / 100 * 100) + (iterationNumber * 100) + 1;
            maxValue = ((int)result.Leaderboard.Count / 100 * 100) + (iterationNumber * 100) + 100;
            PlayfabManager.Singleton.GetLeaderboardEntries(minValue, maxValue, division, (a) =>
            {
                StatisticRecursiveSearch(a, division, iterationNumber + 1, Callback);
            });
        }
        else
            OnLastIteration?.Invoke();
    }

    public void ProcessDivisionList(PlayFab.ClientModels.GetLeaderboardResult result)
    {
        foreach (var user in result.Leaderboard)
        {
            if (user.StatValue == -1)
                continue;
            listData.Add(new LeaderboardElement.ElementData() { name = user.DisplayName, points = user.StatValue.ToString(), position = (user.Position + 1).ToString(), victories = "-" });
        }
        DrawList(listData);
        listData.Clear();
    }

    public void ProcessVictorieList(PlayFab.ClientModels.GetLeaderboardResult result)
    {
        foreach (var user in result.Leaderboard)
        {
            if (user.StatValue == -1)
                continue;
            victorieData.Add(new LeaderboardElement.ElementData() { name = user.DisplayName, victories = user.StatValue.ToString() });
        }
    }

    public enum Division
    {
        Bronze = 0,
        Silver = 1,
        Gold = 2,
        Champion = 3
    }
}
