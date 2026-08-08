using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(fileName = "WallPrefabs", menuName = "Scriptable Objects/WallPrefabs")]
public class WallPrefabs : ScriptableObject
{
    [SerializeField] GameObject[] plainWalls;
    [SerializeField] GameObject[] windowedWalls;
    [SerializeField] GameObject[] cornerWalls;
    [SerializeField] GameObject[] tSplitWalls;
    [SerializeField] GameObject[] doorwayWalls;

    public GameObject[] PlainWalls => plainWalls;
    public GameObject[] WindowedWalls => windowedWalls;
    public GameObject[] CornerWalls => cornerWalls;
    public GameObject[] TSplitWalls => tSplitWalls;
    public GameObject[] DoorwayWalls => doorwayWalls;

    // No need, useless
    public GameObject GetRandomPlainWall()
    {
        return GetRandomWallFrom(plainWalls);
    }
    public GameObject GetRandomWindowedWall()
    {
        return GetRandomWallFrom(WindowedWalls);
    }
    public GameObject GetRandomCornerWall()
    {
        return GetRandomWallFrom(CornerWalls);
    }
    public GameObject GetRandomDoorwayWall()
    {
        return GetRandomWallFrom(DoorwayWalls);
    }

    public GameObject GetRandomWallFrom(GameObject[] array)
    {
        int index = Random.Range(0, array.Length);

        return array[index];
    }
}
