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
    public bool interact;

    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = false;
    public bool cursorInputForLook = true;

    [Header("Look Settings")]
    public float mouseSensitivity = 0.12f;
    public float stickSensitivity = 0.2f;
    [SerializeField] private float stickCursorSpeed = 800f;

    private Vector2 rawStickInput;
    private Vector2 virtualCursorPosition;

#if ENABLE_INPUT_SYSTEM
    private PlayerInput playerInput;
    private bool isGamepad;

    public bool IsGamepad => isGamepad;


    private void Awake()
    {
        cursorLocked = false;
        playerInput = GetComponent<PlayerInput>();

        virtualCursorPosition = new Vector2(
            Screen.width * 0.5f,
            Screen.height * 0.5f
        );

        look = virtualCursorPosition;
    }

    private void Update()
    {
        if (!isGamepad)
            return;

        virtualCursorPosition +=
            rawStickInput *
            stickCursorSpeed *
            Time.deltaTime;

        virtualCursorPosition.x = Mathf.Clamp(
            virtualCursorPosition.x,
            0f,
            Screen.width
        );

        virtualCursorPosition.y = Mathf.Clamp(
            virtualCursorPosition.y,
            0f,
            Screen.height
        );

        look = virtualCursorPosition;
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

        if (isGamepad)
        {
            virtualCursorPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            look = virtualCursorPosition;
        }
        else
        {
            rawStickInput = Vector2.zero;
        }
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

        Vector2 newLookInput = value.Get<Vector2>();

        if (isGamepad)
        {
            // Gamepad returns a direction from -1 to 1.
            rawStickInput = newLookInput;
        }
        else
        {
            // Mouse returns its screen position in pixels.
            look = newLookInput;
        }
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

    public void OnInteract(InputValue value)
    {

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

    public void InteractInput(bool newInteractState)
    {
        interact = newInteractState;
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