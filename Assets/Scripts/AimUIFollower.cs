using UnityEngine;

public class AimUIFollower : MonoBehaviour
{
    public Transform target;
    public Camera cam;

    void Update()
    {
        Vector3 screenPos = cam.WorldToScreenPoint(target.position);
        transform.position = screenPos;
    }
}