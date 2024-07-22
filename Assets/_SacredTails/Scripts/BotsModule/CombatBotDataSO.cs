using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Timba.Games.CharacterFactory;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.Arena;
using Timba.SacredTails.Database;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCombatBotSO", menuName = "Timba/SacredTails/CombatBots/NewCombatBotSO")]
public class CombatBotDataSO : ScriptableObject
{
    [Title("Bot information")]
    public string botDisplayName;
    public string botPlayfabId;
    public BotType botType;
    [Title("Bot Shinsei's Cards")]
    public BotShinseiData[] botShinseis = new BotShinseiData[5];
    public List<Shinsei> shinseis = new List<Shinsei>();

    [Button]
    public void GenerateData()
    {
        shinseis.Clear();
        Dictionary<string, string> data = new Dictionary<string, string>();

        int index = 1;
        foreach (BotShinseiData shinseiData in botShinseis)
        {
            string newShinseiDna = shinseiData.randomShinseiDna ? ServiceLocator.Instance.GetService<IDatabase>().GetRandomShinsei() : shinseiData.shinseiDna;

            List<int> actionsList = new List<int>();
            for(int i = 0; i < shinseiData.actionCards.Length; i++)
            {
                actionsList.Add(ServiceLocator.Instance.GetService<IDatabase>().GetActionIndex(shinseiData.actionCards[i]));
            }

            Shinsei newShinsei = new Shinsei()
            {
                ShinseiDna = newShinseiDna,
                ShinseiActionsIndex = actionsList,
                ShinseiOriginalStats = shinseiData.randomShinseiStats ? ServiceLocator.Instance.GetService<IDatabase>().GetShinseiStats(newShinseiDna) : shinseiData.shinseiStats,
                shinseiType = shinseiData.shinseiType,
                shinseiRarity = ServiceLocator.Instance.GetService<IDatabase>().ObtainShinseiRarity(newShinseiDna)
            };

            newShinsei.shinseiHealth = newShinsei.ShinseiOriginalStats.Health;
            newShinsei.shinseiEnergy = newShinsei.ShinseiOriginalStats.Energy;

            shinseis.Add(newShinsei);

            data.Add($"ShinseiSlot{index}", JsonUtility.ToJson(newShinsei));
            if(index == 1)
                data.Add("ShinseiCompanion", JsonUtility.ToJson(newShinsei));

            index++;
        }

        //PlayfabManager.Singleton.SetUserDataForBot(data ,botPlayfabId, () => { Debug.Log("Bot data sent"); }, PlayFab.ClientModels.UserDataPermission.Public);
        
    }
}

[System.Serializable]
public class BotShinseiData
{
    public bool randomShinseiDna = true;
    [HideIf("randomShinseiDna")]
    public string shinseiDna;
    public CharacterType shinseiType;
    public ActionCard[] actionCards = new ActionCard[4];
    public bool randomShinseiStats;
    [HideIf("randomShinseiStats")]
    public ShinseiStats shinseiStats;
}

public enum BotType
{
    agressive,
    thinker,
    random
}
