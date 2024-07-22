using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractWithPlayerController : MonoBehaviour
{
    private Quaternion lockedRotation;
    public GameObject interactionsPanel;

    void Start()
    {
        lockedRotation = transform.rotation;
    }

    void Update()
    {
        transform.rotation = lockedRotation;
    }
    public void ActivatePieMenu()
    {
        //A pretty animation releasing the dropdown can be done, but i'm not doing it right now :v
        interactionsPanel.SetActive(!interactionsPanel.activeInHierarchy);
    }
}
