using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NftsInfoCell : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private TextMeshProUGUI dnaText;
    [SerializeField]
    private TextMeshProUGUI descText;
    [SerializeField]
    private TextMeshProUGUI animationUrlText;
    [SerializeField]
    private TextMeshProUGUI AttributesText;

    public void SetInfo(string name, string dna, string desc, string animUrl, string Attrib)
    {
        nameText.text = "Name: " + name;
        dnaText.text = "Dna: " + dna;
        descText.text = "Description: " + desc;
        animationUrlText.text = "Animation Url: " + animUrl;
        AttributesText.text = "Attributes: " + Attrib;
    }

    // Start is called before the first frame update
    /*void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }*/
}
