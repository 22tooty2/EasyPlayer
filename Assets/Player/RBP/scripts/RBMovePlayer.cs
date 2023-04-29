using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBMovePlayer : MonoBehaviour
{
    //Crown
    public Vector3 MoveValue;
    public Vector3 velocity = Vector3.zero;

    //Servants
    [SerializeField] GameObject Player;
    Rigidbody rb;

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
    public bool _isGrounded;
    public LayerMask groundMask;

    //Helpers
    Vector3 currentTargetRotation;
    public Vector3 timeToReachTargetRotation; // 0, 0.14, 0

    Vector3 dumpedVelocity;
    Vector3 dumpedVelocityPassedTime;

    const float GROUNDED_THRESHOLD = -0.2f;

    public delegate void StartJumping();
    public event StartJumping StartedJump;

    public delegate void Landing();
    public event Landing Landed;

    public delegate void FallingChangedEvent(bool status);
    public event FallingChangedEvent FallingBoolChanged;

    public bool isFalling; // Walked off the edge

    //Knockback
    public float knockbackStrength;
    public Vector3 knockbackPower;

    public bool IsFalling
    {
        get
        {
            return isFalling;
        }
        set
        {
            isFalling = value;
            if (FallingBoolChanged != null)
            {
                FallingBoolChanged(isFalling);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        director = GetComponent<InputDirector>();
        camControl = GetComponent<CameraController>();
        MyCamera = camControl.MyCamera;
        rb = Player.GetComponent<Rigidbody>();

        director.JumpPressed += Jump;
        StarterManager.knockbackPlayer += knockback;
    }

    // Update is called once per frame
    void Update()
    {
        _isGrounded = isGrounded();
        calculateMovement();
        calculateGravity();
        ExtraDetectables();
        rb.AddForce(MoveValue);
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
        Direction = new Vector3(director.movementInput.x, 0f, director.movementInput.y);

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
        if (!_isGrounded)
            rb.AddForce(Vector3.down * Gravity * Time.deltaTime);

        StayOnGround(); // The player will have the default gravity if on ground.

        //MoveValue.y = velocity.y * Time.deltaTime;
    }

    // Jump
    void Jump()
    {
        if (_isGrounded && !isJumping)
        {
            isJumping = true;
            velocity.y = Mathf.Sqrt(2f * JumpHeight * Gravity);
            StartedJump?.Invoke();
        }
    }

    void StayOnGround()
    {
        if (_isGrounded && rb.velocity.y < 0 && !isJumping)
        {
            Vector3 tempVelocity = rb.velocity;
            tempVelocity.y = GROUNDED_THRESHOLD;
            rb.velocity = tempVelocity;

            if (trackJumpFall)
            {
                trackJumpFall = false;
            }

            isFalling = false;
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
                isFalling = false;
                trackJumpFall = false;
                Debug.Log("Touching! " + Time.time);
            }
        }

        // Walked from edge (to fall) detect
        if (!_isGrounded && !isJumping)
        {
            isFalling = true;
        }

        // At the highest point of the Jump detect
        if (velocity.y < 0 && isJumping)
        {
            isJumping = false;
            isFalling = true;

            trackJumpFall = true;
        }

        // Move Knockback
        if (knockbackPower.y <= 0f)
        {
            knockbackPower = Vector3.zero;
            velocity.x = 0f;
            velocity.z = 0f;
        }
        else
        {
            float yTemp = velocity.y;
            knockbackPower = knockbackPower.normalized * (knockbackPower.magnitude - (knockbackStrength * Time.deltaTime));
            velocity = knockbackPower;
            velocity.y = yTemp;
        }
        MoveValue += velocity * Time.deltaTime;
    }

    public void knockback(Vector3 dir, float force)
    {
        knockbackPower = dir.normalized * force;
        knockbackPower.y = force;
        velocity += knockbackPower;
        Debug.Log(knockbackPower);
    }

    bool isGrounded()
    {
        bool grounded = Physics.CheckSphere(gameObject.transform.position, 0.1f, groundMask);

        RaycastHit hit;
        if (grounded && Physics.Raycast(gameObject.transform.position, Vector3.down, out hit, 0.2f, groundMask))
        {
            gameObject.GetComponentInParent<PlayerInfo>().isGrounded = true;
            return true;
        }

        gameObject.GetComponentInParent<PlayerInfo>().isGrounded = false;
        return false;
    }
}
