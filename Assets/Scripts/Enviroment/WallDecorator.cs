using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WallDecorator : MonoBehaviour
{
    [Header("Wall Parameters")]
    [SerializeField] bool canDecorateWall;
    [SerializeField] bool canDecorateGround;

    [Header("Prefab objects")]
    //[SerializeField] GameObject[] torchs;

    [SerializeField] EnviromentPrefabs wallDecorations;
    [SerializeField] EnviromentPrefabs groundDecorations;

    //[SerializeField] int decorationsTypeCount = 2;

    [Header("Wall Decorations Chances")]
    [SerializeField] private float nothingChance = 0.2f;
    [SerializeField] private float torchChance = 0.3f;



    public void GenerateDecorations()
    {
        //int decorationType = Random.Range(0, decorationsTypeCount);
        //foreach (var decoration in decorations) { 
        //    Destroy(decoration.gameObject);

        //    decorations.Remove(decoration);
        //}
        if (canDecorateWall) { 
            GenerateWallDecoration();
        }

        if (canDecorateGround) {
            GenerateGroundDecoration();
        }        
    }

    void GenerateGroundDecoration()
    {

    }

    void GenerateWallDecoration()
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

        GameObject newDecoration = Instantiate(wallDecorations.Prefabs[decorationIndex], transform);
        newDecoration.transform.localPosition = Vector3.zero;
    }

}
