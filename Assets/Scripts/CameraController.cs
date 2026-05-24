using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] Transform target;

    [Header("Follow")]
    [SerializeField] float followSpeed = 10f;
    [SerializeField] Vector3 offset = new Vector3(0f, 5f, -7f);

    [Header("Orbit")]
    [SerializeField] float mouseSensitivity = 2f;
    [SerializeField] float minPitch = -10f;
    [SerializeField] float maxPitch = 70f;

    float yaw;    // left / right
    float pitch;  // up / down

    void Start()
    {
        yaw = 0;
        pitch = 11;

        offset = transform.position - target.position;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        //HandleRotation();
        HandlePosition();
    }

    void HandleRotation()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        yaw += mouseDelta.x * mouseSensitivity;
        pitch -= mouseDelta.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    void HandlePosition()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position + rotation * offset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        transform.LookAt(target.position);
    }
}