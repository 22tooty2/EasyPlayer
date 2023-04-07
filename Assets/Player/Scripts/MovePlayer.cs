using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    //Crown
    public Vector3 MoveValue;
    public Vector3 velocity = Vector3.zero;

    //Servants
    [SerializeField] GameObject Player;
    CharacterController cc;

    InputDirector director;
    CameraController camControl;
    GameObject MyCamera;

    public float Speed = 1f;
    public float Gravity = 10f;
    public Vector3 Direction;
    public float SpeedMultiplier = 3f;
    public float JumpHeight = 10f;

    public bool isJumping;
    public bool trackJumpFall; // To change and detect when landed

    //Helpers
    Vector3 currentTargetRotation;
    public Vector3 timeToReachTargetRotation; // 0, 0.14, 0

    Vector3 dumpedVelocity;
    Vector3 dumpedVelocityPassedTime;

    const float GROUNDED_THRESHOLD = -0.2f;

    public delegate void StartJumping();
    public event StartJumping StartedJump;

    public delegate void StartFalling();
    public event StartFalling StartedFall;

    public delegate void Landing();
    public event Landing Landed;


    // Start is called before the first frame update
    void Start()
    {
        director = GetComponent<InputDirector>();
        camControl = GetComponent<CameraController>();
        MyCamera = camControl.MyCamera;
        cc = Player.GetComponent<CharacterController>();

        director.JumpPressed += Jump;
    }

    // Update is called once per frame
    void Update()
    {
        calculateMovement();
        calculateGravity();
        ExtraDetectables();
        cc.Move(MoveValue);
    }

    private void calculateMovement()
    {
        MoveValue = Vector3.zero; // Starting from Zero

        // Is it even moving?
        if (director.movementInput.x == 0 && director.movementInput.y == 0)
            return;

        // get forward Direction
        Vector3 forwardDirection = camControl.cameraController.transform.forward;
        Vector3 RightDirection = camControl.cameraController.transform.right;

        // Get Input direction
        Direction =  new Vector3(director.movementInput.x, 0f, director.movementInput.y);

        // For rotating the player (towards the camera direction)
        float targetRotation = RotatePlayer(Direction);

        Vector3 targetRotationDirection = GetTargetRotationDirection(targetRotation);

        // For if the player is running (pressing shift or toggled run on controller)
        float moveSpeed = Speed;
        if (director.isHoldingRun)
            moveSpeed *= SpeedMultiplier;

        // Final move value
        MoveValue = targetRotationDirection * moveSpeed * Time.deltaTime;
    }

    private float RotatePlayer(Vector3 _direction)
    {
        float directionAngle = UpdateTargetRotation(_direction);

        RotateTowardsTargetRotation();

        return directionAngle;
    }

    private void rotateTowardsTargetRotation()
    {
        float currentAngle = Player.transform.rotation.eulerAngles.y;

        if (currentAngle == currentTargetRotation.y)
            return;

        float smoothedAngle = Mathf.SmoothDampAngle(currentAngle, currentTargetRotation.y, ref dumpedVelocity.y, timeToReachTargetRotation.y - dumpedVelocityPassedTime.y);
        dumpedVelocityPassedTime.y += Time.deltaTime;

        Vector3 targetRotation = new Vector3(0f, smoothedAngle, 0f);

        Player.transform.Rotate(targetRotation);
    }

    protected float UpdateTargetRotation(Vector3 direction, bool shouldConsiderCameraRotation = true)
    {
        float directionAngle = GetDirectionAngle(direction);

        if (shouldConsiderCameraRotation)
        {
            directionAngle = AddCameraRotationToAngle(directionAngle);
        }

        if (directionAngle != currentTargetRotation.y)
        {
            UpdateTargetRotationData(directionAngle);
        }

        return directionAngle;
    }

    private float GetDirectionAngle(Vector3 direction)
    {
        float directionAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        if (directionAngle < 0f)
        {
            directionAngle += 360f;
        }

        return directionAngle;
    }

    private float AddCameraRotationToAngle(float angle)
    {
        angle += MyCamera.transform.eulerAngles.y;

        if (angle > 360f)
        {
            angle -= 360f;
        }

        return angle;
    }

    private void UpdateTargetRotationData(float targetAngle)
    {
        currentTargetRotation.y = targetAngle;

        dumpedVelocityPassedTime.y = 0f;
    }

    protected void RotateTowardsTargetRotation()
    {
        float currentYAngle = Player.transform.rotation.eulerAngles.y;

        if (currentYAngle == currentTargetRotation.y)
        {
            return;
        }

        float smoothedYAngle = Mathf.SmoothDampAngle(currentYAngle, currentTargetRotation.y, ref dumpedVelocity.y, timeToReachTargetRotation.y - dumpedVelocityPassedTime.y);

        dumpedVelocityPassedTime.y += Time.deltaTime;

        Quaternion targetRotation = Quaternion.Euler(0f, smoothedYAngle, 0f);

        Player.transform.rotation = targetRotation;
    }

    protected Vector3 GetTargetRotationDirection(float targetRotationAngle)
    {
        return Quaternion.Euler(0f, targetRotationAngle, 0f) * Vector3.forward;
    }

    private void calculateGravity()
    {   
        if (!cc.isGrounded)
            velocity.y -= Gravity * Time.deltaTime;

        StayOnGround(); // The player will have the default gravity if on ground.

        MoveValue.y = velocity.y * Time.deltaTime;
    }

    // Jump
    void Jump()
    {
        if (cc.isGrounded && !isJumping)
        {
            isJumping = true;
            velocity.y = Mathf.Sqrt(2f * JumpHeight * Gravity);
            StartedJump?.Invoke();
        }
    }

    void StayOnGround()
    {
        if (cc.isGrounded && velocity.y < 0 && !isJumping)
        {
            velocity.y = GROUNDED_THRESHOLD;

            if (trackJumpFall)
            {
                trackJumpFall = false;
            }
        }
    }

    void ExtraDetectables()
    {
        // Early land detect
        if (trackJumpFall)
        { // I want to trigger the landing animation a little earlier.
            if (Physics.Raycast(Player.transform.position, Vector3.down, 0.5f))
            {
                Landed?.Invoke();
            }
        }

        // Walked from edge (to fall) detect
        if (!cc.isGrounded && !isJumping)
        {
            trackJumpFall = true;
            StartedFall?.Invoke();
        }

        // At the highest point of the Jump detect
        if (velocity.y < 0 && isJumping)
        {
            isJumping = false;
            StartedFall?.Invoke();

            trackJumpFall = true;
        }   
    }
}



