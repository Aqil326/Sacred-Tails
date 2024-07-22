using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoContainer : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    [SerializeField]
    private RectTransform initPosition;
    [SerializeField]
    private RectTransform limitPosition;

    [SerializeField]
    private GameObject infoPanelAlteredStatePrefab;
    [SerializeField]
    private GameObject infoPanelDamagePrefab;
    [SerializeField]
    private GameObject infoPanelHealingPrefab;

    private bool enableContainer = true;

    private List<NotifyDamageInfo> notifyDamageInfo = new List<NotifyDamageInfo>();

    /*void Start()
    {
        ShowInfoText();
    }*/

    public void ShowInfoText(List<NotifyDamageInfo> locNotifyDamageInfo, BattleNotificationSystem locBattleNotificationSystem, bool locIsPlayer)
    {
        foreach (NotifyDamageInfo aux in locNotifyDamageInfo)
        {
            notifyDamageInfo.Add(aux);
        }

        StartCoroutine(ShowNotifications(locNotifyDamageInfo, locBattleNotificationSystem, locIsPlayer));
    }

    private IEnumerator ShowNotifications(List<NotifyDamageInfo> locNotifyDamageInfo, BattleNotificationSystem locBattleNotificationSystem, bool locIsPlayer)
    {
        float auxIsMultipleNotify = locNotifyDamageInfo.Count > 1 ? 0.5f : 0;

        foreach (NotifyDamageInfo aux in locNotifyDamageInfo)
        {
            GameObject infoPanel;

            if (aux._alteredStateType == "")
            {
                if(aux._damageAmount != 0)
                {
                    infoPanel = Instantiate(infoPanelDamagePrefab, this.transform);
                }
                else
                {
                    infoPanel = Instantiate(infoPanelHealingPrefab, this.transform);
                }
            }
            else
            {
                infoPanel = Instantiate(infoPanelAlteredStatePrefab, this.transform);
            }
            
            infoPanel.transform.localPosition = initPosition.localPosition;

            infoPanel.GetComponent<InfoAlertPanel>().ShowInfoPanel(limitPosition, aux, locBattleNotificationSystem, locIsPlayer);

            yield return new WaitForSeconds(auxIsMultipleNotify);
        }

        yield return null;
    }

    /*private IEnumerator ShowNotifications(List<NotifyDamageInfo> locNotifyDamageInfo)
    {
        foreach (NotifyDamageInfo aux in locNotifyDamageInfo)
        {
            GameObject infoText = Instantiate(infoTextPrefab, this.transform);
            infoText.transform.localPosition = initPosition.localPosition;

            if (aux._damageAmount != 0)
            {
                //infoText.GetComponent<InfoTextDamage>().ShowInfoText(limitPosition, "-" + aux._damageAmount.ToString() + " " + aux._damageType + " Damage");
                infoText.GetComponent<InfoTextDamage>().ShowInfoText(limitPosition, "-" + aux._damageAmount.ToString() + " " + aux._damageType + " Damage");
            }
            else if (aux._healingAmount != 0)
            {
                //infoText.GetComponent<InfoTextDamage>().ShowInfoText(limitPosition, "+" + aux._damageAmount.ToString() + " " + aux._damageType + " Healing");
                infoText.GetComponent<InfoTextDamage>().ShowInfoText(limitPosition, "+" + aux._damageAmount.ToString() + " " + aux._damageType + " Healing");
            }

            yield return new WaitForSeconds(0.5f);
        }

        yield return null;
    }*/

    // Update is called once per frame
    void Update()
    {
        //if (transform != null)
        if (enableContainer)
        {
            transform.position = Camera.main.WorldToScreenPoint(target.position + offset);
        }
    }
}
