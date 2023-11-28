using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool isCrawling;
    public bool isTransitioning;

    [SerializeField] private CharacterController controller;
    [SerializeField] private float movingSpeed = 12f;
    [SerializeField] private float runningMultiplier = 1.8f;
    [SerializeField] private float crawlingMultiplier = 0.3f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 3f;
    private float crawlingHeight = 0.6f;
    private float originalHeight;
    private Vector3 originalScale;
    [SerializeField] private Vector3 normalCenter = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 crawlingCenter = new Vector3(0f, -0.7f, 0f);  // Adjusted for crawling
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform firstPersonBody;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    private Vector3 velocity;
    private bool isGrounded;
    

    private Coroutine crawlCoroutine; // Reference to the current crouch coroutine

    private void Start()
    {
        originalHeight = controller.height;
        originalScale = firstPersonBody.localScale;

    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (crawlCoroutine != null)
            {
                StopCoroutine(crawlCoroutine); // Stop the current coroutine if one is running
            }
            crawlCoroutine = StartCoroutine(ToggleCrawl());
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        float currentSpeed = movingSpeed;
        if (Input.GetKey(KeyCode.LeftShift) && !isCrawling) // Can only run if not crawling
        {
            currentSpeed *= runningMultiplier;
        }
        if (isCrawling) // Apply crawling speed multiplier if crawling
        {
            currentSpeed *= crawlingMultiplier;
        }

        controller.Move(move * currentSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded && !isCrawling) // Can only jump if not crawling
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime; // Apply gravity
        controller.Move(velocity * Time.deltaTime); // Apply velocity
    }

    private IEnumerator ToggleCrawl()
    {
        isTransitioning = true;

        // Determine the target values based on whether we're transitioning to or from crawling
        float targetHeight = isCrawling ? originalHeight : crawlingHeight;
        Vector3 targetCenter = isCrawling ? normalCenter : crawlingCenter;
        Vector3 targetScale = isCrawling ? originalScale : new Vector3(originalScale.x, originalScale.y * (crawlingHeight / originalHeight), originalScale.z);

        float timeToCrouch = 0.5f; // Time in seconds to complete the crouch/stand transition
        float elapsedTime = 0;

        // Start with the current values
        float currentHeight = controller.height;
        Vector3 currentCenter = controller.center;
        Vector3 currentScale = firstPersonBody.localScale;

        while (elapsedTime < timeToCrouch)
        {
            // Lerp from current to target values over time
            controller.height = Mathf.Lerp(currentHeight, targetHeight, (elapsedTime / timeToCrouch));
            controller.center = Vector3.Lerp(currentCenter, targetCenter, (elapsedTime / timeToCrouch));
            firstPersonBody.localScale = Vector3.Lerp(currentScale, targetScale, (elapsedTime / timeToCrouch));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final values are set precisely after interpolation
        controller.height = targetHeight;
        controller.center = targetCenter;
        firstPersonBody.localScale = targetScale;

        isCrawling = !isCrawling;
        isTransitioning = false;
    }


}
