using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] GameObject RoomPrefab;

    [SerializeField] AisleGenerator aisleGenerator;

    [SerializeField] List<GameObject> Level;

    [SerializeField] int levelLength = 3;
    [SerializeField] Vector3 startPosition = new(0, 0, 1);
    void Start()
    {
        // Only mooving in axis Z
        for (int i = 0; i < levelLength; i++)
        {
            Level.Add(aisleGenerator.GenerateAisle(startPosition));
            

            GameObject room = Instantiate(RoomPrefab, transform);
            RoomGenerator roomGenerator = room.GetComponent<RoomGenerator>();

            startPosition += new Vector3(0, 0, aisleGenerator.FinalLength + roomGenerator.Height / 2);
            
            roomGenerator.GenerateRoom(startPosition);

            startPosition += new Vector3(0, 0, roomGenerator.Height/2);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
