using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

enum PlayerId
{
    singlePlayer,
    firstPlayer,
    secondPlayer
}

public class Player : MonoBehaviour
{
    [Header("Player")]


    [Header("Stats")]
    [SerializeField] float maxHp = 100f;
    [SerializeField] float speed = 100f;
    float currentHp;

    //[Header("Dash")]
    //[SerializeField] float dashForce = 200f;
    //[SerializeField] float dashRecoverTime = 1f;

    //[Header("Weapons")]
    //[SerializeField] GameObject[] weapons;
    //[SerializeField] float weaponSwitchDelay = 0.2f;

    [Header("Rotation")]
    [SerializeField] float rotationSpeed = 15f;
    private bool canRotate = false;

    [Header("Animation")]
    public Animator animator;

    Rigidbody rb;
    Vector3 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHp = maxHp;
        canRotate = false;
    }

    void Update()
    {
        HandleMovement();

        // Implement it to listen the event inRoom(bool). and if in it is true than set canRotate = true
        if (canRotate)
        {
            HandleRotation();
        }
        HandleAttack();
    }

    void HandleMovement()
    {
        rb.AddForce(moveDirection * speed * Time.deltaTime);

        if ((moveDirection == Vector3.zero))
        {
            animator.SetBool("Running", false);
        }
        else
        {
            animator.SetBool("Running", true);
        }
    }

    void HandleRotation()
    {
        if (moveDirection == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void HandleAttack()
    {
        //if (Mouse.current.leftButton.isPressed)
    }

    public void OnMove(InputValue input)
    {
        Vector2 move = input.Get<Vector2>();
        moveDirection = new Vector3(move.x, 0f, move.y);
    }



}