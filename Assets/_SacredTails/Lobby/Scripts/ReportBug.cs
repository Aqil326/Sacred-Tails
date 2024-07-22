using Newtonsoft.Json;
using PlayFab;
using PlayFab.CloudScriptModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;

public class ReportBug : MonoBehaviour
{
    [Header("UI")]

    [SerializeField] TMP_InputField inputField;

    [SerializeField] bool debugRequest = true;
    [SerializeField] bool debugResult = true;
    [SerializeField] bool debugError = true;
    [SerializeField] UnityEvent GoodCallback, BadCallback;

    [SerializeField] Texture2D texture2D;
    [SerializeField] CanvasGroup panelCanvasGroup;
    Coroutine requestRoutine;
    public void SendBug()
    {
        if (requestRoutine != null)
            StopCoroutine(requestRoutine);

        requestRoutine = StartCoroutine(TournamentRequest());
    }
    public IEnumerator TournamentRequest()
    {
        string cloudFunctionName = "ReportBug";

        GoodCallback.Invoke();
        panelCanvasGroup.alpha = 0;
        yield return new WaitForSeconds(0.1f);
        ScreenCapture.CaptureScreenshot(Application.persistentDataPath + "BugImage");
        yield return new WaitForSeconds(0.1f);
        panelCanvasGroup.alpha = 1;
        yield return null;
        if (!File.Exists(Application.persistentDataPath + "BugImage"))
        {
            yield break;
        }

        var www = UnityWebRequestTexture.GetTexture("file://" + Application.persistentDataPath + "BugImage");
        yield return www.SendWebRequest();
        var texture = DownloadHandlerTexture.GetContent(www);


        //Upload to imgur
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", texture.EncodeToPNG());
        www = UnityWebRequest.Post("https://api.imgur.com/3/upload", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            SacredTailsLog.LogMessage(www.error);
            yield break;
        }
        else
        {
            SacredTailsLog.LogMessage("Form upload complete!");
        }
        object funcParams;
        funcParams = new
        {
            Keys = new
            {
                userName = PlayerDataManager.Singleton.localPlayerData.playerName,
                picture = JsonConvert.DeserializeObject<DtoBugData>(www.downloadHandler.text).data.link,
                message = inputField.text,
                matchId = PlayerDataManager.Singleton.localPlayerData.currentMatchId,
                tournamentId = PlayerDataManager.Singleton.currentTournamentId,
            }
        };

        /*string data = JsonConvert.SerializeObject(texture.EncodeToPNG());
        byte[] array = JsonConvert.DeserializeObject<byte[]>(data);
        texture2D = new Texture2D(1920,1080);
        texture2D.LoadImage(array);*/

        var req = new ExecuteFunctionRequest()
        {
            FunctionName = cloudFunctionName,
            FunctionParameter = funcParams
        };
        if (debugRequest)
            SacredTailsLog.LogMessage($"<color=cyan>Request of <Bug></color>: {JsonConvert.SerializeObject(req, Formatting.Indented)}");
        PlayFabCloudScriptAPI.ExecuteFunction(req, (res) =>
        {
            if (debugResult)
                SacredTailsLog.LogMessage($"<color=orange>Response of <Bug></color>: {JsonConvert.SerializeObject(res, Formatting.Indented)}");
            GoodCallback.Invoke();
            requestRoutine = null;
        }, (err) =>
        {
            if (debugError)
                SacredTailsLog.LogErrorMessage($"Any was wrong <Bug>:" + err);
            BadCallback.Invoke();
            requestRoutine = null;
        });
    }
}

public struct DtoBugData
{
    public int status;
    public bool success;
    public DtoDetailBugData data;
}

public struct DtoDetailBugData
{
    public string link;
}
