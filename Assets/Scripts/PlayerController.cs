using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float movingSpeed = 20f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private PlayerInput playerInput;

    private Rigidbody rb;
    private bool isMoving;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (playerInput.IsEnteringText()) return;

        Vector2 inputVector = playerInput.GetMovementVectorNormalized();

        Vector3 moveDirection = transform.right * inputVector.x + transform.forward * inputVector.y;
        moveDirection.y = 0; // Ignoriert die Y-Achse f�r Bewegung auf der Ebene

        transform.position += moveDirection * movingSpeed * Time.deltaTime;

        isMoving = moveDirection != Vector3.zero;


        //JumpLogic
        if (playerInput.IsJumping() && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
    }
    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

}
