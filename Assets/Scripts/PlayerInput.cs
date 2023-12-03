using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private TextInput playerInputActions;
    [SerializeField]
    private Canvas finalScreen;
    private bool isEnteringText = false;
    private bool jumpPressed = false;

    [SerializeField]
    private FetchFromModel fetchFromModel;

    private void Awake()
    {
        playerInputActions = new TextInput();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Jump.performed += OnJumpPerformed;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpPressed = true;
    }

   /* private void OnEnable()
    {
        playerInputActions.Enable();
    }

    private void OnDisable()
    {
        playerInputActions.Disable();
    }*/

    void Start()
    {
        // Event-Listener for text input
        playerInputActions.Player.Enter.started += _ => EnterPressed();
        playerInputActions.Player.Escape.started += _ => EscapePressed();
    }
    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        inputVector = inputVector.normalized;
        return inputVector;
    }

    // Check if Jumpaction is activ
    public bool IsJumping()
    {
        if (jumpPressed)
        {
            jumpPressed = false;
            return true;
        }
       return false;
    }

    private void EscapePressed()
    {
        if (isEnteringText)
        {
            EndTextEntry();
        }
        else
        {
            finalScreen.gameObject.SetActive(!finalScreen.gameObject.activeSelf);
        }
        Cursor.lockState = finalScreen.gameObject.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = finalScreen.gameObject.activeSelf;
        FindAnyObjectByType<MouseLook>().enabled = !finalScreen.gameObject.activeSelf;
    }

    private void EnterPressed()
    {
        fetchFromModel.EnterPressed();
        if (isEnteringText)
        {
            EndTextEntry();
        }
        else
        {
            StartTextEntry();
        }
    }

    private void StartTextEntry()
    {
        isEnteringText = true;
        // deactivate movement actions
        playerInputActions.Player.Move.Disable();
        playerInputActions.Player.Jump.Disable();
    }
    private void EndTextEntry()
    {
        isEnteringText = false;
        // activate movement actions
        playerInputActions.Player.Move.Enable();
        playerInputActions.Player.Jump.Enable();
    }

    public bool IsEnteringText()
    {
        return isEnteringText;
    }

    public bool IsEscapePressed()
    {
        return finalScreen.gameObject.activeSelf;
    }
}


