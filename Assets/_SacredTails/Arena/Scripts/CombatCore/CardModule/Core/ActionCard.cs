using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Timba.Games.CharacterFactory;
using UnityEngine;
using UnityEngine.Serialization;

namespace Timba.SacredTails.Arena
{
    /// <summary>
    /// Code representation of attack cards in the game
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "ActionCard", menuName = "CombatModule/ActionCard", order = 1)]
    public class ActionCard : ScriptableObject
    {
        public CharacterType cardType;
        [FormerlySerializedAs("TurnAction")] public List<BattleActionData> BattleActions;
        [JsonIgnore][TextArea] public string Description;
        [JsonIgnore][TextArea] public string DisplayNotification;
        [SerializeField][JsonIgnore] public int VfxIndex;
        [SerializeField][JsonIgnore] public bool vfxAffectBoth;
        [SerializeField][JsonIgnore] public AttacksAnimation casterAnimation;
        [SerializeField][JsonIgnore] public AttacksAnimation targetAnimation;
        [SerializeField] public int isComingFromCopyIndex = -1;
        [JsonIgnore] public Sprite cardImage;
        public int PpCost;
        [HideInInspector] public List<string> BattleAction;
        public bool ShouldSerializeBattleActions()
        {
            return false;
        }

        public void OnValidate()
        {
            foreach (var battleAction in BattleActions)
            {
                if (battleAction.actionElementType != cardType)
                    battleAction.actionElementType = cardType;
            }
        }
    }
}