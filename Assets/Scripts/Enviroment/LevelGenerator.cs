using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FloatRange
{
    public float start;
    public float end;

    public FloatRange(float start, float end)
    {
        this.start = start;
        this.end = end;
    }
}

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] GameObject RoomPrefab;
    [SerializeField] AisleGenerator aisleGenerator;

    [SerializeField] List<FloatRange> aisleStartEndPositions;

    [SerializeField] List<GameObject> level;
    [SerializeField] List<Vector3> roomsScreenCenters;

    [SerializeField] int currentRoomIndex = 0;
    public int CurrentRoomIndex => currentRoomIndex;

    [SerializeField] int levelLength = 5;
    [SerializeField] Vector3 startPosition = new(0, 0, 1);

    [Header("Parameters")]
    [SerializeField] float aisleScreenCenterOffset = 8.5f;
    [SerializeField] float roomScreenCenterOffset = 0.8f;
    void Start()
    {
        // Only mooving in axis Z
        level = new List<GameObject>();
    }

    public void GenerateLevel()
    {
        for (int i = 0; i < levelLength; i++)
        {
            level.Add(aisleGenerator.GenerateAisle(startPosition));
            roomsScreenCenters.Add(startPosition + new Vector3(0, 0, aisleScreenCenterOffset));

            GameObject room = Instantiate(RoomPrefab, transform);
            RoomGenerator roomGenerator = room.GetComponent<RoomGenerator>();

            // Adding the Start and End position of the aisle
            aisleStartEndPositions.Add(new FloatRange(startPosition.z, startPosition.z + aisleGenerator.FinalLength));

            startPosition += new Vector3(0, 0, aisleGenerator.FinalLength + roomGenerator.Height / 2);
            
            roomGenerator.GenerateRoom(startPosition);
            roomsScreenCenters.Add(startPosition + new Vector3(0, 0, roomScreenCenterOffset));
            Debug.Log("roomSize " + roomsScreenCenters.Count);

            level.Add(room);

            startPosition += new Vector3(0, 0, roomGenerator.Height / 2);
        }
    }

    public List<GameObject> GetLevel()
    {
        return level;
    }

    public List<Vector3> GetScreenCenters()
    {
        return roomsScreenCenters;
    }
    public Vector3 GetScreenCenterOf(int roomIndex)
    {
        Debug.Log("roomSize  " + roomsScreenCenters.Count);
        Debug.Log("Room screenCenter index " + roomIndex);

        return roomsScreenCenters[roomIndex];
    }

    public void SetCurrentLevelIndex(int levelIndex)
    {
        currentRoomIndex = levelIndex;
    }

    void HandleLevelStateChange(LevelStates levelState)
    {
        if (levelState == LevelStates.Aisle)
        {
            SpawnEnemies();
        }
    }

    void SpawnEnemies()
    {
        gameObject.GetComponent<EnemySpawner>().SpawnEnemiesInRange(
            aisleStartEndPositions[currentRoomIndex/2].start + (aisleScreenCenterOffset * 2) + 1.5f,
            aisleStartEndPositions[currentRoomIndex/2].end - 2f, // 2 is just to keep distance from room wall
            currentRoomIndex / 2); // To get the Aisle Number for difficulty calculations
    }

    private void OnEnable()
    {
        GameManager.Instance.OnLevelStateChanged += HandleLevelStateChange;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnLevelStateChanged -= HandleLevelStateChange;
    }

}
