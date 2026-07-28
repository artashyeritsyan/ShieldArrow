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
    [SerializeField]
    private List<PlayerClass> selectableClasses = new()
    {
        PlayerClass.Archer,
        PlayerClass.Knight,
        PlayerClass.Crossbower,
        PlayerClass.Barbarian,
        PlayerClass.Assasin,
        //PlayerClass.Mage
    };


    private readonly Dictionary<PlayerInput, InputAction> changeClassActions = new();
    private readonly Dictionary<PlayerInput, int> selectedClassIndices = new();

    private readonly HashSet<PlayerInput> playersBeingReplaced = new();

    [Header("Input Action Maps")]
    [SerializeField] private string lobbyActionMap = "Lobby";
    [SerializeField] private string gameplayActionMap = "Player";
    [SerializeField] private string leaveActionName = "Leave";
    [SerializeField] private string changeClassActionName = "ChangeClass";

    private PlayerInputManager inputManager;

    private readonly Dictionary<PlayerInput, InputAction> leaveActions = new();

    private bool isLobbyActive;

    private void Awake()
    {
        inputManager = GetComponent<PlayerInputManager>();


        inputManager.onPlayerJoined += OnPlayerJoined;
        inputManager.onPlayerLeft += OnPlayerLeft;

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

        foreach (InputAction action in changeClassActions.Values)
        {
            action.performed -= OnChangeClassPerformed;
        }

        leaveActions.Clear();
        changeClassActions.Clear();
        selectedClassIndices.Clear();
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        Debug.Log($"Player joined: {playerInput.playerIndex}");

        PlayerController playerController =
            playerInput.GetComponent<PlayerController>();

        if (playerController == null)
        {
            Debug.LogError("Player prefab has no PlayerController.");
            Destroy(playerInput.gameObject);
            return;
        }

        PlayerManager.Instance.Register(playerController);

        RegisterLeaveAction(playerInput);
        RegisterChangeClassAction(playerInput);

        if (!selectedClassIndices.ContainsKey(playerInput))
        {
            selectedClassIndices[playerInput] = 0;
        }

        if (isLobbyActive)
        {
            SwitchActionMap(playerInput, lobbyActionMap);
        }
        else
        {
            Destroy(playerInput.gameObject);
            return;
        }

        UpdateJoiningState();
    }

    private void RegisterChangeClassAction(PlayerInput playerInput)
    {
        InputAction changeClassAction = playerInput.actions.FindAction(
            $"{lobbyActionMap}/{changeClassActionName}",
            false
        );

        if (changeClassAction == null)
        {
            Debug.LogError(
                $"Action '{lobbyActionMap}/{changeClassActionName}' " +
                $"was not found on '{playerInput.gameObject.name}'."
            );

            return;
        }

        changeClassAction.performed += OnChangeClassPerformed;
        changeClassActions[playerInput] = changeClassAction;
    }

    private void OnChangeClassPerformed(
    InputAction.CallbackContext context)
    {
        if (!isLobbyActive)
            return;

        PlayerInput playerInput =
            FindPlayerByAction(changeClassActions, context.action);

        if (playerInput == null)
            return;

        if (playersBeingReplaced.Contains(playerInput))
            return;

        StartCoroutine(ChangePlayerClassRoutine(playerInput));
    }

    private IEnumerator ChangePlayerClassRoutine(
    PlayerInput oldPlayerInput)
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

        int nextClassIndex =
            (currentClassIndex + 1) % selectableClasses.Count;

        PlayerClass nextClass = selectableClasses[nextClassIndex];

        GameObject newPrefab =
            playersPrefabs.GetPlayerOfClass(nextClass);

        if (newPrefab == null)
        {
            Debug.LogError(
                $"No prefab was found for class '{nextClass}'."
            );

            playersBeingReplaced.Remove(oldPlayerInput);
            yield break;
        }

        Transform oldTransform = oldPlayerInput.transform;

        Vector3 spawnPosition = oldTransform.position;
        Quaternion spawnRotation = oldTransform.rotation;

        int playerIndex = oldPlayerInput.playerIndex;
        int splitScreenIndex = oldPlayerInput.splitScreenIndex;
        string controlScheme = oldPlayerInput.currentControlScheme;

        InputDevice[] pairedDevices =
            new InputDevice[oldPlayerInput.devices.Count];

        for (int i = 0; i < oldPlayerInput.devices.Count; i++)
        {
            pairedDevices[i] = oldPlayerInput.devices[i];
        }

        UnregisterChangeClassAction(oldPlayerInput);
        UnregisterLeaveAction(oldPlayerInput);

        selectedClassIndices.Remove(oldPlayerInput);

        oldPlayerInput.DeactivateInput();
        oldPlayerInput.user.UnpairDevices();

        Destroy(oldPlayerInput.gameObject);

        yield return null;

        if (!isLobbyActive)
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

        selectedClassIndices[newPlayerInput] = nextClassIndex;

        playersBeingReplaced.Remove(oldPlayerInput);

        SwitchActionMap(newPlayerInput, lobbyActionMap);
        UpdateJoiningState();

        Debug.Log($"Player {playerIndex} changed class to {nextClass}.");
    }

    private void UnregisterChangeClassAction(
    PlayerInput playerInput)
    {
        if (!changeClassActions.TryGetValue(playerInput, out InputAction action))
        {
            return;
        }

        action.performed -= OnChangeClassPerformed;
        changeClassActions.Remove(playerInput);
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

        UnregisterLeaveAction(playerInput);
        UnregisterChangeClassAction(playerInput);

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
            $"{lobbyActionMap}/{leaveActionName}",
            false
        );

        if (leaveAction == null)
        {
            Debug.LogError(
                $"Action '{lobbyActionMap}/{leaveActionName}' was not found " +
                $"on player '{playerInput.gameObject.name}'."
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
        if (!isLobbyActive)
            return;

        PlayerInput playerToRemove = null;

        foreach (KeyValuePair<PlayerInput, InputAction> pair in leaveActions)
        {
            if (pair.Value == context.action)
            {
                playerToRemove = pair.Key;
                break;
            }
        }

        if (playerToRemove == null)
            return;

        Debug.Log($"Removing player {playerToRemove.playerIndex}");

        Destroy(playerToRemove.gameObject);
    }

    private void HandleLevelStateChanged(LevelStates newState)
    {
        isLobbyActive = newState == LevelStates.Lobby;

        UpdateJoiningState();

        foreach (PlayerInput playerInput in PlayerInput.all)
        {
            string actionMap = isLobbyActive
                ? lobbyActionMap
                : gameplayActionMap;

            SwitchActionMap(playerInput, actionMap);
        }
    }

    private void UpdateJoiningState()
    {
        bool hasFreePlayerSlot = PlayerInput.all.Count < playerMaxCount;

        if (isLobbyActive && hasFreePlayerSlot)
        {
            inputManager.EnableJoining();
        }
        else
        {
            inputManager.DisableJoining();
        }
    }

    private void SwitchActionMap(
        PlayerInput playerInput,
        string actionMapName)
    {
        if (playerInput.actions.FindActionMap(actionMapName) == null)
        {
            Debug.LogError(
                $"Action map '{actionMapName}' was not found on " +
                $"'{playerInput.gameObject.name}'."
            );

            return;
        }

        if (playerInput.currentActionMap?.name == actionMapName)
            return;

        playerInput.SwitchCurrentActionMap(actionMapName);
    }


    public void SetPlayerClass(PlayerClass playerClass)
    {
        GameObject prefab = playersPrefabs.GetPlayerOfClass(playerClass);

        if (prefab == null)
        {
            Debug.LogError(
                $"No player prefab was found for class '{playerClass}'."
            );

            return;
        }

        inputManager.playerPrefab = prefab;
    }

    private void SubscribeToGameManager()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance was not found.");
            return;
        }

        GameManager.Instance.OnLevelStateChanged += HandleLevelStateChanged;
    }

    private void UnsubscribeFromGameManager()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelStateChanged -= HandleLevelStateChanged;
        }
    }

    private static string GetDeviceName(PlayerInput playerInput)
    {
        if (playerInput.devices.Count == 0)
            return "Unknown device";

        return playerInput.devices[0].displayName;
    }
}