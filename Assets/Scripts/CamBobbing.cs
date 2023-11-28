using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamBobbing : MonoBehaviour
{
    [SerializeField] private float bobbingSpeed = 0.05f;
    [SerializeField] private float bobbingAmount = 0.35f;
    [SerializeField] private float midpoint = 2.0f;

    [SerializeField] private float speedMultiplier = 1.8f; // Multiplier for running speed
    [SerializeField] private float amountMultiplier = 1.08f;

    [SerializeField] private PlayerMovement playerMovement;

    private float timer = 0.0f;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponentInParent<CharacterController>();
    }

    void Update()
    {
        if (controller.isGrounded && !playerMovement.isCrawling)
        {
            float waveslice = 0.0f;
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            bool isRunning = Input.GetKey(KeyCode.LeftShift) && !playerMovement.isCrawling; // Check if running

            // Apply multiplier if running
            float currentBobbingSpeed = isRunning ? bobbingSpeed * speedMultiplier : bobbingSpeed;
            float currentBobbingAmount = isRunning ? bobbingAmount * amountMultiplier : bobbingAmount;

            Vector3 cSharpPosition = transform.localPosition;

            if (Mathf.Abs(horizontal) == 0 && Mathf.Abs(vertical) == 0)
            {
                timer = 0.0f;
            }
            else
            {
                waveslice = Mathf.Sin(timer);
                timer = timer + currentBobbingSpeed;
                if (timer > Mathf.PI * 2)
                {
                    timer = timer - (Mathf.PI * 2);
                }
            }

            if (waveslice != 0)
            {
                float translateChange = waveslice * currentBobbingAmount;
                float totalAxes = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
                totalAxes = Mathf.Clamp(totalAxes, 0.0f, 1.0f);
                translateChange = totalAxes * translateChange;
                cSharpPosition.y = midpoint + translateChange;
            }
            else
            {
                cSharpPosition.y = midpoint;
            }

            transform.localPosition = cSharpPosition;
        }
        else
        {
            timer = 0.0f; // Reset timer when the player is not grounded
        }
    }
}
