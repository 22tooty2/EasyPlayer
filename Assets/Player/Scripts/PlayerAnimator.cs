using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    //Crown
    [SerializeField] Animator animator;

    //Key
    public bool CanAnimate;

    //Servants
    InputDirector director;
    MovePlayer movePlayer;
    Vector2 movement;
    bool isRunning;


    // Start is called before the first frame update
    void Start()
    {
        director = GetComponent<InputDirector>();
        movePlayer = GetComponent<MovePlayer>();

        movePlayer.StartedJump += MovePlayer_StartedJump;
        movePlayer.FallingBoolChanged += MovePlayer_StartedFall;
        movePlayer.Landed += MovePlayer_Landed;
    }

    // Update is called once per frame
    void Update()
    {
        //Throne Room
        if (!CanAnimate)
            return;

        //Collect Data
        movement = director.movementInput;
        isRunning = director.isHoldingRun;

        //Calculate Vector2 Movement Direction
        float movementDiff = 0f;

        // Pethagoreas to get strength
        movementDiff = movement.sqrMagnitude;

        if (movementDiff == 0)
            isRunning = false;

        //Animate
        animator.SetFloat("Movement", movementDiff);
        animator.SetBool("Running", isRunning);
    }

    private void MovePlayer_StartedJump()
    {
        animator.SetTrigger("Jump");
    }

    private void MovePlayer_StartedFall(bool status)
    {
        animator.SetBool("Falling", status);
    }

    private void MovePlayer_Landed()
    {
        animator.SetTrigger("Landing");
    }
}
