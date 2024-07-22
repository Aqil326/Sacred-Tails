using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace Timba.SacredTails.DialogSystem
{
    public class DialogNode : Node
    {

        public struct Connection { }

        [Input] public Connection input;
        [Output] public Connection output;
        public string dialogText;
        public bool randomAnswer;

        public bool sequentialAnswers;
        public string sequentialCurrentAnswerKey;
        [Output(dynamicPortList = true)] public List<string> Answers;
    }
}