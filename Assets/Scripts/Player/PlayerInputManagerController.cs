using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(PlayerInputManager))]
public class PlayerInputManagerController : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField, Min(1)] private int playerMaxCount = 2;
    [SerializeField] private PlayersPrefabs playersPrefabs;

    [Header("Class Selection")]
    [SerializeField] private List<PlayerClass> selectableClasses = new()
    {
        PlayerClass.Archer,
        PlayerClass.Knight,
        PlayerClass.Crossbower,
        PlayerClass.Barbarian,
        PlayerClass.Assasin,
        PlayerClass.Mage
    };

    private int nextPlayerClassIndex = 0;

    //private readonly Dictionary<PlayerInput, InputAction> interactActions = new();
    private readonly Dictionary<PlayerInput, int> selectedClassIndices = new();

    private readonly HashSet<PlayerInput> playersBeingReplaced = new();

    [Header("Input Action Maps")]
    [SerializeField] private string playerActionMap = "Player";
    [SerializeField] private string leaveActionName = "Leave";
    //[SerializeField] private string changeClassActionName = "ChangeClass";
    //[SerializeField] private string interactActionName = "Interact";

    private PlayerInputManager inputManager;

    private readonly Dictionary<PlayerInput, InputAction> leaveActions = new();

    //private bool isLobbyActive;

    private void Awake()
    {
        inputManager = GetComponent<PlayerInputManager>();


        inputManager.onPlayerJoined += OnPlayerJoined;
        inputManager.onPlayerLeft += OnPlayerLeft;


        inputManager.playerPrefab = playersPrefabs.GetPlayerOfClass(selectableClasses[nextPlayerClassIndex]);
    }

    private void Start()
    {
        SubscribeToGameManager();

        HandleLevelStateChanged(GameManager.Instance.CurrentLevelState);
    }

    private void OnDestroy()
    {
        if (inputManager != null)
        {
            inputManager.onPlayerJoined -= OnPlayerJoined;
            inputManager.onPlayerLeft -= OnPlayerLeft;
        }

        UnsubscribeFromGameManager();

        foreach (InputAction action in leaveActions.Values)
        {
            action.performed -= OnLeavePerformed;
        }

        //foreach (InputAction action in interactActions.Values)
        //{
        //    action.performed -= OnInteractPerformed;
        //}

        //interactActions.Clear();
        leaveActions.Clear();
        selectedClassIndices.Clear();
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        if (GameManager.Instance.CurrentLevelState != LevelStates.Lobby)
        {
            Destroy(playerInput.gameObject);
            return;
        }

        int classIndex = nextPlayerClassIndex;
        selectedClassIndices[playerInput] = classIndex;

        // Prepare the opposite class for the next player.
        nextPlayerClassIndex = classIndex == 0 ? 1 : 0;

        inputManager.playerPrefab = playersPrefabs.GetPlayerOfClass(selectableClasses[nextPlayerClassIndex]);

        selectedClassIndices[playerInput] = classIndex;

        // After the first player joins, prepare the second class.
        if (selectedClassIndices.Count == 1)
        {
            int secondClassIndex = classIndex == 0 ? 1 : 0;

            inputManager.playerPrefab =
                playersPrefabs.GetPlayerOfClass(
                    selectableClasses[secondClassIndex]
                );
        }


        RegisterLeaveAction(playerInput);
        //RegisterInteractAction(playerInput);

        PlayerController playerController = playerInput.GetComponent<PlayerController>();

        if (playerController == null)
        {
            Debug.LogError(
                $"Player prefab '{playerInput.gameObject.name}' " +
                "does not contain PlayerController."
            );

            Destroy(playerInput.gameObject);
            return;
        }

        playerController.SetInputManager(this);

        PlayerManager.Instance.Register(playerController);

        UpdateJoiningState();
    }

    public void TryChangeClass(PlayerController playerController)
    {
        PlayerInput playerInput = playerController.GetComponent<PlayerInput>();

        if (playerInput == null)
            return;

        if (GameManager.Instance.CurrentLevelState != LevelStates.Lobby)
            return;

        if (selectedClassIndices.Count > 2)
            return;

        if (playersBeingReplaced.Contains(playerInput))
            return;

        StartCoroutine(ChangePlayerClassRoutine(playerInput));
    }


    private IEnumerator ChangePlayerClassRoutine(PlayerInput oldPlayerInput)
    {
        if (selectableClasses.Count == 0)
        {
            Debug.LogError("No selectable player classes configured.");
            yield break;
        }

        playersBeingReplaced.Add(oldPlayerInput);

        int currentClassIndex = 0;

        if (selectedClassIndices.TryGetValue(
            oldPlayerInput,
            out int storedIndex))
        {
            currentClassIndex = storedIndex;
        }

        int nextClassIndex = GetNextAvailableClassIndex(oldPlayerInput, currentClassIndex);

        if (nextClassIndex == -1)
        {
            Debug.LogWarning("No other player class is available.");

            playersBeingReplaced.Remove(oldPlayerInput);
            yield break;
        }

        PlayerClass nextClass = selectableClasses[nextClassIndex];

        GameObject newPrefab = playersPrefabs.GetPlayerOfClass(nextClass);

        if (newPrefab == null)
        {
            Debug.LogError($"No prefab was found for class '{nextClass}'.");

            playersBeingReplaced.Remove(oldPlayerInput);
            yield break;
        }

        Transform oldTransform = oldPlayerInput.transform;

        Vector3 spawnPosition = oldTransform.position;
        Quaternion spawnRotation = oldTransform.rotation;

        int playerIndex = oldPlayerInput.playerIndex;
        int splitScreenIndex = oldPlayerInput.splitScreenIndex;
        string controlScheme = oldPlayerInput.currentControlScheme;

        InputDevice[] pairedDevices = new InputDevice[oldPlayerInput.devices.Count];

        for (int i = 0; i < oldPlayerInput.devices.Count; i++)
        {
            pairedDevices[i] = oldPlayerInput.devices[i];
        }

        //UnregisterInteractAction(oldPlayerInput);
        UnregisterLeaveAction(oldPlayerInput);

        selectedClassIndices.Remove(oldPlayerInput);

        oldPlayerInput.DeactivateInput();
        oldPlayerInput.user.UnpairDevices();

        int playerListIndex = PlayerManager.Instance.GetPlayerIndex(oldPlayerInput.GetComponent<PlayerController>());


        Destroy(oldPlayerInput.gameObject);

        yield return null;

        if (GameManager.Instance.CurrentLevelState != LevelStates.Lobby)
        {
            playersBeingReplaced.Remove(oldPlayerInput);
            UpdateJoiningState();
            yield break;
        }

        PlayerInput newPlayerInput = PlayerInput.Instantiate(
            newPrefab,
            playerIndex,
            controlScheme,
            splitScreenIndex,
            pairedDevices
        );

        newPlayerInput.transform.SetPositionAndRotation(
            spawnPosition,
            spawnRotation
        );

        RegisterLeaveAction(newPlayerInput);
        //RegisterInteractAction(newPlayerInput);

        selectedClassIndices[newPlayerInput] = nextClassIndex;

        int availableClassIndex = nextClassIndex == 0 ? 1 : 0;

        inputManager.playerPrefab = playersPrefabs.GetPlayerOfClass(selectableClasses[availableClassIndex]);

        playersBeingReplaced.Remove(oldPlayerInput);

        //SwitchActionMap(newPlayerInput, joinActionMap);
        PlayerController newPlayerController = newPlayerInput.GetComponent<PlayerController>();

        PlayerManager.Instance.RegisterAt(newPlayerController, playerListIndex);
        UpdateJoiningState();


        Debug.Log($"Player {playerIndex} changed class to {nextClass}.");
    }

    private int GetNextAvailableClassIndex(PlayerInput currentPlayer, int currentClassIndex)
    {
        for (int offset = 1; offset <= selectableClasses.Count; offset++)
        {
            int candidateIndex = (currentClassIndex + offset) % selectableClasses.Count;

            bool classAlreadyUsed = false;

            foreach (var pair in selectedClassIndices)
            {
                PlayerInput otherPlayer = pair.Key;
                int otherClassIndex = pair.Value;

                if (otherPlayer == currentPlayer)
                    continue;

                if (otherClassIndex == candidateIndex)
                {
                    classAlreadyUsed = true;
                    break;
                }
            }

            if (!classAlreadyUsed)
                return candidateIndex;
        }

        return -1;
    }


    private static PlayerInput FindPlayerByAction(
    Dictionary<PlayerInput, InputAction> actions,
    InputAction triggeredAction)
    {
        foreach (KeyValuePair<PlayerInput, InputAction> pair in actions)
        {
            if (pair.Value == triggeredAction)
            {
                return pair.Key;
            }
        }

        return null;
    }

    private void OnPlayerLeft(PlayerInput playerInput)
    {
        Debug.Log($"Player left: {playerInput.playerIndex}");

        //UnregisterInteractAction(playerInput);
        UnregisterLeaveAction(playerInput);

        selectedClassIndices.Remove(playerInput);

        PlayerController playerController = playerInput.GetComponent<PlayerController>();

        if (playerController != null && PlayerManager.Instance != null)
        {
            PlayerManager.Instance.Unregister(playerController);
        }

        UpdateJoiningState();
    }

    private void RegisterLeaveAction(PlayerInput playerInput)
    {
        InputAction leaveAction = playerInput.actions.FindAction(
            $"{playerActionMap}/{leaveActionName}",
            false
        );

        if (leaveAction == null)
        {
            Debug.LogError(
                $"Action '{playerActionMap}/{leaveActionName}' not found."
            );
            return;
        }

        leaveAction.performed += OnLeavePerformed;
        leaveActions[playerInput] = leaveAction;
    }

    private void UnregisterLeaveAction(PlayerInput playerInput)
    {
        if (!leaveActions.TryGetValue(playerInput, out InputAction leaveAction))
            return;

        leaveAction.performed -= OnLeavePerformed;
        leaveActions.Remove(playerInput);
    }

    private void OnLeavePerformed(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.CurrentLevelState != LevelStates.Lobby)
            return;

        PlayerInput playerInput = FindPlayerByAction(leaveActions, context.action);

        if (playerInput == null)
            return;

        // Make the leaving player's class available again.
        if (selectedClassIndices.TryGetValue(playerInput, out int leavingClassIndex))
        {
            nextPlayerClassIndex = leavingClassIndex;

            inputManager.playerPrefab = playersPrefabs.GetPlayerOfClass(selectableClasses[nextPlayerClassIndex]);
        }

        Destroy(playerInput.gameObject);
    }

    private void HandleLevelStateChanged(LevelStates state)
    {
        UpdateJoiningState();
    }
    private void HandleGameStateChanged(GameStates state)
    {
        UpdateJoiningState();
    }

    private void UpdateJoiningState()
    {
        bool canJoin = false;

        if (GameManager.Instance.CurrentGameState == GameStates.Playing)
        {
            canJoin = GameManager.Instance.CurrentLevelState == LevelStates.Lobby
                && PlayerInput.all.Count < playerMaxCount;
        }

        if (canJoin)
        {
            inputManager.EnableJoining();
        }
        else
        {
            inputManager.DisableJoining();
        }

        //Debug.LogWarning("Can Join" + canJoin);
    }


    public void SetPlayerClass(PlayerClass playerClass)
    {
        GameObject prefab = playersPrefabs.GetPlayerOfClass(playerClass);

        if (prefab == null)
        {
            Debug.LogError($"No player prefab was found for class '{playerClass}'.");

            return;
        }

        inputManager.playerPrefab = prefab;
    }

    private void HandleGameReset()
    {
        var players = new List<PlayerController>(PlayerManager.Instance.GetPlayers());

        foreach (PlayerController player in players)
        {
            if (player != null)
                Destroy(player.gameObject);
        }

        playersBeingReplaced.Clear();

        nextPlayerClassIndex = 0;

        inputManager.playerPrefab = playersPrefabs.GetPlayerOfClass(selectableClasses[nextPlayerClassIndex]);
    }

    private void SubscribeToGameManager()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance was not found.");
            return;
        }

        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        GameManager.Instance.OnLevelStateChanged += HandleLevelStateChanged;
        GameManager.Instance.OnGameReset += HandleGameReset;
    }

    private void UnsubscribeFromGameManager()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            GameManager.Instance.OnLevelStateChanged -= HandleLevelStateChanged;
            GameManager.Instance.OnGameReset -= HandleGameReset;
        }
    }

    private static string GetDeviceName(PlayerInput playerInput)
    {
        if (playerInput.devices.Count == 0)
            return "Unknown device";

        return playerInput.devices[0].displayName;
    }
}