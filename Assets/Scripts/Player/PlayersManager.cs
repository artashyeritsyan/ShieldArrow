using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public event Action OnPlayerClassChanged;

    public static PlayerManager Instance;
    private List<PlayerController> players = new List<PlayerController>();

    public Vector3 playerSpawnPosition;

    private void Awake()
    {
        Instance = this;
    }

    public void Register(PlayerController player)
    {
        players.Add(player);
        player.transform.position = playerSpawnPosition;
    }

    public void Unregister(PlayerController player)
    {
        players.Remove(player);
    }

    public List<PlayerController> GetPlayers()
    {
        return players;
    }

    public void ChangePlayerClass(InputHandler input)
    {
        OnPlayerClassChanged?.Invoke();
    }

    public int GetPlayerIndex(PlayerController player)
    {
        return players.IndexOf(player);
    }
    public void RegisterAt(PlayerController player, int index)
    {
        if (player == null)
            return;

        players.Remove(player);

        index = Mathf.Clamp(index, 0, players.Count);

        players.Insert(index, player);
    }
}