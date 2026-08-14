using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Scriptable Objects/EnemyStats")]
public class EnemyStats : ScriptableObject
{
    [SerializeField] CoreStats stats;

    public CoreStats GetStats()
    {
        return stats;
    }
}
