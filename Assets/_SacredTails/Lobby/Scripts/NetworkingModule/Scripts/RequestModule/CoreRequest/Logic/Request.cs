using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CoreRequestManager
{
    public class Request : MonoBehaviour
    {
        #region Instance
        public static Request instance;
        public UnityNewtonsoftJsonSerializer Serializer = new UnityNewtonsoftJsonSerializer();

        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            GameObject go = new GameObject();
            go.name = "FirebasRequestHandler";
            Request fbRequest = (Request)go.AddComponent(typeof(Request));
            //go.AddComponent(typeof(FirebaseTokenManager));
            instance = fbRequest;
            DontDestroyOnLoad(fbRequest);
        }
        #endregion Instance

        #region  Methods
        public void RequestPetiton<T>(string _url, RequestType _type = RequestType.GET, object _payload = null, Action<SacredTailsPSDto<T>> _callback = null)
        {
            string data = "";
            if (_payload != null)
            {
                if (_payload.GetType() == typeof(string))
                    data = (string)_payload;
                else
                    data = UnityNewtonsoftJsonSerializer.Serialize(_payload);

                if (_type == RequestType.GET || _type == RequestType.DELETE)
                    _url += SetGetParameters(data);
            }

            StartCoroutine(WebRequestObject(_url, _type, data, _callback));
        }

        IEnumerator WebRequestObject<T>(string _url, RequestType _type, string data, Action<SacredTailsPSDto<T>> _callback)
        {
            UnityWebRequest request;
            switch (_type)
            {
                case RequestType.GET:
                    request = UnityWebRequest.Get(_url);
                    break;
                case RequestType.POST:
                    request = UnityWebRequest.Post(_url, "POST");
                    request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data));
                    break;
                case RequestType.PUT:
                    request = UnityWebRequest.Put(_url, data);
                    break;
                case RequestType.PATCH:
                    request = UnityWebRequest.Put(_url, data);
                    request.method = "PATCH";
                    break;
                case RequestType.DELETE:
                    request = UnityWebRequest.Delete(_url);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    break;
                default:
                    request = UnityWebRequest.Get(_url);
                    break;
            }
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                SacredTailsLog.LogMessage(request.error);
            }
            else
            {
                string result = "";
                if (request.downloadHandler != null)
                {
                    result = request.downloadHandler.text;
                    result = result.Replace("\\", string.Empty);
                }

                SacredTailsPSDto<T> responseR = default;
                if (!string.IsNullOrEmpty(result) && result != "null")
                    try
                    {
                        responseR = UnityNewtonsoftJsonSerializer.Deserialize<SacredTailsPSDto<T>>(result);
                    }
                    catch {
                        SacredTailsLog.LogMessage("tellmeWHY!");
                    }

                if (_callback != null)
                    _callback?.Invoke(responseR);
            }
        }
        #region Helpers
        private string SetGetParameters(string json)
        {
            string paramsUrl = "";
            foreach (KeyValuePair<string, string> entry in JsonConvert.DeserializeObject<Dictionary<string, string>>(json))
            {
                string finalValue = entry.Value;
                if (finalValue == null)
                    continue;
                if (paramsUrl.Length > 0)
                    paramsUrl += "&";
                if (finalValue.Contains("&"))
                    finalValue = finalValue.Replace("&", "%26");
                if (finalValue.Contains("+"))
                    finalValue = finalValue.Replace("+", "%2B");
                paramsUrl += $"{entry.Key}={finalValue}";
            }
            if (!String.IsNullOrEmpty(paramsUrl))
                return $"?{paramsUrl}";
            else
                return "";
        }
        #endregion Helpers
        #endregion  Methods
    }
}
