using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace CoreRequestManager
{

    public class SacredTailsPSDto<T>
    {
        public bool success;
        public int code;
        public string message;
        public T data;
    }
}
