//using UnityEngine;

//using UnityEngine;
//using UnityEngine.InputSystem;

//public class InputHandler : MonoBehaviour
//{
//    public PlayerController player1;
//    public PlayerController player2;

//    public bool isTwoPlayer = false;

//    private PlayerInputActions input;

//    private void Awake()
//    {
//        input = new PlayerInputActions();
//    }

//    private void OnEnable() => input.Enable();
//    private void OnDisable() => input.Disable();

//    private void Update()
//    {
//        HandlePlayer1();

//        if (isTwoPlayer)
//            HandlePlayer2();
//    }

//    // ---------------- PLAYER 1 (Keyboard + Mouse OR fallback gamepad) ----------------
//    void HandlePlayer1()
//    {
//        Vector2 move = input.Player.Move.ReadValue<Vector2>();

//        if (move.magnitude < 0.1f)
//            move = Vector2.zero;

//        player1.SetMoveInput(move);

//        if (input.Player.Jump.WasPressedThisFrame())
//            player1.Jump();

//        if (input.Player.Attack.WasPressedThisFrame())
//            player1.Attack();
//    }

//    // ---------------- PLAYER 2 (Gamepad ONLY) ----------------
//    void HandlePlayer2()
//    {
//        Vector2 move = input.Player.GamepadMove.ReadValue<Vector2>();

//        if (move.magnitude < 0.1f)
//            move = Vector2.zero;

//        player2.SetMoveInput(move);

//        if (input.Player.GamepadJump.WasPressedThisFrame())
//            player2.Jump();

//        if (input.Player.GamepadAttack.WasPressedThisFrame())
//            player2.Attack();
//    }
//}