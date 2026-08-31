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
        //PlayerClass.Crossbower,
        //PlayerClass.Barbarian,
        //PlayerClass.Assasin,
        //PlayerClass.Mage
    };

    private int nextPlayerClassIndex = 0;

    private readonly Dictionary<PlayerInput, InputAction> changeClassActions = new();
    private readonly Dictionary<PlayerInput, int> selectedClassIndices = new();

    private readonly HashSet<PlayerInput> playersBeingReplaced = new();

    [Header("Input Action Maps")]
    [SerializeField] private string playerActionMap = "Player";
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
        if (!isLobbyActive)
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
        RegisterChangeClassAction(playerInput);

        PlayerController playerController =
            playerInput.GetComponent<PlayerController>();

        if (playerController == null)
        {
            Debug.LogError(
                $"Player prefab '{playerInput.gameObject.name}' " +
                "does not contain PlayerController."
            );

            Destroy(playerInput.gameObject);
            return;
        }

        PlayerManager.Instance.Register(playerController);

        UpdateJoiningState();
    }

    private void RegisterChangeClassAction(PlayerInput playerInput)
    {
        InputAction changeClassAction = playerInput.actions.FindAction(
            $"{playerActionMap}/{changeClassActionName}",
            false
        );

        if (changeClassAction == null)
        {
            Debug.LogError(
                $"Action '{playerActionMap}/{changeClassActionName}' not found."
            );
            return;
        }

        changeClassAction.performed += OnChangeClassPerformed;
        changeClassActions[playerInput] = changeClassAction;
    }
    private void OnChangeClassPerformed(InputAction.CallbackContext context)
    {
        if (!isLobbyActive)
            return;

        if (selectedClassIndices.Count >= 2)
            return;

        PlayerInput playerInput = FindPlayerByAction(changeClassActions, context.action);

        if (playerInput == null || playersBeingReplaced.Contains(playerInput))
        {
            return;
        }

        PlayerController playerController = playerInput.GetComponent<PlayerController>();
        if (playerController == null)
            return;

        // Check for class change
        if (!playerController.CanInteractWith(InteractionType.ClassChange))
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

        int nextClassIndex = (currentClassIndex + 1) % selectableClasses.Count;

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

        InputDevice[] pairedDevices = new InputDevice[oldPlayerInput.devices.Count];

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

        int availableClassIndex = nextClassIndex == 0 ? 1 : 0;

        inputManager.playerPrefab =
            playersPrefabs.GetPlayerOfClass(
                selectableClasses[availableClassIndex]
            );

        playersBeingReplaced.Remove(oldPlayerInput);

        //SwitchActionMap(newPlayerInput, joinActionMap);
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
        if (!isLobbyActive)
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
        isLobbyActive = state == LevelStates.Lobby;

        UpdateJoiningState();
    }

    private void UpdateJoiningState()
    {
        bool canJoin = false;

        if (GameManager.Instance.CurrentGameState != GameStates.Menu && GameManager.Instance.CurrentGameState != GameStates.Pause)
        {
            canJoin = isLobbyActive && PlayerInput.all.Count < playerMaxCount;
        }

        if (canJoin)
        {
            inputManager.EnableJoining();
        }
        else
        {
            inputManager.DisableJoining();
        }
    }

    //private void SwitchActionMap(
    //    PlayerInput playerInput,
    //    string actionMapName)
    //{
    //    if (playerInput.actions.FindActionMap(actionMapName) == null)
    //    {
    //        Debug.LogError(
    //            $"Action map '{actionMapName}' was not found on " +
    //            $"'{playerInput.gameObject.name}'."
    //        );

    //        return;
    //    }

    //    if (playerInput.currentActionMap?.name == actionMapName)
    //        return;

    //}

    private void HandleGameStateChanged(GameStates  state)
    {
        //isLobbyActive = state == GameStates.Lobby;

        UpdateJoiningState();
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

        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        GameManager.Instance.OnLevelStateChanged += HandleLevelStateChanged;
    }

    private void UnsubscribeFromGameManager()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
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