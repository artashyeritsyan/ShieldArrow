using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInputManagerController : MonoBehaviour
{
    [SerializeField] int playerMaxCount;
    [SerializeField] List<GameObject> playerPrefabs;

    PlayerInputManager inputManager;

    private void Awake()
    {
        inputManager = GetComponent<PlayerInputManager>();

        inputManager.onPlayerJoined += OnPlayerJoined;
        inputManager.onPlayerLeft += OnPlayerLeft;

        SetPlayerClass(PlayerClass.Archer);
    }

    private void OnDestroy()
    {
        inputManager.onPlayerJoined -= OnPlayerJoined;
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        Debug.Log("Player joined: " + playerInput.playerIndex);

        // You can access the spawned player like this:
        GameObject player = playerInput.gameObject;


        if (playerInput.playerIndex == 0)
        {
            SetPlayerClass(PlayerClass.Knight);
        }
    }

    private void OnPlayerLeft(PlayerInput playerInput)
    {
        if (playerInput.playerIndex == 1)
        {
            SetPlayerClass(PlayerClass.Archer);
        }
    }

    public void SetPlayerClass(PlayerClass playerClass)
    {
        inputManager.playerPrefab = playerPrefabs[(int)playerClass];

    }

    public void SetPlayerJoinEnabled(bool joinEnabled)
    {
        if (joinEnabled)
        {
            inputManager.EnableJoining();
        }
        else
        {
            inputManager.DisableJoining();
        }
    }

}
