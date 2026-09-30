using System.Collections.Generic;
using Unity.VisualScripting;
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
    private List<GameObject> enemies;

    [SerializeField] int aisleNumber = 1;
    [SerializeField] int rowNumber = 0;

    [SerializeField] private AisleDifficulty aisleDifficulty = AisleDifficulty.Easy;


    [SerializeField] private float minX = -5f;
    [SerializeField] private float maxX = 5f;

    private float maxRows;
    float minOffsetX;
    float maxOffsetX;

    [SerializeField] private float zNoise = 0.3f;
    [SerializeField] private float xNoise = 0.2f;

    // min and max ranges for enemies in row. Its depends on difficulty
    [SerializeField] private FloatRange minEnemiesInRow = new (1, 2);
    [SerializeField] private FloatRange maxEnemiesInRow = new (4, 6);

    int minEnemyInRow = 1;
    int maxEnemyInRow = 6;

    float distanceBetweenRows = 2f;

    // For skipping row
    private FloatRange noSpawnChanceRange = new (0.20f, 0.10f);
    private float noSpawnChance;
    private bool isPreviousRowSkipped = false;


    // TODO: rewrite it into scriptable object later

    // It changes depends on difficulty
    [SerializeField] FloatRange archerChancesRange = new (0.80f, 0.50f);
    [SerializeField] FloatRange shieldmanChancesRange = new (0.20f, 0.35f);
    //[SerializeField] FloatRange crossbowerChancesRange;

    float archerChance;
    float shieldmanChance;
    float crossbowerChance;

    private void Start()
    {
        enemies = new List<GameObject>();
    }

    public void SpawnEnemiesInRange(float startZ, float endZ, int aisleNumber)
    {
        if (enemies.Count != 0) {
            ClearEnemies();
        }

        maxRows = (endZ - startZ) / distanceBetweenRows;
        if (this.aisleNumber != aisleNumber)
        {
            this.aisleNumber = aisleNumber;
            rowNumber = 0;
        }

        float z = startZ;
        while (z <= endZ)
        {
            SpawnRow(z);
            z += distanceBetweenRows;
        }
    }

    public void SpawnRow(float rowZ)
    {
        UpdateDifficulty();

        // Skipping row chance
        if (rowNumber > 1 && Random.value < noSpawnChance && !isPreviousRowSkipped)
        {
            //Debug.LogWarning("Skipping row " + rowNumber);
            rowNumber++;
            isPreviousRowSkipped = true;
            return;
        }


        int enemyCount = Random.Range(minEnemyInRow, maxEnemyInRow + 1);

        minOffsetX = minX;
        maxOffsetX = maxX;

        if (enemyCount < maxEnemiesInRow.end)
        {
            //Debug.LogWarning("Adding offset on row " + rowNumber);

            //This part only for creating an offset from 0-2
            float offsetX = Random.value * 2;

            int random = Random.Range(0, 2);

            if (random == 0)
            {
                minOffsetX = minX + offsetX;
                //Debug.LogWarning("Min offset on row " + rowNumber + " minOffsetX = " + minOffsetX);
            }
            else
            {
                maxOffsetX = maxX - offsetX;
                //Debug.LogWarning("Max offset on row " + rowNumber + " maxOffsetX = " + maxOffsetX);
            }
        }


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
                x = (minOffsetX + maxOffsetX) / 2f;
            }
            else
            {
                x = Mathf.Lerp(minOffsetX, maxOffsetX, i / (enemyCount - 1f));
            }

            float randomX = x + Random.Range(-(xNoise), xNoise);
            float randomZ = rowZ + Random.Range(-(zNoise), zNoise);
            Vector3 spawnPosition = new(randomX, 1, randomZ);

            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.Euler(0, 180, 0));

            enemies.Add(enemy);
        }
        isPreviousRowSkipped = false;
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

        float difficulty = aisleBaseDifficulty + rowProgress * 0.2f;

        float aisleDifficultyModifier = aisleDifficulty switch
        {
            AisleDifficulty.Easy => -0.1f,
            AisleDifficulty.Normal => 0f,
            AisleDifficulty.Hard => 0.1f,
            _ => 0f
        };

        difficulty += aisleDifficultyModifier;

        difficulty = Mathf.Clamp01(difficulty);

        // Enemy chances
        archerChance = Mathf.Lerp(archerChancesRange.start, archerChancesRange.end, difficulty);
        shieldmanChance = Mathf.Lerp(shieldmanChancesRange.start, shieldmanChancesRange.end, difficulty);
        crossbowerChance = 1f - archerChance - shieldmanChance;

        noSpawnChance = Mathf.Lerp(noSpawnChanceRange.start, noSpawnChanceRange.end, difficulty);

        // Enemy count
        if (rowNumber == 0 || rowNumber == 1)
        {
            minEnemyInRow = 3;
            maxEnemyInRow = 4;
        }
        else
        {
            // Here are the max and min enemies in row depends on difficulty
            minEnemyInRow = Mathf.RoundToInt(Mathf.Lerp(minEnemiesInRow.start, minEnemiesInRow.end, difficulty));
            maxEnemyInRow = Mathf.RoundToInt(Mathf.Lerp(maxEnemiesInRow.start, maxEnemiesInRow.end, difficulty));
        }

        Debug.Log("Difficulty is " + difficulty);

    }

    public void ClearEnemies()
    {
        foreach (var enemy in enemies)
        {
            Destroy(enemy);
        }
        enemies.Clear();
    }

}