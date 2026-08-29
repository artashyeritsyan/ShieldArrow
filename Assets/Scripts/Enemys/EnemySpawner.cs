using UnityEngine;

public enum AisleDifficulty
{
    Easy = 1,
    Normal = 2,
    Hard = 3
}


public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject[] enemysPrefabs;

    [SerializeField] int aisleNumber = 1;
    [SerializeField] int rowNumber = 0;

    [SerializeField] private AisleDifficulty aisleDifficulty = AisleDifficulty.Normal;

    [SerializeField] private int maxRows = 15;


    [SerializeField] private float minZ = -5f;
    [SerializeField] private float maxZ = 5f;

    int minEnemyInRow = 3;
    int maxEnemyInRow = 4;

    // TODO: rewrite it into scriptable object later
    [SerializeField] float archerChance;
    [SerializeField] float shieldmanChance;
    [SerializeField] float crossbowerChance;

    public void SpawnRow(float x, int aisleNumber)
    {
        if (this.aisleNumber != aisleNumber)
        {
            this.aisleNumber = aisleNumber;
            rowNumber = 0;
        }

        UpdateDifficulty();

        int enemyCount = Random.Range(minEnemyInRow, maxEnemyInRow + 1);

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject enemyPrefab = SpawnEnemy();

            float z;

            if (enemyCount == 1)
            {
                z = (minZ + maxZ) / 2f;
            }
            else
            {
                z = Mathf.Lerp(minZ, maxZ, i / (enemyCount - 1f) );
            }

            Vector3 spawnPosition = new Vector3(x, 0f, z);

            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }

        rowNumber++;
    }


    GameObject SpawnEnemy()
    {
        float random = Random.value;

        if (random < shieldmanChance)
        {
            return enemysPrefabs[2];
        }

        if (random < shieldmanChance + crossbowerChance)
        {
            return enemysPrefabs[1];
        }

        return enemysPrefabs[0];
    }

    void UpdateDifficulty()
    {
        float rowProgress = Mathf.Clamp01(rowNumber / (float)maxRows);

        float aisleBaseDifficulty = (aisleNumber - 1) * 0.10f;

        float difficulty =aisleBaseDifficulty + rowProgress * 0.35f;

        float aisleDifficultyModifier = aisleDifficulty switch
        {
            AisleDifficulty.Easy => -0.08f,
            AisleDifficulty.Normal => 0f,
            AisleDifficulty.Hard => 0.08f,
            _ => 0f
        };

        difficulty += aisleDifficultyModifier;

        difficulty = Mathf.Clamp01(difficulty);

        shieldmanChance = Mathf.Lerp(0f, 0.15f, difficulty);

        crossbowerChance = shieldmanChance * 1.5f;

        archerChance = 1f - shieldmanChance - crossbowerChance;

        minEnemyInRow = Mathf.RoundToInt(Mathf.Lerp(3f, 6f, difficulty));

        maxEnemyInRow = Mathf.RoundToInt(Mathf.Lerp(4f, 9f, difficulty));
    }

}