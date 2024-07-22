using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class PartSelector : MonoBehaviour {
    public Button leftButton;
    public Button rigthButton;
    public TMP_Text selectorName;
    public TMP_Text currentSelectorPart;

    private PartType bodyPart;

    public PartType BodyPart => bodyPart;
    public void SetBodyPart(PartType _bodyPart) => bodyPart = _bodyPart;
}