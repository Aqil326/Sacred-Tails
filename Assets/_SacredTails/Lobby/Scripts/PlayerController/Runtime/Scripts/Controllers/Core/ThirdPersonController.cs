using UnityEngine;
using Unity.Netcode;
using Timba.Packages.Games.PlayerControllerModule;
using Timba.Packages.Games.PlayerControllerModule.Core;
using UnityEngine.AI;
using DG.Tweening;
using System;
using Timba.Games.SacredTails.LobbyDatabase;
using System.Collections.Generic;
using UnityEngine.UI;
using Timba.SacredTails.Lobby;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.TournamentBehavior;

/// <summary>
/// Controll the behavior of the main character
/// </summary>
[RequireComponent(typeof(Rigidbody))]

public class ThirdPersonController : ThirdPersonCoreController
{
    #region ----Fields----
    [SerializeField] private float walkingSpeed = 5;
    [SerializeField] private float currentSpeed = 5;
    [SerializeField] private float runningSpeed = 15;
    [SerializeField] private Vector3 moveDirection = Vector3.zero;
    public GameObject playerPersonalUI;
    public List<Animator> animator = new List<Animator>();
    public NavMeshAgent navmeshAgent;
    private Vector3 oldInputPos = Vector3.zero;
    int blendHash;
    public string playfabId;
    public string displayName;
    public string currentMatchId = "";

    private Transform objectTransform;
    private Rigidbody rb;
    private float horizontalInput;
    private float verticalInput;
    private bool isMovementBloqued;
    public bool IsMovementBloqued
    {
        get => isMovementBloqued;
        set
        {
            if (CanBeBlocked)
                isMovementBloqued = value;
        }
    }
    public bool CanBeBlocked = true;
    public bool IsChatMode;
    public bool IsLocalPlayer = false;

    public float distance = 0;

    public bool EnableMovement = true;

    public Action OnDisablePLayer;
    public PlayerIconController playerIconController;
    public ChallengePlayerController challengePlayerController;
    //Sorry for put this here :'v
    [SerializeField] List<MaterialReskin> materialReskin = new List<MaterialReskin>();

    //Brackets tournament
    public Button openTournamentButton;
    public TournamentReadyController tournamentReadyController;

    public float MaxDistanceToTeleport = 10;
    #endregion ----Fields----

    #region ----Methods----
    private void Awake()
    {
        Init<object>(null);
        foreach (var recolor in materialReskin)
        {
            recolor.InitReskin();
        }
    }

    private void Start()
    {
        blendHash = Animator.StringToHash("Velocity");
        if (PlayerDataManager.Singleton.localPlayerGameObject == null)
            PlayerDataManager.Singleton.localPlayerGameObject = gameObject;
    }

    private void OnDisable()
    {
        OnDisablePLayer?.Invoke();
    }

    public override void Init<T>(T data)
    {
        if (inputHandler == null)
        {
            if (!TryGetComponent<IInputHandleable>(out inputHandler))
            {
                Debug.LogWarning($"Something wrong IInputHandleable not found.");
            }
            else
            {
                openTournamentButton.onClick.AddListener(() => ServiceLocator.Instance.GetService<IBracketsTournament>().ShowPanelBracketsView(true));
                inputHandler.Init();
                InitRigidBody();
            }
        }
    }


    private void Update()
    {
        if (IsLocalPlayer)
        {
            ClientInput();
            MoveObject();
            //if (Input.GetKeyDown(KeyCode.LeftShift))
            //    currentSpeed = runningSpeed;
            //if (Input.GetKeyUp(KeyCode.LeftShift))
            //    currentSpeed = walkingSpeed;
        }
        else
        {
            if (navmeshAgent != null)
                foreach (var anima in animator)
                    if (anima.gameObject.activeSelf)
                        anima?.SetFloat(blendHash, navmeshAgent.velocity.magnitude);
        }
    }

    private void ClientInput()
    {
        if (!IsMovementBloqued && !IsChatMode)
        {
            //horizontalInput = inputHandler.GetHorizontalInput();
            //verticalInput = inputHandler.GetVerticalInput();
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");
            if (PlayerDataManager.Singleton.isFrenchKeyboardLayout)
            {
                if (horizontalInput == 0)
                    horizontalInput = Input.GetKey(KeyCode.Q) ? -1 : 0;
                else
                    if (Input.GetKey(KeyCode.A))
                    horizontalInput = 0;
                if (verticalInput == 0)
                    verticalInput = Input.GetKey(KeyCode.Z) ? 1 : 0;
                else
                    if (Input.GetKey(KeyCode.W))
                    verticalInput = 0;
            }
        }
        else
        {
            horizontalInput = 0;
            verticalInput = 0;
        }
        Vector3 moveDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;
        if (moveDirection != oldInputPos)
        {
            oldInputPos = moveDirection;
            this.moveDirection = moveDirection;
        }
    }

    public void PutPlayerInFacingCamera()
    {
        Vector3 newPos = transform.position;
        newPos += new Vector3(0, 0, -1f);
        MoveObject(newPos);
    }

    public void MoveObject(Vector3? replacePosition = null)
    {
        if (replacePosition != null)
        {
            if (navmeshAgent.isOnNavMesh)
            {
                //If distance is too long teleport instead only move
                if ((transform.position - replacePosition.Value).sqrMagnitude > MaxDistanceToTeleport * MaxDistanceToTeleport)
                {
                    navmeshAgent.Warp(replacePosition.Value);
                    GetComponent<ShinseiSpawner>().characterSlot.GetComponent<NavMeshAgent>().Warp(replacePosition.Value);
                }
                navmeshAgent.destination = replacePosition.Value;
            }
            return;
        }

        if (!EnableMovement)
        {
            foreach (var anima in animator)
            {
                if (anima.gameObject.activeSelf)
                    anima?.SetFloat(blendHash, navmeshAgent.velocity.magnitude);
            }
            return;
        }

        //Detect ground
        bool isGrounded = false;
        Ray ray = new Ray(transform.position, transform.up * -distance);
        RaycastHit[] raycast = Physics.RaycastAll(ray, distance);
        if (raycast.Length > 0)
        {
            isGrounded = true;
            Debug.DrawRay(ray.origin, ray.direction * distance, Color.green);
        }
        else
            Debug.DrawRay(ray.origin, ray.direction * distance, Color.red);
        if (isGrounded)
        {
            navmeshAgent.velocity = new Vector3(moveDirection.x * currentSpeed, navmeshAgent.velocity.y, moveDirection.z * currentSpeed);
            foreach (var anima in animator)
            {
                if (anima.gameObject.activeSelf)
                    anima?.SetFloat(blendHash, navmeshAgent.velocity.magnitude);
            }
        }

        if (moveDirection != Vector3.zero)
        {
            objectTransform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    private void InitRigidBody()
    {
        rb = this.GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        objectTransform = this.transform;
    }

    public void SetStateIcon(CharacterStateEnum characterState)
    {
        playerIconController?.ChangeIcon(characterState);
    }
    #endregion ----Methods----
}
