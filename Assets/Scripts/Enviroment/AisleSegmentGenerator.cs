using System.Collections.Generic;
using UnityEngine;

public class AisleSegmentGenerator : MonoBehaviour
{
    private List<GameObject> floorTiles = new();

    // In future generate the walls as well, so it would be random type of wall
    //[SerializeField] GameObject[] wallSpawnPoints;

    [Header("Segment Parameters")]
    [SerializeField] int length = 4;
    public int Length => length;

    [Header("Walls")]
    [SerializeField] Transform leftWallParent;
    [SerializeField] Transform rightWallParent;

    [Header("Prefab objects")]
    [SerializeField] WallPrefabs wallPrefabs;
    [SerializeField] EnviromentPrefabs groundPrefabs;


    [Header("Wall Chances")]
    [SerializeField] private float plainWallChance = 0.5f;

    [Header("Ground Chances")]
    [SerializeField] private float simpleFloorChance = 0.5f;


    public void GenerateSegment()
    {
        //int decorationType = Random.Range(0, decorationsTypeCount);
        //foreach (var decoration in decorations) { 
        //    Destroy(decoration.gameObject);

        //    decorations.Remove(decoration);
        //}

        GenerateWallFor(leftWallParent.transform);
        GenerateWallFor(rightWallParent.transform);

        // move this part into wall script (create new script)
        //GenerateFloor();
    }

    void GenerateWallFor(Transform parent)
    {
        float chance = Random.value;
        GameObject wallPref;

        if (chance < plainWallChance) {
            wallPref = wallPrefabs.GetRandomPlainWall();
        }
        else {
            wallPref = wallPrefabs.GetRandomWindowedWall();
        }

        GameObject wall = Instantiate(wallPref, parent.transform);
        wall.transform.localPosition = Vector3.zero;
        wall.GetComponent<WallDecorator>().GenerateDecorations();
    }

    void GenerateFloor()
    {
        float chance = Random.value;

        int tileIndex;

        if (chance < simpleFloorChance)
        {
            tileIndex = 0;
        }
        else
        {
            tileIndex = Random.Range(1, groundPrefabs.Size);
        }

        GameObject tile = Instantiate(groundPrefabs.Prefabs[tileIndex], gameObject.transform);
        tile.transform.localPosition = Vector3.zero;
        floorTiles.Add(tile);
    }
}
