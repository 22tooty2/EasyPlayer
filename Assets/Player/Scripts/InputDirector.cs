using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputDirector : MonoBehaviour
{
    //Crown
    public inputActionAsset InputActions;

    //Servants
    public Vector2 movementInput;
    public Vector2 cameraInput;

    public bool IsController = false;

    public delegate void firstButton();
    public event firstButton TriggerPressed;

    public delegate void InteractButton();
    public event InteractButton InteractPressed;

    public delegate void JumpButton();
    public event JumpButton JumpPressed;

    public bool isHoldingRun = false;

    //Helpers
    public bool canLook;
    public bool canMove;

    public void StartMoving()
    {
        InputActions = new inputActionAsset();
        InputActions.Enable();

        InputActions.MovingPlayer.Interact.performed += ctx => InteractPressed?.Invoke();
        InputActions.MovingPlayer.Shoot.performed += ctx => TriggerPressed?.Invoke();
        InputActions.MovingPlayer.Jump.started += ctx => JumpPressed?.Invoke();

        InputActions.MovingPlayer.ToggleRun.performed += ctx => isHoldingRun = !isHoldingRun;

        StarterManager.activatePlayer += EnableLooking;
        StarterManager.activatePlayer += EnableMoving;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartMoving();
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
            movementInput = InputActions.MovingPlayer.Movement.ReadValue<Vector2>();
        else
            movementInput = Vector2.zero;

        if (!IsController)
        {
            float runValue = InputActions.MovingPlayer.Run.ReadValue<float>();
            isHoldingRun = runValue > 0; // On the line before this one, if NOT holding run and run value <= 0 the player just stopped running.
        }

        if (canLook)
            cameraInput = InputActions.MovingPlayer.Mouse.ReadValue<Vector2>();
        else
            cameraInput = Vector2.zero;
    }

    void EnableLooking()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        canLook = true;
    }

    void EnableMoving()
    {
        canMove = true;
    }
}
