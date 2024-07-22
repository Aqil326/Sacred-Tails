using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewBattleMessage : MonoBehaviour
{
    public static NewBattleMessage _instance;

    //public LoopVerticalScrollRect combatLogScrollRect;

    private List<string> combatLogMessages = new List<string>();
    public List<string> _combatLogMessages { get { return combatLogMessages; } set { combatLogMessages = value; } }

    private void Awake()
    {
        _instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //combatLogScrollRect.totalCount = 0;
        //combatLogScrollRect.RefillCells();
        /*combatLogMessages.Add("<color=#F54F4F>[Enemy]</color> the enemy used <color=#FB4CE3>the mud ball</color> and that reduced your <color=#FFA526>attack speed</color> and made you <color=#FFA526>dirty</color>.");
        combatLogMessages.Add("<color=#F54F4F>[1]</color> the enemy used <color=#FB4CE3>the mud ball</color> and that reduced your <color=#FFA526>attack speed</color> and made you <color=#FFA526>dirty</color>.");
        combatLogMessages.Add("<color=#F54F4F>[2]</color> the enemy used <color=#FB4CE3>the mud ball</color> and that reduced your <color=#FFA526>attack speed</color> and made you <color=#FFA526>dirty</color>.");
        combatLogMessages.Add("<color=#F54F4F>[3]</color> the enemy used <color=#FB4CE3>the mud ball</color> and that reduced your <color=#FFA526>attack speed</color> and made you <color=#FFA526>dirty</color>.");
        combatLogMessages.Add("<color=#F54F4F>[4]</color> the enemy used <color=#FB4CE3>the mud ball</color> and that reduced your <color=#FFA526>attack speed</color> and made you <color=#FFA526>dirty</color>.");
        combatLogMessages.Add("<color=#F54F4F>[5]</color> the enemy used <color=#FB4CE3>the mud ball</color> and that reduced your <color=#FFA526>attack speed</color> and made you <color=#FFA526>dirty</color>.");
        combatLogMessages.Add("<color=#F54F4F>[6]</color> the enemy used <color=#FB4CE3>the mud ball</color> and that reduced your <color=#FFA526>attack speed</color> and made you <color=#FFA526>dirty</color>.");
        combatLogMessages.Add("<color=#F54F4F>[7]</color> the enemy used <color=#FB4CE3>the mud ball</color> and that reduced your <color=#FFA526>attack speed</color> and made you <color=#FFA526>dirty</color>.");
        combatLogMessages.Add("<color=#F54F4F>[8]</color> the enemy used <color=#FB4CE3>the mud ball</color> and that reduced your <color=#FFA526>attack speed</color> and made you <color=#FFA526>dirty</color>.");
        combatLogMessages.Add("<color=#F54F4F>[9]</color> the enemy used <color=#FB4CE3>the mud ball</color> and that reduced your <color=#FFA526>attack speed</color> and made you <color=#FFA526>dirty</color>.");

        UpdateCombatLog();*/
    }

    public void InsertAndShowInCombatLog(string locText)
    {
        combatLogMessages.Add(locText);
        //UpdateCombatLog();
    }

    // Update is called once per frame
    /*private void UpdateCombatLog()
    {
        combatLogScrollRect.totalCount = combatLogMessages.Count;

        combatLogScrollRect.RefillCells();

        combatLogScrollRect.verticalNormalizedPosition = 1;
    }*/
}
