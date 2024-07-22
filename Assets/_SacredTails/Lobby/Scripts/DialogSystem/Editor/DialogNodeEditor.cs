using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace Timba.SacredTails.DialogSystem
{
    [CustomNodeEditor(typeof(DialogNode))]
    public class DialogNodeEditor : NodeEditor
    {
        public Vector2 ScrollPos;
        public override void OnBodyGUI()
        {
            serializedObject.Update();

            var dialogNode = serializedObject.targetObject as DialogNode;
            NodeEditorGUILayout.PortField(dialogNode.GetPort("input"));

            NodeEditorGUILayout.PortField(dialogNode.GetPort("output"));

            dialogNode.sequentialAnswers = GUILayout.Toggle(dialogNode.sequentialAnswers, "Sequential Answers");
            if (dialogNode.sequentialAnswers)
            {
                GUILayout.Label("Sequential Answer Key");
                dialogNode.sequentialCurrentAnswerKey = GUILayout.TextField(dialogNode.sequentialCurrentAnswerKey);
                dialogNode.randomAnswer = false;
            }
            else
                dialogNode.randomAnswer = GUILayout.Toggle(dialogNode.randomAnswer, "Random Answer");

            if (!dialogNode.randomAnswer && !dialogNode.sequentialAnswers)
            {
                GUILayout.Label("Dialog Text");
                ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos, GUILayout.Height(100));
                dialogNode.dialogText = GUILayout.TextArea(dialogNode.dialogText, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }

            NodeEditorGUILayout.DynamicPortList(
                    "Answers", // field name
                    typeof(string), // field type
                    serializedObject, // serializable object
                    NodePort.IO.Input, // new port i/o
                    Node.ConnectionType.Override, // new port connection type
                    Node.TypeConstraint.None,
                    OnCreateReorderableList); // onCreate override. This is where the magic 


            foreach (NodePort dynamicPort in target.DynamicPorts)
            {
                if (NodeEditorGUILayout.IsDynamicPortListPort(dynamicPort)) continue;
                NodeEditorGUILayout.PortField(dynamicPort);
            }

            serializedObject.ApplyModifiedProperties();
        }

        void OnCreateReorderableList(ReorderableList list)
        {
            list.elementHeightCallback = (index) =>
            {
                return 60;
            };

            // Override drawHeaderCallback to display node's name instead
            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var dialogNode = serializedObject.targetObject as DialogNode;

                NodePort port = dialogNode.GetPort("Answers " + index);

                dialogNode.Answers[index] = GUI.TextArea(rect, dialogNode.Answers[index]);

                if (port != null)
                {
                    Vector2 pos = rect.position + (port.IsOutput ? new Vector2(rect.width + 6, 0) : new Vector2(-36, 0));
                    NodeEditorGUILayout.PortField(pos, port);
                }
            };
        }
    }
}