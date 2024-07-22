using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Timba.Games.CharacterFactory;
using UnityEditor;
using UnityEngine;

namespace Timba.SacredTails.Arena.ShinseiType
{
    [CreateAssetMenu(fileName = "ShineiTypeScriptable", menuName = "Timba/SacredTails/ShinseiTypeScriptable")]
    [System.Serializable]
    public class ShinseiTypeScriptable : ScriptableObject
    {
        public List<ShinseiTypeListClass> shinseiTypeMatrix = new List<ShinseiTypeListClass>() { new ShinseiTypeListClass() };

        public void GetShinseiTypes()
        {
            int shinseiTypes = Enum.GetNames(typeof(CharacterType)).Length;
            for (int i = shinseiTypeMatrix.Count; i < shinseiTypes; i++)
            {
                AddColumn();
                AddRow();
            }
        }

        public float CompareTypesAndGetDamage(CharacterType currentShinsei, CharacterType targetShinesi)
        {
            return shinseiTypeMatrix[(int)currentShinsei].rows[(int)targetShinesi];
        }

        public string GetMessageForTypeDamage(float typeDamageMultiplier)
        {
            string result = null;
            if (typeDamageMultiplier == 0)
                result = "attack had no effect on the shinsei.";
            if (typeDamageMultiplier < 1)
                result = "attack was not very effective";
            if (typeDamageMultiplier > 1)
                result = "attack was super effective";

            return result;
        }

        public void SerializeShinseiTypeMatrix()
        {
            string data = JsonConvert.SerializeObject(shinseiTypeMatrix);
            File.WriteAllText("Assets/_content/ServerData/ShinseiTypeScriptable.json", data);
        }

        private void AddRow()
        {
            ShinseiTypeListClass newList = new ShinseiTypeListClass();
            for (int i = 0; i < shinseiTypeMatrix[0].rows.Count; i++)
                newList.rows.Add(1);
            shinseiTypeMatrix.Add(newList);
        }

        private void AddColumn()
        {
            for (var i = 0; i < shinseiTypeMatrix.Count; i++)
                shinseiTypeMatrix[i].rows.Add(1);
        }
    }

    [System.Serializable]
    public class ShinseiTypeListClass
    {
        public List<float> rows = new List<float>() { 1 };
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ShinseiTypeScriptable))]
    public class ShinseiTypeScriptableEditor : Editor
    {
        private static ShinseiTypeScriptable baseClass;
        private static SerializedObject targetSerial;
        public void OnEnable()
        {
            baseClass = (ShinseiTypeScriptable)target;
            targetSerial = new SerializedObject(target);
        }
        Vector2 scrollPos;
        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromScriptableObject(baseClass), typeof(ShinseiTypeScriptable), false);
            GUI.enabled = true;

            if (GUILayout.Button("Generate JSON"))
                baseClass.SerializeShinseiTypeMatrix();

            baseClass.GetShinseiTypes();
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.button.fontSize = BOX_SIZE / 4;
            GUI.backgroundColor = Color.white;
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            for (int i = -1; i < baseClass.shinseiTypeMatrix.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
                GUILayout.FlexibleSpace();
                if (i == -1)
                    GUILayout.Space(BOX_SIZE / 5);
                else
                    CenterGUILabel(true, ((CharacterType)i).ToString());

                for (int j = -1; j < baseClass.shinseiTypeMatrix[0].rows.Count; j++)
                {
                    if (i == -1)
                    {
                        if (j == -1)
                        {
                            GUILayout.Space(BOX_SIZE * 3);
                            GUILayout.Label("", GUILayout.Width(BOX_SIZE), GUILayout.Height(BOX_SIZE * 4));
                        }
                        else
                        {
                            Vector2 pos = GUILayoutUtility.GetLastRect().center;
                            EditorGUIUtility.RotateAroundPivot(90, pos);
                            CenterGUILabel(false, ((CharacterType)j).ToString());
                            EditorGUIUtility.RotateAroundPivot(-90, pos);
                        }
                    }
                    else if (j != -1 && j != baseClass.shinseiTypeMatrix[0].rows.Count)
                    {
                        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                        PaintButton(i, j);
                        EditorGUILayout.EndVertical();
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            targetSerial.ApplyModifiedProperties();
            EditorUtility.SetDirty(baseClass);
        }


        public void CenterGUILabel(bool isVertical, string labelText)
        {
            if (isVertical)
            {
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                GUILayout.Label(labelText, GUILayout.Width(BOX_SIZE * 2));
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(labelText, GUILayout.Width(BOX_SIZE), GUILayout.Height(BOX_SIZE * 4));
                GUILayout.EndHorizontal();
            }
        }

        private const int BOX_SIZE = 35;
        private void PaintButton(int i, int j)
        {
            Color newColor;
            float valueToAnalize = 1;

            try { valueToAnalize = baseClass.shinseiTypeMatrix[i].rows[j]; } catch { }
            if (valueToAnalize == 0)
                newColor = Color.grey;
            else if (valueToAnalize == 1)
                newColor = Color.white;
            else if (valueToAnalize > 1)
                newColor = Color.green;
            else
                newColor = Color.red;

            GUI.backgroundColor = newColor;
            try
            {
                string text = valueToAnalize.ToString();
                if (valueToAnalize == 1)
                    text = "-";
                else if (valueToAnalize == 0)
                    text = "NE";
                if (GUILayout.Button(text, GUILayout.Width(BOX_SIZE), GUILayout.Height(BOX_SIZE)))
                    currentValueWindow = ShinseiTypeSelectEffectValueEditor.Create(baseClass.shinseiTypeMatrix[i].rows[j], j < baseClass.shinseiTypeMatrix[0].rows.Count / 2, this, i, j);
            }
            catch { }
        }

        public void ChangeEffectValue(float value, int i, int j)
        {
            baseClass.shinseiTypeMatrix[i].rows[j] = value;
        }
        private ShinseiTypeSelectEffectValueEditor currentValueWindow;
    }

    [CustomEditor(typeof(ShinseiTypeScriptable))]
    public class ShinseiTypeSelectEffectValueEditor : EditorWindow
    {
        [Range(-10, 10)]
        public static float value;
        private static bool initializedPosition;
        private static bool windowOnRight;
        private static ShinseiTypeScriptableEditor scriptableEditor;
        private static Vector2Int indexs;

        public static ShinseiTypeSelectEffectValueEditor Create(float currentText, bool isOnLeftSide, ShinseiTypeScriptableEditor _scriptableEditor, int i, int j)
        {
            initializedPosition = false;
            value = currentText;
            windowOnRight = isOnLeftSide;
            scriptableEditor = _scriptableEditor;
            indexs = new Vector2Int(i, j);
            return GetWindow<ShinseiTypeSelectEffectValueEditor>(false, "Please type your value", true);
        }
        private const float windowWidth = 210;

        private void OnGUI()
        {
            if (!initializedPosition)
            {
                Vector2 mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                position = new Rect(windowOnRight ? mousePos.x : (mousePos.x - windowWidth), mousePos.y, windowWidth, 5);
                initializedPosition = true;
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            value = EditorGUILayout.FloatField("Effect value: ", value);
            scriptableEditor.ChangeEffectValue(value, indexs.x, indexs.y);
            EditorGUILayout.EndHorizontal();
        }
    }
#endif
}