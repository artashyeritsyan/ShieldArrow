using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class AisleGenerator : MonoBehaviour
{
    private List<GameObject> aisleSegments = new();

    [SerializeField] GameObject[] aislePrefabs;
    [SerializeField] int minLength = 30;
    [SerializeField] int maxLength = 60;
    private int finalLength = 0;

    [SerializeField] Vector3 startPosition;

    void Start()
    {
        GenerateAisle(startPosition);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateAisle(Vector3 StartPosition)
    {
        startPosition = StartPosition;
        int targetLength = Random.Range(minLength, maxLength);
        Vector3 spawnPosition = startPosition;

        Debug.Log("Target Length = " + targetLength);
        finalLength = 0;
        while (finalLength < targetLength) {
            // Choose random piece. Now there are only 1 piece so it useless
            //int randomIndex = Random.Range(0, aislePrefabs.Length);
            //GameObject newPiece = Instantiate(aislePrefabs[randomIndex], gameObject.transform);
            int segmentIndex = Random.Range(0, aislePrefabs.Length);
            int segmentLength = aislePrefabs[segmentIndex].GetComponent<AisleSegmentGenerator>().Length;

            if (segmentLength + finalLength > targetLength)
            {
                break;
            }

            GameObject newSegment = Instantiate(aislePrefabs[segmentIndex], gameObject.transform);
            newSegment.transform.position = spawnPosition;

            spawnPosition += new Vector3(0, 0, segmentLength);

            newSegment.GetComponent<AisleSegmentGenerator>().GenerateSegment();
            aisleSegments.Add(newSegment);

            finalLength += segmentLength;
        }

        Debug.Log("Final Length = " + finalLength);


    }
}
