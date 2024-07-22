using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InteractWithShinsei : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] PetInteraction petInteraction;

    private void Start()
    {
        petInteraction = FindObjectOfType<PetInteraction>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        petInteraction.Show();
    }
}
