using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsProfile", menuName = "Scriptable Objects/PlayerStatsProfile")]
public class PlayerStatsProfile : ScriptableObject
{
    [SerializeField] PlayerStats stats;

    public PlayerStats GetStats()
    {
        return stats;
    }
}
