using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayersPrefabs", menuName = "Scriptable Objects/PlayersPrefabs")]
public class PlayersPrefabs : ScriptableObject
{
    [SerializeField] List<GameObject> playerPrefabs;

    public List<GameObject> GetPlayers()
    {
        return playerPrefabs;
    }

    public GameObject GetPlayerOfClass(PlayerClass playerClass)
    {
        foreach (GameObject player in playerPrefabs)
        {
            if (player.GetComponent<PlayerController>().GetPlayerClass() == playerClass)
                return player;
        }
        
        Debug.LogError("No player found with class " + playerClass.ToString());
        return null;
    }

}
