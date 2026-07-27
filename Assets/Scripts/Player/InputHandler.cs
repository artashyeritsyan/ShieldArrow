using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class InputHandler : MonoBehaviour
{
    [Header("Character Input Values")]
    public Vector2 move;
    public Vector2 look;
    public bool attack;
    public bool jump;
    public bool sprint;

    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = false;
    public bool cursorInputForLook = true;

    [Header("Look Settings")]
    public float mouseSensitivity = 0.12f;
    public float stickSensitivity = 0.2f;

#if ENABLE_INPUT_SYSTEM
    private PlayerInput playerInput;
    private bool isGamepad;

    public bool IsGamepad => isGamepad;


    private void Awake()
    {
        cursorLocked = false;
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
        if (!cursorInputForLook)
            return;

        look = value.Get<Vector2>();
    }

#endif

    public void OnAttack(InputValue value)
    {
        AttackInput(value.isPressed);
        //Debug.Log("IsPRessed" + value.isPressed);
    }

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
        //Debug.Log(move);
    }

    public void LookInput(Vector2 newLookDirection)
    {
        look = newLookDirection;
    }

    public void AttackInput(bool newAttackState)
    {
        attack = newAttackState;
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

    private void SetCursorState(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;

        Cursor.visible = !locked;
    }
}