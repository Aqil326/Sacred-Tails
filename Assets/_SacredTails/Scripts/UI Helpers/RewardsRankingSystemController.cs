using CoreRequestManager;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.CloudScriptModels;
using System.Collections;
using System.Collections.Generic;
using Timba.SacredTails.UiHelpers;
using UnityEngine;
using UnityEngine.UI;
using static NewLeaderBoard;

public class RewardsRankingSystemController : MonoBehaviour
{
    public Transform container;
    public GameObject rowPrefab;
    public EntryController prefabEntry;
    public void InitRewardSystem(Division rewardSystem)
    {
        var req = new ExecuteFunctionRequest()
        {
            FunctionName = "BattleServer_GetRewardsRankSystem"
        };

        PlayFabCloudScriptAPI.ExecuteFunction(req, (result) =>
        {
            SacredTailsPSDto<RankRewardDto> rewardsData = JsonConvert.DeserializeObject<SacredTailsPSDto<RankRewardDto>>(result.FunctionResult.ToString());

            List<RankRewardEntry> currentRankReward = new List<RankRewardEntry>();

            foreach (Transform child in container.transform)
                GameObject.Destroy(child.gameObject);

            switch (rewardSystem)
            {
                case Division.Bronze:
                    currentRankReward = rewardsData.data.Bronze;
                    break;
                case Division.Silver:
                    currentRankReward = rewardsData.data.Silver;
                    break;
                case Division.Gold:
                    currentRankReward = rewardsData.data.Gold;
                    break;
                case Division.Champion:
                    currentRankReward = rewardsData.data.Champion;
                    break;
            }

            Transform currentRow = null;
            for (int i = 0; i < currentRankReward.Count; i++)
            {
                if (i % 2 == 0)
                    currentRow = Instantiate(rowPrefab, container).transform;

                EntryController entryController = Instantiate(prefabEntry, currentRow);
                entryController.FillData(currentRankReward[i]);

                if (i % 2 != 0)
                {
                    var rectTrans = entryController.GetComponent<RectTransform>();
                    StartCoroutine(Wait(currentRow));
                }
            }

        }, (err) =>
        {
            int p = 0;
        });
    }
    IEnumerator Wait(Transform currentRow)
    {
        yield return new WaitForEndOfFrame();
        currentRow.gameObject.AddComponent<HorizontalLayoutGroup>();
    }
}


[System.Serializable]
public class RankRewardDto
{
    public List<RankRewardEntry> Bronze;
    public List<RankRewardEntry> Silver;
    public List<RankRewardEntry> Gold;
    public List<RankRewardEntry> Champion;
}

[System.Serializable]
public class RankRewardEntry
{
    public int? position;
    public float thresholdUp;
    public float thresholdDown;
    public List<RewardEntry> rewards;
}

[System.Serializable]
public class RewardEntry
{
    public string type;
    public int amount;
}
