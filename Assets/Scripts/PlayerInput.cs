using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{

    private TextInput playerInputActions;
    private bool isEnteringText = false;

    private void Awake()
    {
        playerInputActions = new TextInput();
        playerInputActions.Player.Enable();

        // Event-Listener f�r die Texteingabe
        playerInputActions.Player.Enter.started += _ => StartTextEntry();
        playerInputActions.Player.Enter.canceled += _ => EndTextEntry();
    }

    // Diese Methode gibteinen normalisierten Bewegungsvektor basierend auf den Spieler-Eingaben zur�ck.
    public Vector2 GetMovementVectorNormalized()
    {
        // Liest den Bewegungsvektor, der durch die Spieler-Eingaben bestimmt wird.
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();

        // Normalisiert den Vektor, um sicherzustellen, dass die Bewegungsgeschwindigkeit konstant bleibt.
        inputVector = inputVector.normalized;
        // Debug.Log(inputVector); // Gibt den Vektor zur �berpr�fung in die Konsole aus.
        return inputVector;
    }

    // Check if Jumpaction is activ
    public bool IsJumping()
    {
        return playerInputActions.Player.Jump.triggered;
    }

    private void StartTextEntry()
    {
        isEnteringText = true;
        // Deaktivieren anderer Aktionen
        playerInputActions.Player.Move.Disable();
        playerInputActions.Player.Jump.Disable();
    }
    private void EndTextEntry()
    {
        isEnteringText = false;
        // Aktivieren anderer Aktionen
        playerInputActions.Player.Move.Enable();
        playerInputActions.Player.Jump.Enable();
    }

    public bool IsEnteringText()
    {
        return isEnteringText;
    }
}


