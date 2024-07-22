using UnityEngine;
using System;
using Unity.Netcode;
using Unity.Collections;
using Timba.Games.CharacterFactory;
using Timba.SacredTails.Database;
using System.Collections;
using Timba.Games.SacredTails.Lobby;
using Timba.Patterns.ServiceLocator;

namespace Timba.SacredTails.Lobby
{
    /// <summary>
    /// Spawns shinsei as network objects for the lobby scene 
    /// </summary>

    public class ShinseiSpawner : MonoBehaviour
    {
        [SerializeField] private Transform shinseiParent;
        private string currentShinsei;
        public CharacterSlot characterSlot;
        IDatabase database;

        private void Awake()
        {
            database = ServiceLocator.Instance.GetService<IDatabase>();
        }

        public void SpawnOtherShinsei(string shinseiCompanionDna, Transform parent, Vector3 playerPos)
        {
            SpawnShinsei(parent, playerPos);
            ChangeCurrentShinsei(shinseiCompanionDna);
        }

        public void OnSpawn(Transform parent, bool isLocalPlayerShinsei = false, bool useGravity = false)
        {
            if (GetComponent<ThirdPersonController>().IsLocalPlayer)
            {
                SpawnShinsei(parent, parent.position, isLocalPlayerShinsei, useGravity);
                if (GetComponent<ThirdPersonController>().IsLocalPlayer)
                {
                    foreach (var partyManager in FindObjectsOfType<PartyManager>())
                        partyManager.shinseiSpawner = this;

                    ChangeCurrentShinsei(PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion.ShinseiDna);
                }
            }
        }

        public void ChangeCurrentShinsei(string newValue)
        {
            currentShinsei = newValue;
            if (characterSlot != null)
            {
                characterSlot.SetCharacterCode(database.GetShinseiStructure(new FixedString64Bytes(newValue).ToString()));
                characterSlot.UpdateVisual();
            }
        }

        private void SpawnShinsei(Transform parent, Vector3 position, bool isLocalPlayerShinsei = false, bool hasGravity = false)
        {
            GameObject CharacterObject = CharacterBuilder.Instance.InstantiateCharacter(0, parent, position);

            characterSlot = CharacterObject.GetComponent<CharacterSlot>();
            CharacterObject.GetComponent<ShinseiMovement>().SetOwner(shinseiParent);
            characterSlot.transform.position = this.transform.position + new Vector3(0,0,3);
            characterSlot.GetComponent<Rigidbody>().useGravity = hasGravity;
            if (isLocalPlayerShinsei)
            {
                CharacterObject.layer = LayerMask.NameToLayer("PlayerShinsei");
                SetGameLayerRecursive(CharacterObject.gameObject, LayerMask.NameToLayer("PlayerShinsei"));
                characterSlot.GetComponent<Rigidbody>().isKinematic = true;
                characterSlot.gameObject.AddComponent<InteractWithShinsei>();
            }
            else
                characterSlot.GetComponent<Rigidbody>().isKinematic = true;

            StartCoroutine(LoadShinsei());
        }

        private void SetGameLayerRecursive(GameObject _go, int _layer)
        {
            _go.layer = _layer;
            foreach (Transform child in _go.transform)
            {
                child.gameObject.layer = _layer;

                Transform _HasChildren = child.GetComponentInChildren<Transform>();
                if (_HasChildren != null)
                    SetGameLayerRecursive(child.gameObject, _layer);

            }
        }

        IEnumerator LoadShinsei()
        {
            while (true)
            {
                if (!String.IsNullOrEmpty(currentShinsei))
                {
                    characterSlot.SetCharacterCode(database.GetShinseiStructure(new FixedString64Bytes(currentShinsei).ToString()));
                    characterSlot.UpdateVisual();
                    break;
                }
                yield return new WaitForSeconds(.1f);
            }

        }
    }
}

