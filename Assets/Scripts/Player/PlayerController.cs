using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : GravityObject
{
    [Header("PlayerController")]
    public float maxSpeed = 6f;
    public float acceleration = 6f;
    public float slipCorrection = 6f;
    public float turnAroundRate = 3f;
    public float InitialJumpSpeed = 15f;
    public float coyoteTime = 0.1f;
    public float jumpQueueTime = 0.1f;
    [Range(0,1)]public float jumpCancelRate = 0.5f;

    private PlayerInput input;
    private float moveX = 0f;

    private bool canJump = false;
    private bool isRunningCoyoteTime = false;
    private bool isJumping;

    private bool isJumpQueued = false;

    //Testing
    public float normalGravity;
    public float cancelGravity;

    void Start()
    {
        input = new PlayerInput();
        input.Player.Enable();

        input.Player.Jump.started += Jump;
        input.Player.Jump.canceled += JumpCancel;

        AddOnTagCollisionEnterEvent("Spike", OnSpikeHit);
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
            isJumping = false;
            gravityModifier = normalGravity;
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
        else
        {
            //for at have højere tyngdekræft når man falder fra et hop
            if (isJumping && velocity.y < 0)
            {
                gravityModifier = cancelGravity;
            }

            if (!isRunningCoyoteTime)
            {
                StartCoroutine(CoyoteTimer());
            }
        }
    }

    private void Jump(InputAction.CallbackContext callbackContext)
    {
        Jump();
    }
    private void Jump()
    {
        if (canJump)
        {
            canJump = false;
            isJumping = true;
            velocity.y = InitialJumpSpeed;
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
            gravityModifier = cancelGravity;
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

    public void OnSpikeHit(RaycastHit2D hit)
    {
        Debug.Log("HitSpike");
    }
}
