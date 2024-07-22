using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace Timba.SacredTails.DialogSystem
{
    public class ActionNode : Node
    {

        public struct Connection { }

        [Input] public Connection input;
        [Output] public Connection output;
        public string dialogText;
        public Action callback;
    }
}