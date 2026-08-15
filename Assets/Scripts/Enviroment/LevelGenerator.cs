using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] GameObject RoomPrefab;
    [SerializeField] AisleGenerator aisleGenerator;

    [SerializeField] List<GameObject> level;
    [SerializeField] List<Vector3> roomsScreenCenters;

    [SerializeField] int levelLength = 5;
    [SerializeField] Vector3 startPosition = new(0, 0, 1);
    void Start()
    {
        // Only mooving in axis Z
        level = new List<GameObject>();
        //roomsScreenCenters = new List<Vector3>();


    }

    public void GenerateLevel()
    {

        Debug.Log("LEvel generated" );

        //List<GameObject> level = new();

        for (int i = 0; i < levelLength; i++)
        {
            level.Add(aisleGenerator.GenerateAisle(startPosition));
            // 15f is the delta from aisle startposition
            roomsScreenCenters.Add(startPosition + new Vector3(0, 0, 8f));

            GameObject room = Instantiate(RoomPrefab, transform);
            RoomGenerator roomGenerator = room.GetComponent<RoomGenerator>();

            startPosition += new Vector3(0, 0, aisleGenerator.FinalLength + roomGenerator.Height / 2);
            
            roomGenerator.GenerateRoom(startPosition);
            // 0.5 just the delta for screenCenter position
            roomsScreenCenters.Add(startPosition + new Vector3(0,0, 0.5f));
            Debug.Log("roomSize 11 " + roomsScreenCenters.Count);


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

}
