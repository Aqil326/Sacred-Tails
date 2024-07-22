using System;
using System.Collections;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using System.IO;
using Timba.Games.Recolor;
using Timba.Games.CharacterFactory;

namespace Timba.Database
{
    public class DatabaseEditorWindow : OdinMenuEditorWindow
    {
        private static DatabaseEditorSettings settings;
        [SerializeField] private static CharacterDatabase characterDatabase;
        [SerializeField] private static PartIndex partsIndex;
        private static string DBName;
        [MenuItem("Timba/Database Editor")]
        private static void OpenWindow()
        {
            GetWindow<DatabaseEditorWindow>().Show();
        }

        protected override OdinMenuTree BuildMenuTree()
        {

            if (settings == null)
            {
                // Look for an existing Settings file
                string[] settingsFiles = AssetDatabase.FindAssets("DatabaseEditorSettings");
                if (settingsFiles.Length > 1)
                {
                    settings = AssetDatabase.LoadAssetAtPath<DatabaseEditorSettings>(
                        AssetDatabase.GUIDToAssetPath(settingsFiles[0]));
                }
                else
                {
                    settings = ScriptableObject.CreateInstance<DatabaseEditorSettings>();
                    AssetDatabase.CreateAsset(settings, "Assets/_content/Databases/DatabaseEditorSettings.asset");
                    AssetDatabase.SaveAssets();
                }

            }


            var tree = new OdinMenuTree(supportsMultiSelect: false);

            // Add the settings menu
            tree.Add("Settings", settings, EditorIcons.SettingsCog);
            if (!AssetDatabase.IsValidFolder(settings.DATABASES_FOLDER))
            {
                AssetDatabase.CreateFolder(settings.DATABASES_FOLDER, CharacterSlot.FOLDER_NAME);

                Debug.LogErrorFormat("The databases folder '{0}' doesnt exist. Create it or change the value at DATABASES_FOLDER int the DatabaseEditorWindow settings");
            }

            // Add a menu for each Database
            tree.Add($"{CharacterSlot.FOLDER_NAME}/Create New Character", new CreateNewParts(), EditorIcons.Plus);
            return tree;
        }

        protected override void OnBeginDrawEditors()
        {
            OdinMenuTreeSelection selected = this.MenuTree != null ? this.MenuTree.Selection : null;
            SirenixEditorGUI.BeginHorizontalToolbar();
            GUILayout.FlexibleSpace();
            if (SirenixEditorGUI.ToolbarButton("Delete Current"))
            {
                string path = AssetDatabase.GetAssetPath(selected.SelectedValue as GameObject);
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.SaveAssets();
            }

            SirenixEditorGUI.EndHorizontalToolbar();
            base.OnBeginDrawEditors();
        }

        public class CreateNewParts
        {
            private List<string> _tempStringAssets = new List<string>();
            private string[] searchedFilesForDB;
            private string[] searchedFilesForPI;
            [LabelText("Game Object")]
            public GameObject[] psbFile;

            [
                Toggle("Toggled"),

                InfoBox("Cuando se implementa está opción se permite seleccionar manualmente que id implementar " +
                "(puede ser usado para sobreescribir un slot previamente creado). De no estar seleccionado, se seleccionara un valor " +
                "equivalente a la capacidad de la lista de gestos."),

                LabelText("isUsingManualIDAsignation")
            ]
            public ToggleCharacterSlot toggleCharacterSlot = new ToggleCharacterSlot();

            [
                Toggle("Toggled"),

                InfoBox("Si desean crear una nueva Data Base, solo deben elegir esta opción y elegir un nombre."),
                LabelText("Use a manual Data Base")
            ]
            public ManualDataBase manualDataBase = new ManualDataBase();

            [
                Toggle("Toggled"),
                InfoBox("Si el personaje es en 3D y desea que sea recolorable, solo deben elegir esta opción," +
                "(el Prefab que va a utilizarse ya debe tener configurado el shader de recolor en su material)"),
                LabelText("Is a 3D asset")
                ]
            public RecolorableAsset3D is3DAsset = new RecolorableAsset3D();

