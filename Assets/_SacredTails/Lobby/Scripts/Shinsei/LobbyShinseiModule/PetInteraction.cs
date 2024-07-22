using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Timba.Games.SacredTails.LobbyNetworking;
using Timba.SacredTails.Lobby;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.UiHelpers;
using Timba.Games.CharacterFactory;
using Timba.Games.SacredTails.Lobby;

public class PetInteraction : MonoBehaviour
{
    [SerializeField] WorldPositionateElement worldPositionateElement;
    [SerializeField] CinemachineVirtualCamera lobbyCamera ,petCamera;
    public LayerMask layerMaskForStyle;
    ThirdPersonController currentPlayer;
    ShinseiMovement shinseiMovement;
    CharacterSlot characterSlot;
    Camera mainCamera;
    bool controlRotation = false;
    [Header("Animations")]
    [SerializeField] int hashInteraction;
    [SerializeField] int hashPetting;
    [SerializeField] Animator animator;
    [SerializeField] List<GameObject> animationObjects = new List<GameObject>();
    [SerializeField] List<float> modelTimes = new List<float>();

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        PetRotation();
    }

    public void Init()
    {
        mainCamera = Camera.main;
        hashInteraction = Animator.StringToHash("PetInteraction");
        hashPetting = Animator.StringToHash("HelloPeter");
    }

    public void Show()
    {
        if (currentPlayer == null)
        {
            currentPlayer = ServiceLocator.Instance.GetService<ILobbyNetworkManager>().CurrentPlayer;
            characterSlot = currentPlayer.GetComponent<ShinseiSpawner>().characterSlot;
            worldPositionateElement.target = characterSlot.gameObject.transform;
            shinseiMovement = characterSlot.GetComponent<ShinseiMovement>();
            petCamera.Follow = worldPositionateElement.target;
            animator = characterSlot.GetComponentInChildren<Animator>();
            animationObjects.Add(animator.transform.GetChild(7).gameObject);
            animationObjects.Add(animator.transform.GetChild(6).gameObject);
        }
        UIGroups.instance.ShowOnlyThisGroup("pet");
        shinseiMovement.navmeshAgent.isStopped = true;
        shinseiMovement.navmeshAgent.velocity = Vector3.zero;
        controlRotation = true;
        worldPositionateElement.gameObject.SetActive(true);
        lobbyCamera.Priority = 0;
        petCamera.Priority = 1;
        mainCamera.cullingMask = layerMaskForStyle;
        //mainCamera.clearFlags = CameraClearFlags.SolidColor;
        currentPlayer.IsMovementBloqued = true;
    }

    IEnumerator TurnOffAfterTime(GameObject target, float time)
    {
        yield return new WaitForSeconds(time);
        target.SetActive(false);
        worldPositionateElement.gameObject.SetActive(true);
    }

    public void PetRotation()
    {
        if (controlRotation)
            shinseiMovement.transform.LookAt(new Vector3(petCamera.transform.position.x, shinseiMovement.transform.position.y, petCamera.transform.position.z));
    }

    public void PlayAnimation(float index)
    {
        animationObjects[(int)index].SetActive(true);
        StartCoroutine(TurnOffAfterTime(animationObjects[(int)index], modelTimes[(int)index]));
        animator.SetFloat(hashInteraction, index);
        animator.Play(hashPetting,0);
        worldPositionateElement.gameObject.SetActive(false);
    }

    public void Hide()
    {
        worldPositionateElement.gameObject.SetActive(false);
        petCamera.Priority = 0;
        lobbyCamera.Priority = 1;
        controlRotation = false;
        UIGroups.instance.ShowOnlyThisGroup("planner");
        shinseiMovement.navmeshAgent.isStopped = false;
        mainCamera.cullingMask = -1;
        //mainCamera.clearFlags = CameraClearFlags.Skybox;
        currentPlayer.IsMovementBloqued = false;
    }
}
