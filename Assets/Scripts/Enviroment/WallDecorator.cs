using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WallDecorator : MonoBehaviour
{
    private List<GameObject> decorations = new();

    // In future generate the walls as well, so it would be random type of wall
    //[SerializeField] GameObject[] wallSpawnPoints;

    [Header("Walls")]
    [SerializeField] Transform leftWallParent;
    [SerializeField] Transform rightWallParent;

    [Header("OnWall Decorations")]
    //[SerializeField] GameObject[] torchs;
    [SerializeField] GameObject[] onWallDecorationsPrefabs;

    [SerializeField] int decorationsTypeCount = 2;

    [Header("Chances")]
    [SerializeField] private float nothingChance = 0.2f;
    [SerializeField] private float torchChance = 0.3f;


    public void GenerateDecorations()
    {
        //int decorationType = Random.Range(0, decorationsTypeCount);
        //foreach (var decoration in decorations) { 
        //    Destroy(decoration.gameObject);

        //    decorations.Remove(decoration);
        //}


        GenerateDecorationsFor(leftWallParent);
        GenerateDecorationsFor(rightWallParent);
    }

    public void GenerateDecorationsFor(Transform parent)
    {
        float chance = Random.value;

        if (chance < nothingChance)
            return;

        int randomDecoration;

        if (chance < nothingChance + torchChance)
        {
            randomDecoration = 0;
        }
        else
        {
            randomDecoration = Random.Range(1, onWallDecorationsPrefabs.Length);
        }

        GameObject newDecoration = Instantiate(onWallDecorationsPrefabs[randomDecoration], parent);
        newDecoration.transform.localPosition = Vector3.zero;
        decorations.Add(newDecoration);
    }

}
