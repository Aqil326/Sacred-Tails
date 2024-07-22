using System.Collections;
using UnityEngine;
using Timba.Games.CharacterFactory;
using Timba.SacredTails.Database;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.AI;
using Timba.Patterns.ServiceLocator;

namespace Timba.SacredTails.Arena
{
    /// <summary>
    /// Spawns Player's and Oponent's shinseis for the arena instances
    /// </summary>
    public class BattleShinseiSpawner : MonoBehaviour
    {
        [SerializeField] private Transform shinseiParent;
        [SerializeField] private List<Transform> shinseiEndBattle;

        [SerializeField] public Transform enemyParent;

        IDatabase database;


        private void Awake()
        {
            database = ServiceLocator.Instance.GetService<IDatabase>();
        }

        public CharacterSlot SpawnPlayerShinseis(bool isEnemy, string dna)
        {
            if (!isEnemy)
                return SpawnShinsei(shinseiParent, dna).GetComponent<CharacterSlot>();
            else
                return SpawnShinsei(enemyParent, dna).GetComponent<CharacterSlot>();
        }

        public void SpawnShinseiEndGame(List<string> dnas, Transform parent)
        {
            shinseiParent.gameObject.SetActive(false);
            enemyParent.gameObject.SetActive(false);
            shinseiEndBattle[0].parent.position = parent.position;
            for (int i = 0; i < 3; i++)
                SpawnShinsei(shinseiEndBattle[i], dnas[i]).GetComponent<CharacterSlot>();
        }

        private GameObject SpawnShinsei(Transform parent, string dna)
        {
            GameObject battleShinsei = CharacterBuilder.Instance.InstantiateCharacter(0, parent, parent.transform.position);
            battleShinsei.GetComponent<NavMeshAgent>().enabled = false;
            battleShinsei.transform.localScale = Vector3.zero;
            battleShinsei.transform.localPosition = new Vector3(0, -1, 0);
            var charSlot = battleShinsei.GetComponent<CharacterSlot>();
            StartCoroutine(LoadShinsei(charSlot, battleShinsei, dna));

            return battleShinsei;
        }

        IEnumerator LoadShinsei(CharacterSlot _charSlot, GameObject battleShinsei, string _dna)
        {
            _charSlot.SetCharacterCode(database.GetShinseiStructure(_dna));
            _charSlot.UpdateVisual();
            yield return new WaitForSeconds(.1f);

            battleShinsei.GetComponent<CharacterSlot>().SetShinseiEvolution(true);
            battleShinsei.transform.DOScale(0.8f, 1f).OnComplete(() =>
            {
                battleShinsei.GetComponent<CharacterSlot>().SetShinseiEvolution(false);
            });
        }
    }
}
