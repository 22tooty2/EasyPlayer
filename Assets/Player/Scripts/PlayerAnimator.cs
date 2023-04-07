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
        movePlayer.StartedFall += MovePlayer_StartedFall;
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

        //Animate
        animator.SetFloat("Movement", movementDiff);
        animator.SetBool("IsRunning", isRunning);
    }

    private void MovePlayer_StartedJump()
    {
        animator.SetTrigger("PrepJump");
    }

    private void MovePlayer_StartedFall()
    {
        animator.SetTrigger("Falling");
    }

    private void MovePlayer_Landed()
    {
        animator.SetTrigger("Landing");
    }
}
