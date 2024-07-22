
using System;
using System.Collections.Generic;
using Timba.Games.CharacterFactory;

namespace Timba.SacredTails.Arena
{
    [Serializable]
    public class UserInfo
    {
        public List<ResourceBarValues> healthbars = new List<ResourceBarValues>();
        public List<ResourceBarValues> energybars = new List<ResourceBarValues>();
        public int userIndex;
        public CharacterSlot spawnedShinsei;
        public List<Shinsei> battleShinseis;
        public List<BattleActionsBase> turnActions;
        public int currentShinseiIndex;
        public bool isLocalPlayer;
    }

    [Serializable]
    public class ResourceBarValues
    {
        public int currentValue;
        public int maxValue;
    }
}

