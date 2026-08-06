using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WallDecorator : MonoBehaviour
{
    private List<GameObject> decorations = new();
    private List<GameObject> floorTiles = new();

    // In future generate the walls as well, so it would be random type of wall
    //[SerializeField] GameObject[] wallSpawnPoints;

    [Header("Walls")]
    [SerializeField] Transform leftWallParent;
    [SerializeField] Transform rightWallParent;

    [Header("Prefab objects")]
    //[SerializeField] GameObject[] torchs;
    [SerializeField] EnviromentPrefabs wallPrefabs;
    [SerializeField] EnviromentPrefabs groundPrefabs;
    [SerializeField] EnviromentPrefabs wallDecorations;
    [SerializeField] EnviromentPrefabs groundDecorations;

    [SerializeField] int decorationsTypeCount = 2;

    [Header("Wall Decorations Chances")]
    [SerializeField] private float nothingChance = 0.2f;
    [SerializeField] private float torchChance = 0.3f;

    [Header("Ground Chances")]
    [SerializeField] private float simpleFloor = 0.5f;


    public void GenerateDecorations()
    {
        //int decorationType = Random.Range(0, decorationsTypeCount);
        //foreach (var decoration in decorations) { 
        //    Destroy(decoration.gameObject);

        //    decorations.Remove(decoration);
        //}


        GenerateDecorationsFor(leftWallParent);
        GenerateDecorationsFor(rightWallParent);
        //GenerateFloor();
    }

    void GenerateFloor()
    {
        float chance = Random.value;

        int decorationIndex;

        if (chance < simpleFloor)
        {
            decorationIndex = 0;
        }
        else
        {
            decorationIndex = Random.Range(1, groundPrefabs.Size);
        }

        GameObject tile = Instantiate(groundPrefabs.Prefabs[decorationIndex], gameObject.transform);
        tile.transform.localPosition = Vector3.zero;
        floorTiles.Add(tile);
    }

    public void GenerateDecorationsFor(Transform parent)
    {
        float chance = Random.value;

        if (chance < nothingChance)
            return;

        int decorationIndex;

        if (chance < nothingChance + torchChance)
        {
            decorationIndex = 0;
        }
        else
        {
            decorationIndex = Random.Range(1, wallDecorations.Size);
        }

        GameObject newDecoration = Instantiate(wallDecorations.Prefabs[decorationIndex], parent);
        newDecoration.transform.localPosition = Vector3.zero;
        decorations.Add(newDecoration);
    }

}
