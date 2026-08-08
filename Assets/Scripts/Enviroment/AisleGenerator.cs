using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class AisleGenerator : MonoBehaviour
{
    //private List<GameObject> aisleSegments = new();

    [SerializeField] GameObject[] aislePrefabs;
    [SerializeField] int minLength = 30;
    [SerializeField] int maxLength = 60;
    private float finalLength = 0;
    public float FinalLength => finalLength;

    [SerializeField] Vector3 startPosition;

    void Start()
    {
        //GenerateAisle(startPosition);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject GenerateAisle(Vector3 StartPosition)
    {
        startPosition = StartPosition;
        GameObject aisle = Instantiate(new GameObject("Aisle"), startPosition, Quaternion.identity, transform);

        finalLength = 0;
        int targetLength = Random.Range(minLength, maxLength);
        Vector3 spawnPosition = startPosition;

        Debug.Log("Target Length = " + targetLength);
        finalLength = 0;
        while (finalLength < targetLength) {

            int segmentIndex = Random.Range(0, aislePrefabs.Length);
            int segmentLength = aislePrefabs[segmentIndex].GetComponent<AisleSegmentGenerator>().Length;

            if (segmentLength + finalLength > targetLength)
            {
                break;
            }

            GameObject newSegment = Instantiate(aislePrefabs[segmentIndex], aisle.transform);
            newSegment.transform.position = spawnPosition;

            spawnPosition += new Vector3(0, 0, segmentLength);

            newSegment.GetComponent<AisleSegmentGenerator>().GenerateSegment();
            //aisleSegments.Add(newSegment);

            finalLength += segmentLength;
        }

        Debug.Log("Final Length = " + finalLength);

        return aisle;
    }
}
