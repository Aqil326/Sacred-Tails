using UnityEngine;

public class TestPlayfabFunction : MonoBehaviour
{
    #region ----Fields----
    #endregion ----Fields----

    #region ----Methods----	
    public void FucnButtonTest()
    {
        var request = new PlayFab.ClientModels.ExecuteCloudScriptRequest()
        {
            FunctionName = "GetPlayers",
            FunctionParameter = new { displayName = "xXx_Destroyer_xXx" }
        };

        PlayFab.PlayFabClientAPI.ExecuteCloudScript(request,
        (result) =>
        {
            Debug.Log(JsonUtility.ToJson(result.FunctionResult));
        },
        (error) =>
        {
            Debug.Log("error");
        });
    }
    #endregion ----Methods----	
}
