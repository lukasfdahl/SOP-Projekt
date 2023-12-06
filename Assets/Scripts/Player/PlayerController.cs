using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : PhysicsObject
{
    [Header("PlayerController")]
    public float maxSpeed = 6f;
    public float acceleration = 6f;
    public float slipCorrection = 6f;
    public float turnAroundRate = 3f;
    public float jumpSpeed = 15f;
    public float coyoteTime = 0.1f;
    public float jumpQueueTime = 0.1f;
    [Range(0,1)]public float jumpCancelRate = 0.5f;

    private PlayerInput input;
    private float moveX = 0f;

    private bool canJump = false;
    private bool isRunningCoyoteTime = false;

    private bool isJumpQueued = false;

    void Start()
    {
        input = new PlayerInput();
        input.Player.Enable();

        input.Player.Jump.started += Jump;
        input.Player.Jump.canceled += JumpCancel;
    }

    private void Update()
    {
        Vector2 move = Vector2.zero;

        MoveX();

        move.x = moveX;

        targetVelocity = move * maxSpeed;

        //Coyote Time
        if (grounded)
        {
            canJump = true;
            isRunningCoyoteTime = false;
            StopCoroutine(CoyoteTimer());

            //jump Queuing
            if (isJumpQueued)
            {
                StopCoroutine(JumpQueueTimer());
                Jump();
            }
            
        }
        else {
            if (!isRunningCoyoteTime)
            {
                StartCoroutine(CoyoteTimer());
            }
        }
    }

    private void Jump(InputAction.CallbackContext callbackContext)
    {
        if (canJump)
        {
            canJump = false;
            velocity.y = jumpSpeed;
        }
        else
        {
            isJumpQueued = true;
            StartCoroutine(JumpQueueTimer());

        }
    }
    private void Jump()
    {
        if (canJump)
        {
            canJump = false;
            velocity.y = jumpSpeed;
        }
        else
        {
            isJumpQueued = true;
            StartCoroutine(JumpQueueTimer());

        }
    }

    private void JumpCancel(InputAction.CallbackContext callbackContext)
    {
        if (velocity.y > 0)
        {
            velocity.y = velocity.y * jumpCancelRate;
        }
    }

    public void MoveX()
    {
        if (input.Player.Horizontal.ReadValue<float>() == -1) //left
        {
            if (moveX > 0) //if it starts the other direction
            {
                moveX -= acceleration * Time.deltaTime * turnAroundRate;
            }
            else
            {
                moveX -= acceleration * Time.deltaTime;
            }
        }
        else if (input.Player.Horizontal.ReadValue<float>() == 1) //right
        {
            if (moveX < 0) //if it starts the other direction
            {
                moveX += acceleration * Time.deltaTime * turnAroundRate;
            }
            else {
                moveX += acceleration * Time.deltaTime;
            }
        }
        else
        {
            if (moveX > 0)
            {
                if (moveX < slipCorrection * Time.deltaTime)
                    moveX = 0;
                else
                    moveX -= slipCorrection * Time.deltaTime;
            }
            else
            {
                if (moveX > -(slipCorrection * Time.deltaTime))
                    moveX = 0;
                else
                    moveX += slipCorrection * Time.deltaTime;
            }
        }

        moveX = Mathf.Clamp(moveX, -1, 1);
    }

    private IEnumerator CoyoteTimer()
    {
        yield return new WaitForSeconds(coyoteTime);
        canJump = false;
    }

    private IEnumerator JumpQueueTimer()
    {
        yield return new WaitForSeconds(jumpQueueTime);
        isJumpQueued = false;
    }
}
