using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Timba.SacredTails.UiHelpers
{
    public class EntryController : MonoBehaviour
    {
        public TMP_Text qualificator;
        public Image nft_image;
        public TMP_Text nft_amount;
        public Image sc_image;
        public TMP_Text sc_amount;

        public void FillData(RankRewardEntry data)
        {
            if (data.position != null)
                qualificator.text = data.position.Value.ToString();
            else
                qualificator.text = (int)(data.thresholdDown * 100) + "% - " + (int)(data.thresholdUp * 100) + "%";

            if (data.rewards[0].type.Equals("SC"))
            {
                nft_image.gameObject.SetActive(false);
                nft_amount.gameObject.SetActive(false);

                sc_image.gameObject.SetActive(true);
                sc_amount.gameObject.SetActive(true);
                nft_amount.text = data.rewards[0].amount.ToString();
            }
            else
            {
                nft_image.gameObject.SetActive(true);
                nft_amount.gameObject.SetActive(true);
                nft_amount.text = data.rewards[0].amount.ToString();

                sc_image.gameObject.SetActive(true);
                sc_amount.gameObject.SetActive(true);
                sc_amount.text = data.rewards[1].amount.ToString();
            }
        }
    }
}