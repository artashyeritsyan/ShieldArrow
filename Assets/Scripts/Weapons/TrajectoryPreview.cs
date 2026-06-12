using System.Collections.Generic;
using UnityEngine;

public class TrajectoryPreview : MonoBehaviour
{
    [Header("References")]
    public Transform firePoint;
    public Transform aimTarget;

    [Header("Ray Settings")]
    public float maxDistance = 20f;
    public int maxBounces = 5;
    public LayerMask hitMask;

    [Header("Dots")]
    public GameObject dotPrefab;
    public int maxDots = 100;
    public float dotsPerUnit = 5f;

    private List<Vector3> points = new();
    private List<GameObject> dots = new();

    void Start()
    {
        // Pre-create dot pool
        for (int i = 0; i < maxDots; i++)
        {
            GameObject dot = Instantiate(dotPrefab, transform);
            dot.SetActive(false);
            dots.Add(dot);
        }
    }

    void Update()
    {
        CalculateTrajectory();
        DrawDots();
    }

    void CalculateTrajectory()
    {
        points.Clear();

        Vector3 origin = firePoint.position;

        Vector3 direction = aimTarget.position - firePoint.position;
        direction.y = 0f;
        direction.Normalize();

        points.Add(origin);

        for (int i = 0; i < maxBounces; i++)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, hitMask))
            {
                points.Add(hit.point);

                direction = Vector3.Reflect(direction, hit.normal);

                origin = hit.point + direction * 0.01f;
            }
            else
            {
                points.Add(origin + direction * maxDistance);
                break;
            }
        }
    }

    void DrawDots()
    {
        int index = 0;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 start = points[i];
            Vector3 end = points[i + 1];

            float distance = Vector3.Distance(start, end);
            int steps = Mathf.CeilToInt(distance * dotsPerUnit);

            for (int j = 0; j < steps; j++)
            {
                if (index >= dots.Count)
                    return;

                float t = j / (float)steps;
                Vector3 pos = Vector3.Lerp(start, end, t);

                dots[index].transform.position = pos;
                dots[index].SetActive(true);

                index++;
            }
        }

        for (int i = index; i < dots.Count; i++)
        {
            if (dots[i].activeSelf)
                dots[i].SetActive(false);
        }
    }
}