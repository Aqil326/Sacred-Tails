using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoAlertPanel : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textInfo;
    //public int _damageAmount { get { return damageAmount; } set { damageAmount = value; } }

    [SerializeField]
    private Image alteredStateIcon;

    [SerializeField]
    private Sprite[] iconsAlteredState;

    private RectTransform rectTransform;

    private float speedDisplacement = 70f;
    private float speedFade = 50f;

    void Awake()
    {
        rectTransform = this.GetComponent<RectTransform>();

        //textoUI.text = "---";
    }

    public void ShowInfoPanel(RectTransform locLimitPosition, NotifyDamageInfo locNotifyDamageInfo, BattleNotificationSystem locBattleNotificationSystem, bool locIsPlayer)
    {
        bool showAlert = true;

        string auxPlayer = locIsPlayer ? "<color=#2FCC7B>[Player]</color>" : "<color=#F54F4F>[Enemy]</color>";
        if (locNotifyDamageInfo._alteredStateType == "")
        {

            if(locNotifyDamageInfo._damageAmount != 0)
            {
                textInfo.text = "-" + locNotifyDamageInfo._damageAmount + " Damage";

                //locBattleNotificationSystem.AddText($"Shinsei has healed " + locNotifyDamageInfo._damageAmount);
                locBattleNotificationSystem.AddText(auxPlayer + " takes " + "<color=#F54F4F>" + locNotifyDamageInfo._damageAmount + "</color>" + " direct " + " <color=#F54F4F>Damage</color>" + "." + GetTypeMultiplierText(locNotifyDamageInfo.multiplier));
            }
            else if (locNotifyDamageInfo._healingAmount != 0)
            {
                textInfo.text = "+" + locNotifyDamageInfo._healingAmount + " Healing";
                locBattleNotificationSystem.AddText(auxPlayer + " takes " + "<color=#2FCC7B>" + locNotifyDamageInfo._healingAmount + "</color>" + " <color=#2FCC7B>Healing</color>" + ".");
            }
            else
            {
                //textInfo.text = "no data";
                textInfo.text = "Dodge";
                locBattleNotificationSystem.AddText(auxPlayer + " " + "<color=#2FCC7B>Dodges</color>" + " attack.");
            }
        }
        else
        {
            switch (locNotifyDamageInfo._alteredStateType)
            {
                case "Ignited":
                    textInfo.text = "-" + locNotifyDamageInfo._damageAmount + " Ignited";
                    alteredStateIcon.sprite = iconsAlteredState[0];
                    locBattleNotificationSystem.AddText(auxPlayer + " is " + "<color=#F54F4F>Ignited</color>" + " takes " + "<color=#F54F4F>" + locNotifyDamageInfo._damageAmount + "</color>" + " " + "<color=#F54F4F>Damage</color>" + "." + GetTypeMultiplierText(locNotifyDamageInfo.multiplier));
                    break;
                case "Rooted":
                    textInfo.text = "-" + locNotifyDamageInfo._damageAmount + " Rooted";
                    alteredStateIcon.sprite = iconsAlteredState[1];
                    locBattleNotificationSystem.AddText(auxPlayer + " is " + "<color=#F54F4F>Rooted</color>" + " takes " + "<color=#F54F4F>" + locNotifyDamageInfo._damageAmount + "</color>" + " " + "<color=#F54F4F>Damage</color>" + "." + GetTypeMultiplierText(locNotifyDamageInfo.multiplier));
                    break;
                case "Bleeding":
                    textInfo.text = "-" + locNotifyDamageInfo._damageAmount + " Bleeding";
                    alteredStateIcon.sprite = iconsAlteredState[2];
                    locBattleNotificationSystem.AddText(auxPlayer + " is " + "<color=#F54F4F>Bleeding</color>" + " takes " + "<color=#F54F4F>" + locNotifyDamageInfo._damageAmount + "</color>" + " " + "<color=#F54F4F>Damage</color>" + "." + GetTypeMultiplierText(locNotifyDamageInfo.multiplier));
                    break;
                default:
                    showAlert = false;
                    textInfo.text = "no data"; //Player continues to have a higher evasion rate
                    //locBattleNotificationSystem.AddText(auxPlayer + " is " + "<color=#F54F4F>" + locNotifyDamageInfo._alteredStateType + "</color>" + " takes " + "<color=#F54F4F>" + locNotifyDamageInfo._damageAmount + "</color>" + " " + "<color=#F54F4F>No data</color>" + ".");
                    locBattleNotificationSystem.AddText(auxPlayer + " continues to have a higher " + "<color=#F54F4F>" + locNotifyDamageInfo._alteredStateType + "</color>" + " rate" + ".");
                    break;
            }
        }
        //textoUI.text = locText;

        if (showAlert)
        {
            StartCoroutine(UpdatePosition(locLimitPosition));
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private string GetTypeMultiplierText(float multiplier)
    {
        string effectivenessText = string.Empty;
        switch(multiplier)
        {
            case 0:
                effectivenessText = " <color=#F54F4F>Minimal Damage!</color>";
                break;
            case 0.5f:
                effectivenessText = " <color=#F54F4F>Moderate Damage!</color>";
                break;
            case 1.0f:
                effectivenessText = string.Empty;
                break;
            case 1.5f:
                effectivenessText = " <color=#2FCC7B>Good Hit!</color>";
                break;
            case 2.0f:
                effectivenessText = " <color=#2FCC7B>Great Hit!</color>";
                break;
        }
        return effectivenessText;
    }

    /*
     public enum AlteredStateEnum
    {
        //EvasionChange, -> 1_39 (Temporal)
        Ignited, -> 1_112
        Rooted, -> 1_113
        Bleeding -> 11_114
    }

    dormir: 1_109
    Aturdir: 1_110
    Electricidad: 1_111
     */

    private IEnumerator UpdatePosition(RectTransform locLimitPosition)
    {
        //yield return new WaitForSeconds(10);
        // Obtener la posición inicial del texto
        Vector3 posicionInicial = rectTransform.position;

        // Calcular la posición final hacia arriba
        //Vector3 posicionFinal = posicionInicial + Vector3.up * 100f; // Ajusta el valor 100f según sea necesario

        // Desplazamiento del texto hacia arriba
        while (rectTransform.localPosition.y < locLimitPosition.localPosition.y)
        {
            // Calcular el desplazamiento hacia arriba
            float desplazamiento = speedDisplacement * Time.deltaTime;

            // Actualizar la posición del texto
            rectTransform.localPosition += Vector3.up * desplazamiento;

            // Desvanecer gradualmente el texto
            /*Color colorTexto = textoUI.color;
            colorTexto.a -= speedFade * Time.deltaTime;
            textoUI.color = colorTexto;*/

            yield return null;
        }

        // Desactivar el texto después de que se complete el desplazamiento
        //textoUI.gameObject.SetActive(false);
        Destroy(this.gameObject);
    }
}
