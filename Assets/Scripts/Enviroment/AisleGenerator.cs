using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class AisleGenerator : MonoBehaviour
{
    private List<GameObject> aislePieces = new();

    [SerializeField] GameObject[] aislePrefabs;
    [SerializeField] int minLength = 10;
    [SerializeField] int maxLength = 20;

    [SerializeField] Vector3 startPosition;
    [SerializeField] float onePieceSize = 2f;

    [Header("Walls")] 
    [SerializeField] GameObject[] walls;


    void Start()
    {
        GenerateAisle();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateAisle()
    {
        int randomLength = Random.Range(minLength, maxLength);
        Vector3 spawnPosition = startPosition;

        Debug.Log("Length = " + randomLength);

        for (int i = 0; i < randomLength; i++) {
            // Choose random piece. Now there arew only 1 piece so it useless
            //int randomIndex = Random.Range(0, aislePrefabs.Length);
            //GameObject newPiece = Instantiate(aislePrefabs[randomIndex], gameObject.transform);
            
            GameObject newPiece = Instantiate(aislePrefabs[0], gameObject.transform);
            newPiece.transform.position = spawnPosition;

            spawnPosition += new Vector3(0, 0, onePieceSize);

            newPiece.GetComponent<WallDecorator>().GenerateDecorations();

            aislePieces.Add(newPiece);
        }

    }
}
