using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoTextDamage : MonoBehaviour
{
    //[SerializeField]
    private TextMeshProUGUI textoUI;
    // Start is called before the first frame update

    private float speedDisplacement = 20f;
    private float speedFade = 50f;

    void Awake()
    {
        textoUI = this.GetComponent<TextMeshProUGUI>();

        //textoUI.text = "---";
    }

    void Start()
    {
        //ShowInfoText();
    }

    public void ShowInfoText(RectTransform locLimitPosition, string locText)
    {
        textoUI.text = locText;

        StartCoroutine(UpdatePosition(locLimitPosition));
    }

    private IEnumerator UpdatePosition(RectTransform locLimitPosition)
    {
        //yield return new WaitForSeconds(10);
        // Obtener la posición inicial del texto
        Vector3 posicionInicial = textoUI.rectTransform.position;

        // Calcular la posición final hacia arriba
        //Vector3 posicionFinal = posicionInicial + Vector3.up * 100f; // Ajusta el valor 100f según sea necesario

        // Desplazamiento del texto hacia arriba
        while (textoUI.rectTransform.localPosition.y < locLimitPosition.localPosition.y)
        {
            // Calcular el desplazamiento hacia arriba
            float desplazamiento = speedDisplacement * Time.deltaTime;

            // Actualizar la posición del texto
            textoUI.rectTransform.localPosition += Vector3.up * desplazamiento;

            // Desvanecer gradualmente el texto
            /*Color colorTexto = textoUI.color;
            colorTexto.a -= speedFade * Time.deltaTime;
            textoUI.color = colorTexto;*/

            yield return null;
        }

        // Desactivar el texto después de que se complete el desplazamiento
        //textoUI.gameObject.SetActive(false);
        Destroy(textoUI.gameObject);
    }
}