            [
                Toggle("Toggled"),
                InfoBox("Arrastre aqui un Scriptable object si quiere un indice que contenga los nombres de las partes"),
                LabelText("Use Part Index SO")
            ]
            public PartsIndex usePartIndex = new PartsIndex();

            private string[] searchedFiles;
            [Button(ButtonSizes.Large), GUIColor(0.4f, 0.4f, 1)]

            public void CreateFromCharacterSlot()
            {
                if (psbFile == null || psbFile.Length == 0)
                {
                    Debug.LogWarning("No valid .psb file or not selected any .psb file");
                    return;
                }
                searchedFilesForDB = AssetDatabase.FindAssets(nameof(CharacterDatabase));
                if (manualDataBase.Toggled)
                {

                    _tempStringAssets.Clear();
                    foreach (string guid1 in searchedFilesForDB)
                    {
                        _tempStringAssets.Add(AssetDatabase.GUIDToAssetPath(guid1));
                    }

                    var _tempAsset = _tempStringAssets.Find(x => x.Split('/').Last() == manualDataBase.DataBaseName + ".asset");
                    int index = _tempStringAssets.FindIndex(x => x.Split('/').Last() == manualDataBase.DataBaseName + ".asset");

                    if (_tempAsset != null)
                    {
                        characterDatabase = AssetDatabase.LoadAssetAtPath<CharacterDatabase>(
                            AssetDatabase.GUIDToAssetPath(searchedFilesForDB[index]));
                    }
                    else
                    {
                        CharacterDatabase _characterDB = new CharacterDatabase();
                        AssetDatabase.CreateAsset(_characterDB, $"Assets/_content/Databases/{manualDataBase.DataBaseName}.asset");
                        int newIndex = _tempStringAssets.FindIndex(x => x.Split('/').Last() == manualDataBase.DataBaseName + ".asset");
                        string[] newSearchedFilesForDB = AssetDatabase.FindAssets(nameof(CharacterDatabase));
                        characterDatabase = AssetDatabase.LoadAssetAtPath<CharacterDatabase>(
                           AssetDatabase.GUIDToAssetPath(newSearchedFilesForDB[newIndex]));
                    }
                }

                searchedFilesForPI = AssetDatabase.FindAssets(nameof(PartIndex));
                if (usePartIndex.Toggled)
                {
                    _tempStringAssets.Clear();
                    foreach (string guid1 in searchedFilesForPI)
                    {
                        _tempStringAssets.Add(AssetDatabase.GUIDToAssetPath(guid1));
                    }

                    var _tempAsset = _tempStringAssets.Find(x => x.Split('/').Last() == usePartIndex.PartIndexName + ".asset");
                    int index = _tempStringAssets.FindIndex(x => x.Split('/').Last() == usePartIndex.PartIndexName + ".asset");

                    if (_tempAsset != null)
                    {
                        partsIndex = AssetDatabase.LoadAssetAtPath<PartIndex>( AssetDatabase.GUIDToAssetPath(searchedFilesForPI[index]));
                        partsIndex.ClearAllLists();
                    }
                    else
                    {
                        PartIndex _partIndex = new PartIndex();
                        AssetDatabase.CreateAsset(_partIndex, $"Assets/_content/PartIndex/{usePartIndex.PartIndexName}.asset");
                        AssetDatabase.Refresh();
                        int newIndex = _tempStringAssets.FindIndex(x => x.Split('/').Last() == usePartIndex.PartIndexName + ".asset");
                        string[] newSearchedFilesForPI = AssetDatabase.FindAssets(nameof(PartIndex));
                        partsIndex = AssetDatabase.LoadAssetAtPath<PartIndex>(
                            AssetDatabase.GUIDToAssetPath(newSearchedFilesForPI[newIndex]));
                    }
                }

                //characterDatabase.AddElement();
                // 0. Reference paths
                string characterSlotsPath = $"{settings.DATABASES_FOLDER}/{CharacterSlot.FOLDER_NAME}";
                string partSlotsPath = $"{characterSlotsPath}/{PartSlot.FOLDER_NAME}";
                string partEntityPath = $"{partSlotsPath}/{PartEntity.FOLDER_NAME}";
                bool isExistCharacterSlotInAssets = false;

                if (!AssetDatabase.IsValidFolder(characterSlotsPath))
                {
                    AssetDatabase.CreateFolder(settings.DATABASES_FOLDER, CharacterSlot.FOLDER_NAME);
                    Debug.LogWarning($"Folder created in asset path: \n {characterSlotsPath}");
                }

                if (!AssetDatabase.IsValidFolder(partSlotsPath))
                {
                    AssetDatabase.CreateFolder(characterSlotsPath, PartSlot.FOLDER_NAME);
                    Debug.LogWarning($"Folder created in asset path: \n {partSlotsPath}");
                }

                if (!AssetDatabase.IsValidFolder(partEntityPath))
                {
                    AssetDatabase.CreateFolder(partSlotsPath, PartEntity.FOLDER_NAME);
                    Debug.LogWarning($"Folder created in asset path: \n {partEntityPath}");
                }
                // 1. Crear El objeto con el character slot
                //int gestureIndex = characterDatabase != null && characterDatabase.CharacterSlotPrefabs != null && characterDatabase.CharacterSlotPrefabs.Length > 0 ? characterDatabase.CharacterSlotPrefabs.Length : 0;
                int gestureIndex = characterDatabase != null && characterDatabase.CharacterSlotPrefabsList != null && characterDatabase.CharacterSlotPrefabsList.Count > 0 ? characterDatabase.CharacterSlotPrefabsList.Count : 0;
                // Si es verdadero se crear� un nuevo objeto
                GameObject newCharacter = null;
                searchedFiles = AssetDatabase.FindAssets(toggleCharacterSlot.characterSelectorID, new string[] { characterSlotsPath });

                if (/*searchedFiles.Length > 0 &&*/ toggleCharacterSlot.Toggled)
                {
                    // newCharacter = AssetDatabase.LoadAssetAtPath<CharacterSlot>(
                    //     AssetDatabase.GUIDToAssetPath(searchedFiles[0])).gameObject;


                    // isExistCharacterSlotInAssets = true;
                    newCharacter = new GameObject($"{toggleCharacterSlot.characterSelectorID}", new Type[] { typeof(CharacterSlot) });
                    newCharacter.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

                    newCharacter.GetComponent<CharacterSlot>().characterID = newCharacter.name;
                }
                else
                {
                    newCharacter = new GameObject($"Gesture_{gestureIndex}", new Type[] { typeof(CharacterSlot) });
                    newCharacter.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

                    newCharacter.GetComponent<CharacterSlot>().characterID = newCharacter.name;
                }

                // 2. Crear las partes de los gameObject/.PSB obtenidos y asignarlas al nuevo character
                foreach (var item in psbFile)
                {
                    // 2.1 Se crea un prefab variant del gameObject/.psb (para que obtenga todos sus cambios futuros)
                    Object partAsset = PrefabUtility.InstantiatePrefab(item);

                    // 2.2 Se crea y se almacena una referncia del prefab variant de slot
                    GameObject partAssetVariantAsset = PrefabUtility.SaveAsPrefabAsset(partAsset as GameObject, $"{partSlotsPath}/{partAsset.name}_{newCharacter.name}.prefab");
                    DestroyImmediate(partAsset);
                    partAssetVariantAsset.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

                    // 2.3 Se asigna el componente partSlot si se encontr�

                    characterDatabase.FindPartSlots(partAssetVariantAsset, (_slot) =>
                    {

                        _slot.AddComponent(typeof(PartSlot));
                        //use this tryparse to set the bodypart of the PartSlot object.
                        PartType pt;
                        if (Enum.TryParse(_slot.gameObject.name.Split('_').First(), out pt))
                        {
                            _slot.GetComponent<PartSlot>().bodyPart = pt;
                        }

                        //_slot.GetComponent<PartSlot>().bodyPart = characterDatabase.FindPartTypeInName(_slot.gameObject);

                        // 2.4 Se asignan los componentes partEntities
                        List<GameObject> partEntities = new List<GameObject>();
                        characterDatabase.FindPartEntities(_slot, (_entity) =>
                        {
                            _entity.gameObject.AddComponent(typeof(PartEntity));
                            _entity.gameObject.SetActive(partEntities.Count == 0);
                            partEntities.Add(_entity);
                            if (!is3DAsset.Toggled)
                            {
                                if (_entity.GetComponent<SpriteRenderer>() != null)
                                {
                                    _entity.GetComponent<SpriteRenderer>().sortingLayerName = _entity.name.Split('_').First();
                                }
                                foreach (Transform child in _entity.transform)
                                {
                                    if (child.gameObject.GetComponent<SpriteRenderer>() != null)
                                    {
                                        child.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = child.name.Split('_').First();
                                    }
                                }
                                var allchilds = _entity.transform.GetComponentsInChildren<SpriteRenderer>();
                                foreach (var item in allchilds)
                                {
                                    item.gameObject.AddComponent<Timba.Characters.PartVisual>();
                                    item.gameObject.AddComponent<Recolorable>();
                                }
                            }
                            else
                            {
                                if (is3DAsset.isRecolorable)
                                {
                                    _entity.AddComponent<Timba.Recolor.RecolorablePart3D>();
                                    foreach(Transform child in _entity.transform)
                                    {
                                        child.gameObject.AddComponent<Timba.Recolor.RecolorablePart3D>();
                                    }
                                }
                            }
                            if (usePartIndex.Toggled)
                            {
                                partsIndex.PopulateLists(_entity.name);
                            }
                            var entity = Instantiate(_entity);
                            var entityAsset = PrefabUtility.SaveAsPrefabAssetAndConnect(entity, $"{partEntityPath}/{_entity.name}_{newCharacter.name}.prefab", InteractionMode.AutomatedAction);

                            var partSlot = _slot.GetComponent<PartSlot>();
                            characterDatabase.AddPartEntityToArray(partSlot.bodyPart, entityAsset.GetComponent<PartEntity>());
                            DestroyImmediate(entity);
                        });
                    });

                    // 2.5 Se instancia el asset previamente creado y se ubica dentro del nuevo personaje
                    Object partAssetVariant = PrefabUtility.InstantiatePrefab(partAssetVariantAsset, newCharacter.transform);
                }

                GameObject characterSlotAsset = null;

                // Save character as new prefab
                if (isExistCharacterSlotInAssets)
                {
                    characterSlotAsset = PrefabUtility.SavePrefabAsset(newCharacter);
                }
                else
                {
                    characterSlotAsset = PrefabUtility.SaveAsPrefabAssetAndConnect(newCharacter, $"{characterSlotsPath}/{newCharacter.name}.prefab", InteractionMode.AutomatedAction);
                }

                AssetDatabase.SaveAssets();

                //AssetDatabase.Refresh();

                // TODO: descomentqar cuando se est�n eliminando bien los elementos
                characterDatabase.AddCharacterSlotToArray(characterSlotAsset.GetComponent<CharacterSlot>());
                DestroyImmediate(newCharacter, false);
            }
        }

        [Serializable]
        public class ToggleCharacterSlot
        {
            public bool Toggled;
            public string characterSelectorID = "Shinsei_Atlas";
        }
        [Serializable]
        public class ManualDataBase
        {
            public bool Toggled;
            public string DataBaseName = "CharacterDatabase";
        }

        [Serializable]
        public class RecolorableAsset3D
        {
            public bool Toggled;
            public bool isRecolorable;
        }

        [Serializable]
        public class PartsIndex
        {
            public bool Toggled;
            public string PartIndexName = "PartIndex";
        }
    }
}