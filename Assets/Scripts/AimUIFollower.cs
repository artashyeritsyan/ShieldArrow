using UnityEngine;

public class AimUIFollow : MonoBehaviour
{
    private Transform target;
    private Camera cam;

    [Header("Settings")]
    [SerializeField] Vector3 offset = Vector3.up * 1.5f;
    [SerializeField] bool hideIfBehindCamera = true;

    void Awake()
    {
        cam = Camera.main;
    }

    // Called by PlayerController after spawn
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void LateUpdate()
    {
        if (target == null || cam == null)
            return;

        Vector3 worldPos = target.position + offset;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        // Handle behind camera
        if (screenPos.z < 0)
        {
            if (hideIfBehindCamera)
            {
                gameObject.SetActive(false);
                return;
            }
        }
        else
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        transform.position = screenPos;
    }
}