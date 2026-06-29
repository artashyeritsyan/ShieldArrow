using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    private List<PlayerController> players = new List<PlayerController>();

    private void Awake()
    {
        Instance = this;
    }

    public void Register(PlayerController player)
    {
        players.Add(player);
    }

    public void Unregister(PlayerController player)
    {
        players.Remove(player);
    }

    public List<PlayerController> GetPlayers()
    {
        return players;
    }
}