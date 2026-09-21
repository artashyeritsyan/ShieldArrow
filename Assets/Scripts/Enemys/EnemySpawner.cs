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

    private float maxRows = 15;


    [SerializeField] private float minX = -5f;
    [SerializeField] private float maxX = 5f;

    [SerializeField] private float zNoise = 0.1f;
    [SerializeField] private float xNoise = 0.1f;

    int minEnemyInRow = 1;
    int maxEnemyInRow = 6;

    private float noSpawnChance;

    float distanceBetweenRows = 2f;

    // TODO: rewrite it into scriptable object later
    [SerializeField] float archerChance;
    [SerializeField] float shieldmanChance;
    [SerializeField] float crossbowerChance;

    public void SpawnEnemiesInRange(float startZ, float endZ)
    {
        maxRows = (endZ - startZ) / distanceBetweenRows;
        float z = startZ;
        while (z <= endZ)
        {
            SpawnRow(z, 0);
            z += distanceBetweenRows;
        }
    }

    public void SpawnRow(float rowZ, int aisleNumber)
    {
        if (this.aisleNumber != aisleNumber)
        {
            this.aisleNumber = aisleNumber;
            rowNumber = 0;
        }

        UpdateDifficulty();

        if (rowNumber != 0 && Random.value < noSpawnChance)
        {
            rowNumber++;
            return;
        }


        int enemyCount = Random.Range(minEnemyInRow, maxEnemyInRow + 1);

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject enemyPrefab;

            // Just for spawning all archers in 1st row
            if (rowNumber == 0)
                enemyPrefab = enemysPrefabs[0];
            else
                enemyPrefab = SpawnEnemy();

            float x;
            if (enemyCount == 1)
            {
                x = (minX + maxX) / 2f;
            }
            else
            {
                x = Mathf.Lerp(minX, maxX, i / (enemyCount - 1f) );
            }

            float randomX = x + Random.Range(-(xNoise), xNoise);
            float randomZ = rowZ + Random.Range(-(zNoise), zNoise);
            Vector3 spawnPosition = new(randomX, 1, randomZ);

            Instantiate(enemyPrefab, spawnPosition, Quaternion.Euler(0, 180, 0));
        }

        rowNumber++;
    }


    GameObject SpawnEnemy()
    {
        float random = Random.value;

        if (random < archerChance)
        {
            return enemysPrefabs[0];
        }

        if (random < archerChance + shieldmanChance)
        {
            return enemysPrefabs[1];
        }

        return enemysPrefabs[2];
    }

    void UpdateDifficulty()
    {
        float rowProgress = Mathf.Clamp01(rowNumber / maxRows);

        float aisleBaseDifficulty = (aisleNumber) * 0.10f;

        float difficulty = aisleBaseDifficulty + rowProgress * 0.35f;

        float aisleDifficultyModifier = aisleDifficulty switch
        {
            AisleDifficulty.Easy => -0.08f,
            AisleDifficulty.Normal => 0f,
            AisleDifficulty.Hard => 0.08f,
            _ => 0f
        };

        difficulty += aisleDifficultyModifier;

        difficulty = Mathf.Clamp01(difficulty);

        // Enemy chances
        archerChance = Mathf.Lerp(0.80f, 0.50f, difficulty);
        shieldmanChance = Mathf.Lerp(0.20f, 0.35f, difficulty);
        crossbowerChance = 1f - archerChance - shieldmanChance;

        noSpawnChance = Mathf.Lerp(0.25f, 0.15f, difficulty);

        // Enemy count
        if (rowNumber == 0 || rowNumber == 1)
        {
            minEnemyInRow = 3;
            maxEnemyInRow = 4;
        }
        else
        {
            // Here are the max and min enemies in row depends on difficulty
            minEnemyInRow = Mathf.RoundToInt(Mathf.Lerp(1f, 2f, difficulty));
            maxEnemyInRow = Mathf.RoundToInt(Mathf.Lerp(4f, 6f, difficulty));
        }
    }

}