using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace FirebaseRequestManager
{
    public class FirebaseTokenManager : MonoBehaviour
    {
        #region ----Fields----
        public string tokenFirebase = "";
        private int tokenExpirationTime = 3600;
        private float initTimeTokenFirebase;

        private string encryptKey = "";
        private string uri = "";

        private bool intialized;
        public static FirebaseTokenManager instance;
        #endregion ----Fields----

        #region ----Methods----
        private void Awake()
        {
            //Singleton
            if (instance != null)
                Destroy(this.gameObject);

            instance = this;
            DontDestroyOnLoad(this);
        }

        public void Init()
        {
            intialized = true;
            initTimeTokenFirebase = Time.time;
            StartCoroutine(RequestTokenFirebase());
        }

        public void StopTokenManager()
        {
            intialized = false;
        }

        private void Update()
        {
            if (intialized && Time.time > initTimeTokenFirebase + tokenExpirationTime)
            {
                StartCoroutine(RequestTokenFirebase());
                initTimeTokenFirebase = Time.time;
            }
        }

        IEnumerator RequestTokenFirebase()
        {
            ReadString();

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                webRequest.SetRequestHeader("Access-Control-Allow-Credentials", "true");
                webRequest.SetRequestHeader("Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
                webRequest.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                webRequest.SetRequestHeader("Access-Control-Allow-Origin", "*");
                webRequest.SetRequestHeader("X-Requested-With", "https://arjs-cors-proxy.herokuapp.com/");
                yield return webRequest.SendWebRequest();
                if (webRequest.result == UnityWebRequest.Result.ConnectionError)
                {
                    SacredTailsLog.LogMessage(webRequest.error);
                }
                else
                {
                    if (!String.IsNullOrEmpty(webRequest.downloadHandler.text))
                        tokenFirebase = DecryptStringWithXORFromHex(webRequest.downloadHandler.text.Replace("\"", ""), encryptKey);
                    else
                        StartCoroutine(RequestTokenFirebase());
                }
            }
        }
        public void ReadString()
        {
            string path = "Assets/Plugins/Config/FirebaseModule.cfg";
            if (!File.Exists(path))
            {
                Directory.CreateDirectory("Assets/Plugins");
                Directory.CreateDirectory("Assets/Plugins/Config");
                File.WriteAllText(path, "key:<Put key here>;uri:<Put uri of middle server here>");
                SacredTailsLog.LogMessage($"<color=red>Please fill your config file in: </color> {path}");
            }

            StreamReader reader = new StreamReader(path);
            string[] config = reader.ReadToEnd().Split(';');

            encryptKey = config[0].Remove(0, 4);
            uri = config[1].Remove(0, 4);

            if (String.IsNullOrEmpty(encryptKey) || String.IsNullOrEmpty(uri))
                SacredTailsLog.LogMessage($"<color=red>Please fill your config file in: </color> {path}");

            reader.Close();
        }

        private string DecryptStringWithXORFromHex(string input, string key)
        {
            StringBuilder c = new StringBuilder();
            try
            {
                while ((key.Length < (input.Length / 2)))
                    key += key;

                for (int i = 0; i < input.Length; i += 2)
                {
                    string hexValueString = input.Substring(i, 2);
                    int value1 = Convert.ToByte(hexValueString, 16);
                    int value2 = key[i / 2];
                    int xorValue = (value1 ^ value2);
                    c.Append(Char.ToString(((char)(xorValue))));
                }
            }
            catch (Exception ex)
            {
                StartCoroutine(RequestTokenFirebase());
                return "";
            }

            return c.ToString();
        }
        #endregion ----Methods----
    }
}