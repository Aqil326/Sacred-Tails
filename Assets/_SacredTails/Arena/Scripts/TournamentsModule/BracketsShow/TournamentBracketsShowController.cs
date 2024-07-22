using CoreRequestManager;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Timba.Games.SacredTails.LobbyDatabase;
using Timba.Games.SacredTails.PopupModule;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.TournamentBehavior;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Timba.SacredTails.TournamentBehavior
{
    public class TournamentBracketsShowController : MonoBehaviour
    {
        public Transform bracketsContentParent;
        public GameObject bracketColumnPrefab;
        public SingleBracketPlayerLabelController bracketLabelPrefab;
        public string testBracket = "";
        public TextMeshProUGUI titleText;
        //public int spacing1 = 100;
        //public int spacing2 = 100;
        //private RectTransform mainBracketColumn;
        private float auxHeight = 0;
        //int currentStage = 0;
        //int counterBrackets = 0;
        public List<RectTransform> bracketsColumn = new List<RectTransform>();

        public (int, int) GetTotalNumberOfStagesAndBrackets(int maxPlayers)
        {
            int bracketPerStage = maxPlayers / 2;
            int totalNumberOfBrackets = bracketPerStage;
            int totalNumberOfStages = 0;
            while (bracketPerStage > 1 || bracketPerStage % 2 == 1)
            {
                bracketPerStage = bracketPerStage / 2;
                totalNumberOfBrackets += bracketPerStage;
                totalNumberOfStages++;
            }
            return (totalNumberOfStages, totalNumberOfBrackets);
        }

        public void OnEnable()
        {
            //ShowTestTournament();
            InitBracket();
            //StartCoroutine(InitBracket());
        }

        [ContextMenu("Test")]
        public void ShowTestTournament()
        {
            PlayerDataManager.Singleton.currentTournamentId = testBracket;
            InitBracket();
            //StartCoroutine(InitBracket());
        }

        public void InitBracket()
        {
            //Debug.Log("PlayerDataManager.Singleton.currentTournamentId: " + PlayerDataManager.Singleton.currentTournamentId);

            bracketsColumn.Clear();

            if (string.IsNullOrEmpty(PlayerDataManager.Singleton.currentTournamentId))
            {
                ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("You are not registered in any tournament");
                return;
            }
            foreach (Transform child in bracketsContentParent.transform)
                GameObject.Destroy(child.gameObject);

            ServiceLocator.Instance.GetService<ITournamentDatabase>().TournamentRequest(PlayerDataManager.Singleton.currentTournamentId,
            TypeOfTournamentRequest.GET_CURRENT_BRACKETS_DATA,
            (result) =>
            {
                //string auxResult = "{\"success\":true,\"code\":0,\"message\":\"Getting current stage brackets success\",\"data\":{\"stages\":{\"stage_1\":{\"brackets\":{\"bracket_1\":{\"PID1\":\"vlakdo\",\"PID2\":\"mangelhoyos\",\"matchId\":\"TT-TestX36_D4F105DCCA52B839_888E17541988002A\",\"p1Joined\":true,\"p2Joined\":false,\"displayName1\":\"vlakdo\",\"displayName2\":\"mangelhoyos\"},\"bracket_2\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_3\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_4\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_5\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_6\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_7\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_8\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_9\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_10\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_11\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_12\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_13\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_14\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_15\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_16\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_17\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_18\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_19\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_20\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_21\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_22\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_23\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_24\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_25\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_26\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_27\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_28\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_29\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_30\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_31\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_32\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_33\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_34\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_35\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_36\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_37\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_38\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_39\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_40\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_41\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_42\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_43\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_44\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_45\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_46\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_47\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_48\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_49\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_50\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_51\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_52\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_53\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_54\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_55\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_56\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_57\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_58\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_59\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_60\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_61\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_62\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_63\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_64\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_65\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_66\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_67\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_68\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_69\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_70\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_71\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_72\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_73\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_74\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_75\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_76\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_77\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_78\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_79\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_80\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_81\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_82\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_83\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_84\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_85\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_86\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_87\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_88\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_89\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_90\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_91\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_92\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_93\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_94\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_95\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_96\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_97\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_98\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_99\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_100\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_101\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_102\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_103\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_104\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_105\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_106\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false},\"bracket_107\":{\"PID1\":\"WinByDefault\",\"PID2\":\"WinByDefault\",\"matchId\":\"TT-TestX36_WinByDefault_WinByDefault\",\"p1Joined\":false,\"p2Joined\":false}}},\"stage_2\":{\"brackets\":{}},\"stage_3\":{\"brackets\":{}},\"stage_4\":{\"brackets\":{}},\"stage_5\":{\"brackets\":{}},\"stage_6\":{\"brackets\":{}},\"stage_7\":{\"brackets\":{}},\"stage_8\":{\"brackets\":{}}},\"maxPlayers\":215}}";
                Debug.Log("*Brackets* RESULT InitBracket: " + result.FunctionResult.ToString());
                SacredTailsPSDto<TournamentBracketsData> response = JsonConvert.DeserializeObject<SacredTailsPSDto<TournamentBracketsData>>(result.FunctionResult.ToString());
                //SacredTailsPSDto<TournamentBracketsData> response = JsonConvert.DeserializeObject<SacredTailsPSDto<TournamentBracketsData>>(auxResult);
                var tournamentData = response.data;
                var stagesData = tournamentData.stages;

                bool isInitBracket = true;
                (int totalStages,
                 int totalBrackets) = GetTotalNumberOfStagesAndBrackets(tournamentData.maxPlayers);

                //(int currentStage, int counterBrackets) = SetCurrentBrackets(tournamentData.maxPlayers, totalStages, stagesData, ref isInitBracket);

                titleText.text = "Brackets Round " + stagesData.Count;

                (int currentStage,
                int counterBrackets,
                int auxCounterPadding,
                 int auxCounterSpacing) = SetCurrentBrackets(tournamentData.maxPlayers, totalStages, stagesData, ref isInitBracket);

                (auxCounterPadding,
                 auxCounterSpacing) = FillFutureBrackets(tournamentData.maxPlayers, totalStages, currentStage, ref isInitBracket, ref counterBrackets, auxCounterPadding, auxCounterSpacing);

                if (counterBrackets == totalBrackets)
                    FillFinalBracket(tournamentData.maxPlayers, isInitBracket, counterBrackets, currentStage, totalStages, auxCounterPadding, auxCounterSpacing);

                StartCoroutine(UpdateColumns());
            });
        }

        private IEnumerator UpdateColumns()
        {
            yield return new WaitForSeconds(0.5f);

            auxHeight = bracketsColumn[0].sizeDelta.y;

            foreach (RectTransform aux in bracketsColumn)
            {
                aux.sizeDelta = new Vector2(aux.sizeDelta.x, auxHeight);
            }

            yield return null;
        }

        #region Create brackets
        public List<string> GetWinnersOfThisStage(Dictionary<string, StageBracketData> stagesData, int currentStageIndex)
        {
            List<string> winnersDisplayName = new List<string>();
            if (stagesData.Count > currentStageIndex)
            {
                var nextStageData = stagesData["stage_" + (currentStageIndex + 1)];
                foreach (var bracketData in nextStageData.brackets)
                {
                    winnersDisplayName.Add(bracketData.Value.PID1);
                    winnersDisplayName.Add(bracketData.Value.PID2);
                }
            }
            return winnersDisplayName;
        }

        public (int, int, int, int) SetCurrentBrackets(int maxPlayers, int totalStages, Dictionary<string, StageBracketData> stagesData, ref bool isInitBracket)
        //public IEnumerator SetCurrentBrackets(int maxPlayers, int totalStages, Dictionary<string, StageBracketData> stagesData, ref bool isInitBracket)
        {
            int currentStage = 0;
            int counterBrackets = 0;

            int auxCounterPadding = 0;
            int auxCounterSpacing = 0;

            Debug.Log("*Brackets* stagesData.Count 01: " + stagesData.Count);

            for (int i = 1; i <= stagesData.Count; i++)
            {
                StageBracketData currentStageData = stagesData["stage_" + i];
                List<string> winnersOfThisStage = GetWinnersOfThisStage(stagesData, currentStage + 1);

                Debug.Log("*Brackets* currentStageData.brackets.Count 02: " + currentStageData.brackets.Count);

                //Instantiate column
                //var currentColumnBracket = i == 1 ? Instantiate(bracketColumnPrefab, bracketsContentParent) : Instantiate(mainBracketColumn, bracketsContentParent);
                var currentColumnBracket = Instantiate(bracketColumnPrefab, bracketsContentParent);
                bracketsColumn.Add(currentColumnBracket.GetComponent<RectTransform>());

                if (i == 1)
                {
                    //mainBracketColumn = currentColumnBracket;
                    auxHeight = 88 * (currentStageData.brackets.Count * 2);//currentColumnBracket.GetComponent<RectTransform>().sizeDelta.y;
                    //auxHeight = currentColumnBracket.GetComponent<RectTransform>().sizeDelta.y;
                    //mainBracketColumn = currentColumnBracket.GetComponent<RectTransform>();
                    //Debug.Log("auxHeight 01: " + auxHeight + "((currentStageData.brackets.Count + 1) * 2): " + ((currentStageData.brackets.Count + 0.5f) * 2));
                }
                else
                {
                    currentColumnBracket.GetComponent<ContentSizeFitter>().enabled = false;
                    currentColumnBracket.GetComponent<RectTransform>().sizeDelta = new Vector2(currentColumnBracket.GetComponent<RectTransform>().sizeDelta.x, auxHeight);

                    auxCounterPadding = i >= 3 ? (auxCounterPadding * 2) + 1 : 0;
                    auxCounterSpacing = i >= 2 ? (auxCounterSpacing * 2) + 1 : 0;

                    currentColumnBracket.GetComponent<VerticalLayoutGroup>().padding.top = 44 + (88 * auxCounterPadding); //(i > 3 ? 88 * (i - 2 + 1) : 88 * (i - 2));
                    currentColumnBracket.GetComponent<VerticalLayoutGroup>().spacing = 88 * auxCounterSpacing;
                }

                Debug.Log("auxHeight 02: " + auxHeight);

                //SetSpacingBetweenBrackets(currentColumnBracket.GetComponent<VerticalLayoutGroup>(), currentStage);
                foreach (var bracketData in currentStageData.brackets)
                {
                    if (bracketData.Value.PID1.Equals("WinByDefault") && !bracketData.Value.PID2.Equals("WinByDefault"))
                    {
                        CreateBracketLabel(bracketData.Value.PID1, false, isInitBracket, maxPlayers, currentColumnBracket, counterBrackets, currentStage, totalStages, true, new List<string>());
                        if (counterBrackets != maxPlayers)
                            CreateBracketLabel(bracketData.Value.PID2, true, isInitBracket, maxPlayers, currentColumnBracket, counterBrackets, currentStage, totalStages, true, winnersOfThisStage);

                        counterBrackets++;
                        continue;
                    }
                    else if (bracketData.Value.PID2.Equals("WinByDefault") && !bracketData.Value.PID1.Equals("WinByDefault"))
                    {
                        CreateBracketLabel(bracketData.Value.PID1, false, isInitBracket, maxPlayers, currentColumnBracket, counterBrackets, currentStage, totalStages, true, winnersOfThisStage);
                        if (counterBrackets != maxPlayers)
                            CreateBracketLabel(bracketData.Value.PID2, true, isInitBracket, maxPlayers, currentColumnBracket, counterBrackets, currentStage, totalStages, true, new List<string>());
                        
                        counterBrackets++;
                        continue;
                    }

                    //Instantiate bracekts
                    CreateBracketLabel(bracketData.Value.PID1, false, isInitBracket, maxPlayers, currentColumnBracket, counterBrackets, currentStage, totalStages, true, winnersOfThisStage);
                    if (counterBrackets != maxPlayers)
                        CreateBracketLabel(bracketData.Value.PID2, true, isInitBracket, maxPlayers, currentColumnBracket, counterBrackets, currentStage, totalStages, true, winnersOfThisStage);

                    counterBrackets++;
                }

                if (isInitBracket)
                    isInitBracket = false;
                currentStage++;
            }

            return (currentStage, counterBrackets, auxCounterPadding, auxCounterSpacing);
            //yield return null;
        }

        public (int, int) FillFutureBrackets(int maxPlayers, int totalStages, int currentStage, ref bool isInitBracket, ref int counterBrackets, int auxCounterPadding, int auxCounterSpacing)
        {
            Debug.Log("*Brackets* currentStage: " + currentStage);
            //int auxCounterPadding = 0;
            //int auxCounterSpacing = 0;

            Debug.Log("auxHeight 03: " + auxHeight);

            for (int i = currentStage; i < totalStages; i++)
            {
                var currentColumnBracket = Instantiate(bracketColumnPrefab, bracketsContentParent);
                int stageBrackets = (int)(maxPlayers / (Mathf.Pow(2, (i + 1))));

                bracketsColumn.Add(currentColumnBracket.GetComponent<RectTransform>());

                currentColumnBracket.GetComponent<ContentSizeFitter>().enabled = false;
                currentColumnBracket.GetComponent<RectTransform>().sizeDelta = new Vector2(currentColumnBracket.GetComponent<RectTransform>().sizeDelta.x, auxHeight);

                auxCounterPadding = i >= 2 ? (auxCounterPadding * 2) + 1 : 0;
                auxCounterSpacing = i >= 1 ? (auxCounterSpacing * 2) + 1 : 0;

                currentColumnBracket.GetComponent<VerticalLayoutGroup>().padding.top = 44 + (88 * auxCounterPadding); //(i > 3 ? 88 * (i - 2 + 1) : 88 * (i - 2));
                currentColumnBracket.GetComponent<VerticalLayoutGroup>().spacing = 88 * auxCounterSpacing;

                Debug.Log("auxHeight 04: " + auxHeight);

                Debug.Log("counter i 05: " + i);

                //SetSpacingBetweenBrackets(currentColumnBracket.GetComponent<VerticalLayoutGroup>(), i);
                for (int j = 0; j < stageBrackets; j++)
                {
                    CreateBracketLabel("...", false, isInitBracket, maxPlayers, currentColumnBracket, counterBrackets, i, totalStages);
                    CreateBracketLabel("...", true, isInitBracket, maxPlayers, currentColumnBracket, counterBrackets, i, totalStages);
                    counterBrackets++;

                }
            }

            return (auxCounterPadding, auxCounterSpacing);
        }

        /*public void SetSpacingBetweenBrackets(VerticalLayoutGroup currentColumnBracket, int currentStage)
        {
            var previousStage = (Mathf.Max(0, currentStage - 1));
            if (currentStage % 2 == 0)
                //currentColumnBracket.GetComponent<VerticalLayoutGroup>().spacing = 88 * ((previousStage * currentStage) + previousStage);
                currentColumnBracket.GetComponent<VerticalLayoutGroup>().spacing = spacing1 * ((previousStage * currentStage) + previousStage);
            else
                //currentColumnBracket.GetComponent<VerticalLayoutGroup>().spacing = 88 * ((previousStage * currentStage) + 1);
                currentColumnBracket.GetComponent<VerticalLayoutGroup>().spacing = spacing2 * ((previousStage * currentStage) + 1);
        }*/

        public void FillFinalBracket(int maxPlayers, bool isInitBracket, int counterBrackets, int currentStage, int totalStages, int auxCounterPadding, int auxCounterSpacing)
        {
            var currentColumnBracket = Instantiate(bracketColumnPrefab, bracketsContentParent);
            bracketsColumn.Add(currentColumnBracket.GetComponent<RectTransform>());

            currentColumnBracket.GetComponent<ContentSizeFitter>().enabled = false;
            currentColumnBracket.GetComponent<RectTransform>().sizeDelta = new Vector2(currentColumnBracket.GetComponent<RectTransform>().sizeDelta.x, auxHeight);

            auxCounterPadding = (auxCounterPadding * 2) + 1;
            auxCounterSpacing = (auxCounterSpacing * 2) + 1;

            currentColumnBracket.GetComponent<VerticalLayoutGroup>().padding.top = 44 + (88 * auxCounterPadding);
            currentColumnBracket.GetComponent<VerticalLayoutGroup>().spacing = 88 * auxCounterSpacing;

            CreateBracketLabel("...", false, isInitBracket, maxPlayers, currentColumnBracket, counterBrackets, currentStage, totalStages, isFinalBracket: true);
        }
        #endregion Create brackets

        #region Helpers 

        public void CreateBracketLabel(string _PID, bool _isOddNumber, bool _isInitBracket, int _maxPlayers, GameObject _currentColumnBracket, int _counterBrackets, int _currentStage, int maxStage, bool _isAnExistingStage = false, List<string> winnersOfCurrentStage = null, bool isFinalBracket = false)
        {
            SingleBracketPlayerLabelController currentBracket = Instantiate(bracketLabelPrefab, _currentColumnBracket.transform);
            currentBracket.InitPlayerLabel(isInitBracket: _isInitBracket,
                                            isWinnerBracket: _counterBrackets == _maxPlayers - 1,
                                            isOddNumber: _isOddNumber,
                                            playerName: _PID.Equals("WinByDefault") ? "-" : _PID,
                                            currentStage: _currentStage,
                                            totalStages: maxStage,
                                            isWinnerOfPreviousStage: winnersOfCurrentStage != null ? winnersOfCurrentStage.Contains(_PID) : false,
                                            isAnExistingStage: _isAnExistingStage,
                                            isFinalBracket: isFinalBracket);
        }
        #endregion Helpers 
    }

}

public class BracketsData
{
    public string PID1;
    public string PID2;
    public string displayName1;
    public string displayName2;

    public string matchId;
    public bool p1Joined;
    public bool p2Joined;
}

public class StageBracketData
{
    public Dictionary<string, BracketsData> brackets;
}

public class TournamentBracketsData
{
    public int maxPlayers;
    public Dictionary<string, StageBracketData> stages;
}
