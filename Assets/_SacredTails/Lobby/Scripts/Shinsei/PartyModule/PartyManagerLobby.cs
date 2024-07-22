using System.Collections;
using UnityEngine;
using DG.Tweening;
using Timba.SacredTails.Arena;
using System;

namespace Timba.Games.SacredTails.Lobby
{
    public class PartyManagerLobby : PartyManager
    {
        public override void Initialize(Action<int, ShinseiSlot> onNewSlotCreated = null)
        {
            selectorPos = 1;
            base.Initialize((index, NewSlot) =>
            {
                NewSlot.isPreviewOnly = true;
                if (!NewSlot.IsCompanion)
                    NewSlot.transform.localScale = new Vector3(0.70f, 0.70f);
            });
            shinseiSlots[shinseiSlots.Count - 1].gameObject.transform.SetSiblingIndex(0);
            shinseiSlots[0].gameObject.transform.SetSiblingIndex(1);
        }

        public override void OnClickSlot(int listIndex, ShinseiSlot eventShinseiSlot)
        {
            base.OnClickSlot(listIndex, eventShinseiSlot);
            shinseiSlotCompanion.gameObject.transform.SetSiblingIndex(1);
        }

        public void SwapShinseiBtn(int dir)
        {
            foreach (var item in shinseiSlots)
                item.transform.DOScale(0.70f, 0.25f);

            if (dir > 0)
            {
                var slot = CompanionSelectionPanel.GetChild(shinseiSlots.Count - 1);
                slot.gameObject.transform.SetSiblingIndex(0);
            }
            else
            {
                var slot = CompanionSelectionPanel.GetChild(0);
                slot.gameObject.transform.SetSiblingIndex(shinseiSlots.Count - 1);
            }
            OnClickSlot(CompanionSelectionPanel.GetChild(1).GetComponent<ShinseiSlot>().listIndex, CompanionSelectionPanel.GetChild(1).GetComponent<ShinseiSlot>());
            StartCoroutine(SwipeSnapSequence());
        }

        IEnumerator SwipeSnapSequence()
        {
            yield return new WaitForSeconds(0.25f);
            CompanionSelectionPanel.GetChild(1).transform.DOScale(1f, 0.25f);
        }

    }
}