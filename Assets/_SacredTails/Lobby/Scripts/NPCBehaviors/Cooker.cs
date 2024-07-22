using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Timba.Games.CharacterFactory;
using Timba.Games.SacredTails.Lobby;
using Timba.Games.SacredTails.LobbyNetworking;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.Lobby;
using Timba.SacredTails.UiHelpers;
using UnityEngine;

/// <summary>
/// This script controls the behavior of npc cooker in the bar
/// </summary>
public class Cooker : MonoBehaviour
{
    [SerializeField] private Animator NPCAnimator;
    [SerializeField] private GameObject pan, dish, handDish, beer, handBeer;
    [SerializeField] private float cookingTime, beerServingTime, foodServingTime, afterServing, TakeBeer, EndBeer, TakeTime;
    [SerializeField] private Transform targetPosition;
    Camera mainCamera;
    public LayerMask layerMaskForCook;
    int foodHash;

    private void Start()
    {
        foodHash = Animator.StringToHash("Food");
        mainCamera = Camera.main;
    }

    /// <summary>
    /// Playback the cooking animations
    /// </summary>
    /// <param name="type"></param>
    [ContextMenu("PlayFoodAnimations")]
    public void GetFood(int type)
    {
        StartCoroutine(CookingFood(type));
    }

    IEnumerator CookingFood(int type)
    {
        yield return null;
        Show();
        UIGroups.instance.ShowOnlyThisGroup("Nan");
        currentPlayer.navmeshAgent.Warp(targetPosition.transform.position);
        currentPlayer.transform.rotation = targetPosition.rotation;
        if (type == 0)
        {
            NPCAnimator.SetBool(foodHash, true);
            pan.SetActive(true);
            yield return new WaitForSeconds(cookingTime);
            pan.SetActive(false);
            handDish.SetActive(true);
            yield return new WaitForSeconds(foodServingTime);
        }
        else if (type == 1)
        {
            NPCAnimator.SetBool("Beer", true);
            handBeer.SetActive(true);
            yield return new WaitForSeconds(beerServingTime);
        }
        NPCAnimator.SetBool(foodHash, false);
        NPCAnimator.SetBool("Beer", false);
        handDish.SetActive(false);
        handBeer.SetActive(false);
        if (type == 0)
        {
            dish.SetActive(true);
            foreach (var animators in currentPlayer.animator)
                animators.SetBool("Eat", true);
        }
        else if (type == 1)
        {
            beer.SetActive(true);
            foreach (var animators in currentPlayer.animator)
                animators.SetBool("Drink", true);
            yield return new WaitForSeconds(TakeBeer);
            beer.SetActive(false);
            yield return new WaitForSeconds(EndBeer);
            beer.SetActive(true);
            NPCAnimator.SetTrigger("Take");
            yield return new WaitForSeconds(TakeTime);
            beer.SetActive(false);
            handBeer.SetActive(true);
        }
        yield return new WaitForSeconds(afterServing);
        handDish.SetActive(false);
        handBeer.SetActive(false);
        yield return new WaitForSeconds(3);
        if (type == 0)
            foreach (var animators in currentPlayer.animator)
                animators.SetBool("Eat", false);
        if (type == 1)
            foreach (var animators in currentPlayer.animator)
                animators.SetBool("Drink", false);
        dish.SetActive(false);
        Hide();
    }

    ThirdPersonController currentPlayer;
    ShinseiMovement shinseiMovement;
    CharacterSlot characterSlot;
    [SerializeField] CinemachineVirtualCamera lobbyCamera, cookingCamera;
    /// <summary>
    ///     Set camera setting for cooking event
    /// </summary>
    public void Show()
    {
        if (currentPlayer == null)
        {
            currentPlayer = ServiceLocator.Instance.GetService<ILobbyNetworkManager>().CurrentPlayer;
        }
        //UIGroups.instance.ShowOnlyThisGroup("pet");
        lobbyCamera.Priority = 0;
        cookingCamera.Priority = 1;
        mainCamera.cullingMask = layerMaskForCook;
        currentPlayer.IsMovementBloqued = true;
    }

    /// <summary>
    ///     Put camera settings to normal values
    /// </summary>
    public void Hide()
    {
        cookingCamera.Priority = 0;
        lobbyCamera.Priority = 1;
        UIGroups.instance.ShowOnlyThisGroup("planner");
        mainCamera.cullingMask = -1;
        currentPlayer.IsMovementBloqued = false;
    }
}
