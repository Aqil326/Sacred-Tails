using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Timba.SacredTails.TournamentBehavior
{
    public class SingleBracketPlayerLabelController : MonoBehaviour
    {
        [Header("Text")]
        public TMP_Text playerDisplayName;

        [Header("Icons")]
        public Image shinseiIcon;
        public Image vsImage;

        [Header("LabelFrame")]
        public Image labelFrameReference;
        public Sprite labelFrameWhite;

        [Header("bracketsLines")]
        public Sprite bracketLineGlowin;
        public Image bracketLineBack;
        public Image bracketLineFront;
        public Image bracketLineUp;
        public Image bracketLineDown;

        // Start is called before the first frame update
        public void InitPlayerLabel(bool isInitBracket, bool isWinnerBracket, bool isOddNumber, string playerName, int currentStage, int totalStages, bool isWinnerOfPreviousStage, bool isAnExistingStage, bool isFinalBracket = false)
        {
            vsImage.gameObject.SetActive(isOddNumber);
            bracketLineFront.gameObject.SetActive(!isWinnerBracket);
            //If is not left column
            if (!isInitBracket)
            {
                //is the player below on the bracket
                if (isOddNumber)
                {
                    var tempAnchoredPos = vsImage.rectTransform.anchoredPosition;
                    vsImage.rectTransform.anchoredPosition = new Vector2(tempAnchoredPos.x, 50 * Mathf.Pow(2, currentStage));
                }

                bracketLineBack.gameObject.SetActive(true);
                //Is an stage with data
                if (isAnExistingStage)
                {
                    bracketLineBack.sprite = bracketLineGlowin;
                    bracketLineBack.rectTransform.sizeDelta = new Vector2(bracketLineBack.rectTransform.sizeDelta.x, 50);
                    //bracketLineBack.transform.localScale = new Vector3(bracketLineBack.transform.localScale.x * 1.16f, 1, 1);
                }
            }

            //Check Lines up and down
            if (!isWinnerBracket)
            {
                float newSizeDown = Mathf.Max(1, bracketLineDown.transform.localScale.x * Mathf.Pow(2, currentStage));
                bracketLineDown.gameObject.SetActive(!isOddNumber);
                bracketLineDown.transform.localScale = new Vector3(newSizeDown, 1, 1);

                float newSizeUp = Mathf.Max(1, bracketLineUp.transform.localScale.x * Mathf.Pow(2, currentStage));
                bracketLineUp.gameObject.SetActive(isOddNumber);
                bracketLineUp.transform.localScale = new Vector3(newSizeUp, 1, 1);
            }

            playerDisplayName.text = playerName;

            //Paint alternative colors
            if (isWinnerOfPreviousStage)
            {
                shinseiIcon.color = Color.black;
                playerDisplayName.color = Color.black;

                labelFrameReference.sprite = labelFrameWhite;

                bracketLineFront.sprite = bracketLineGlowin;
                bracketLineFront.rectTransform.sizeDelta = new Vector2(bracketLineFront.rectTransform.sizeDelta.x, 50);
                bracketLineFront.transform.localScale = new Vector3(bracketLineFront.transform.localScale.x * 1.13f, 1, 1);

                bracketLineDown.sprite = bracketLineGlowin;
                bracketLineDown.rectTransform.sizeDelta = new Vector2(bracketLineDown.rectTransform.sizeDelta.x, 50);
                bracketLineDown.transform.localScale = new Vector3(bracketLineDown.transform.localScale.x * 1.33f, 1, 1);


                bracketLineUp.sprite = bracketLineGlowin;
                bracketLineUp.rectTransform.sizeDelta = new Vector2(bracketLineUp.rectTransform.sizeDelta.x, 50);
                bracketLineUp.transform.localScale = new Vector3(bracketLineUp.transform.localScale.x * 1.33f, 1, 1);
            }

            if(isFinalBracket)
            {
                bracketLineFront.gameObject.SetActive(false);
                bracketLineDown.gameObject.SetActive(false);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}