using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class InputHandler : MonoBehaviour
{
    [Header("Character Input Values")]
    public Vector2 move;
    public Vector2 look;
    public bool jump;
    public bool sprint;

    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

    [Header("Look Settings")]
    public float mouseSensitivity = 0.1f;
    public float stickSensitivity = 1.5f;

#if ENABLE_INPUT_SYSTEM
    private PlayerInput playerInput;
    private bool isGamepad;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        playerInput.onControlsChanged += OnControlsChanged;
    }
    private void OnDisable()
    {
        playerInput.onControlsChanged -= OnControlsChanged;
    }
    private void OnControlsChanged(PlayerInput input)
    {
        isGamepad = input.currentControlScheme == "Gamepad";
    }

    public void OnMove(InputValue value)
    {
        MoveInput(value.Get<Vector2>());
    }

#if ENABLE_INPUT_SYSTEM
    public void OnLook(InputValue value)
    {
        if (!cursorInputForLook) return;

        Vector2 rawInput = value.Get<Vector2>();

        if (isGamepad)
            look = rawInput * stickSensitivity;
        else
            look = rawInput * mouseSensitivity;
    }

#endif

    public void OnJump(InputValue value)
    {
        JumpInput(value.isPressed);
    }

    public void OnSprint(InputValue value)
    {
        SprintInput(value.isPressed);
    }
#endif

    public void MoveInput(Vector2 newMoveDirection)
    {
        move = newMoveDirection;
    }

    public void LookInput(Vector2 newLookDirection)
    {
        look = newLookDirection;
    }

    public void JumpInput(bool newJumpState)
    {
        jump = newJumpState;
    }

    public void SprintInput(bool newSprintState)
    {
        sprint = newSprintState;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
}