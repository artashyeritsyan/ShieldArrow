using UnityEngine;

public class LootSpawner : MonoBehaviour
{
    public static LootSpawner Instance { get; private set; }

    [SerializeField] Coin coinPrefab;
    [SerializeField] float coinChance = 0.2f;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SpawnCoin(Vector3 position, EnemyCombatType enemyType)
    {
        float random = Random.value;

        float spawnChance;

        switch (enemyType)
        {
            case EnemyCombatType.Archer:
                spawnChance = coinChance;
                break;
            case EnemyCombatType.Crossbower:
                spawnChance = coinChance * 1.5f;
                break;
            case EnemyCombatType.Shieldman:
                spawnChance = coinChance * 2f;
                break;
            default:
                spawnChance = coinChance;
                break;
        }

        if (random < spawnChance)
        {
            ObjectPoolManager.Instance.Spawn(coinPrefab, position, Quaternion.identity);
        }

    }

}
