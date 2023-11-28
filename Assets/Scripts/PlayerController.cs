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
    private void Update()
    {
        if (playerInput.IsEnteringText()) return;

        Vector2 inputVector = playerInput.GetMovementVectorNormalized();

        //Eingaben auf X und Z Achse ummoddeln und auf 3d Vector
        Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);

        transform.position += moveDirection * movingSpeed * Time.deltaTime;
        //Rotation des Players zur Movement Richtung
        //transform.forward = moveDirection;

        isMoving = moveDirection != Vector3.zero;

        float rotateSpeed = 10f;
        //Rotation Smoother machen
        transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);

        //JumpLogic
        if (playerInput.IsJumping() && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Playground"))
        {
            isGrounded = true;
        }
    }

    public bool IsMoving()
    {
        return isMoving;
    }

}
