using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform target;
    [SerializeField] InputHandler input;   // <-- reference to your input system

    [Header("Follow")]
    [SerializeField] float followSpeed = 10f;
    [SerializeField] Vector3 offset = new Vector3(0f, 5f, -7f);

    [Header("Orbit")]
    [SerializeField] float mouseSensitivity = 100f;
    [SerializeField] float minPitch = -10f;
    [SerializeField] float maxPitch = 70f;
    [SerializeField] bool freeRotate = false;


    float yaw;
    float pitch;

    void Start()
    {
        yaw = 0f;
        pitch = 11f;

        offset = transform.position - target.position;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null || input == null) return;

        HandleRotation();
        HandlePosition();
    }

    void HandleRotation()
    {
        Vector2 lookInput = input.look;

        // If using mouse, scale with deltaTime OFF (raw input)
        float deltaMultiplier = input.cursorInputForLook ? 1f : Time.deltaTime;

        yaw += lookInput.x * mouseSensitivity * deltaMultiplier;
        pitch -= lookInput.y * mouseSensitivity * deltaMultiplier;

        if (freeRotate)
        {
            pitch = Mathf.Clamp(pitch, 0, 360);
        }
        else {
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
        
    }

    void HandlePosition()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position + rotation * offset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        transform.LookAt(target.position);
    }
}