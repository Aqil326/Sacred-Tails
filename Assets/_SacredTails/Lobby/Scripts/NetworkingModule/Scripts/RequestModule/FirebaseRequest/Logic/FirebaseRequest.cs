using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace FirebaseRequestManager
{
    public class FirebaseRequest : MonoBehaviour
    {
        #region Instance
        public static FirebaseRequest instance;
        public UnityNewtonsoftJsonSerializer Serializer = new UnityNewtonsoftJsonSerializer();

        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            GameObject go = new GameObject();
            go.name = "FirebasRequestHandler";
            FirebaseRequest fbRequest = (FirebaseRequest)go.AddComponent(typeof(FirebaseRequest));
            go.AddComponent(typeof(FirebaseTokenManager));
            instance = fbRequest;
            DontDestroyOnLoad(fbRequest);
        }
        #endregion Instance

        #region  Methods
        #region SingleObject Request 
        public void FirebaseObjectRequestPetiton<T>(string _url, bool plusJsonExtension = true, string authToken = null, object _payload = null, Action<bool, T> _callback = null, RequestType _type = RequestType.GET)
        {
            string data = null;
            if (_payload != null)
            {
                data = "";
                if (_payload.GetType() == typeof(string))
                    data = (string)_payload;
                else
                    data = UnityNewtonsoftJsonSerializer.Serialize(_payload);

                if (plusJsonExtension)
                    _url += ".json";
                if (authToken != null)
                    _url += $"?auth={authToken}";

                if (_type == RequestType.GET)
                    _url += SetGetParameters(data);
            }
            else
            {
                _url += ".json";
                if (authToken != null)
                    _url += $"?auth={authToken}";
            }

            StartCoroutine(WebRequestObject(_url, _type, data, _callback));
        }

        IEnumerator WebRequestObject<T>(string _url, RequestType _type, string data, Action<bool, T> _callback)
        {
            UnityWebRequest request;
            switch (_type)
            {
                case RequestType.GET:
                    request = UnityWebRequest.Get(_url);
                    break;
                case RequestType.POST:
                    request = UnityWebRequest.Post(_url, data);
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
                    break;
                default:
                    request = UnityWebRequest.Get(_url);
                    break;
            }

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

                T responseR = default;
                if (!string.IsNullOrEmpty(result) && result != "null")
                    responseR = UnityNewtonsoftJsonSerializer.Deserialize<T>(result);

                if (_callback != null)
                {
                    if (IsAnyNotNullOrEmpty(responseR))
                        _callback(responseR != null, responseR);
                    else
                        _callback(false, default);
                }
            }
        }
        #endregion SingleObject Request 

        #region List Request Petiton
        public void FirebaseDictionaryRequestPetiton<T>(string _url, bool plusJsonExtension = true, string _patchItemKey = null, string authToken = null, object _payload = null, Action<bool, FirebaseDictionaryDto<T>> _callback = null, RequestType _type = RequestType.GET)
        {
            string data = null;
            if (_payload != null)
            {
                data = "";
                if (_payload.GetType() == typeof(string))
                    data = (string)_payload;
                else
                    data = UnityNewtonsoftJsonSerializer.Serialize(_payload);

                //Set URL
                if (_patchItemKey != null)
                    _url += $"/List/{_patchItemKey}";

                if (_type == RequestType.POST)
                    _url += $"/List";

                if (plusJsonExtension)
                    _url += "/.json";

                //Set AuthToken
                if (authToken != null)
                    _url += $"?auth={authToken}";

                //Set GetParameters
                if (_type == RequestType.GET)
                    _url += SetGetParameters(data);
            }
            else
            {
                _url += "/.json";
                if (authToken != null)
                    _url += $"?auth={authToken}";
            }

            StartCoroutine(WebRequestDictionary(_url, _patchItemKey, _type, data, _callback, _payload?.GetType()));
        }

        IEnumerator WebRequestDictionary<T>(string _url, string _patchItemKey, RequestType _type, string data, Action<bool, FirebaseDictionaryDto<T>> _callback, Type _payloadType)
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
                if (request.downloadHandler != null)
                {
                    string result = request.downloadHandler.text;
                    result = result.Replace("\\", string.Empty);

                    FirebaseDictionaryDto<T> responseR = default;
                    if (_type == RequestType.PATCH)
                    {
                        T patchedItem = JsonConvert.DeserializeObject<T>(result);
                        responseR = new FirebaseDictionaryDto<T>();
                        responseR.List.Add(_patchItemKey, patchedItem);
                    }
                    else if (!string.IsNullOrEmpty(result) && result != "null")
                        responseR = UnityNewtonsoftJsonSerializer.Deserialize<FirebaseDictionaryDto<T>>(result);

                    if (_callback != null)
                    {
                        if (IsAnyNotNullOrEmpty(responseR))
                            _callback(responseR != null, responseR);
                        else
                            _callback(false, default);
                    }
                }
            }
        }
        #endregion List Request Petiton

        #region Helpers
        private string SetGetParameters(string json)
        {
            string paramsUrl = "";
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            foreach (KeyValuePair<string, object> entry in dictionary)
            {
                if (paramsUrl.Length > 0)
                    paramsUrl += "&";
                paramsUrl += $"{entry.Key}={entry.Value}";
            }
            return $"?{paramsUrl}";
        }

        private bool IsAnyNotNullOrEmpty(object myObject)
        {
            if (myObject == null)
                return true;
            bool anyParamNotNull = true;
            foreach (FieldInfo pi in myObject.GetType().GetRuntimeFields())
            {
                if (pi.GetValue(myObject) == null)
                {
                    anyParamNotNull = false;
                }
            }
            return anyParamNotNull;
        }
        #endregion Helpers
        #endregion  Methods
    }

    public class FirebaseDictionaryDto<T>
    {
        public Dictionary<string, T> List = new Dictionary<string, T>();
    }
}
