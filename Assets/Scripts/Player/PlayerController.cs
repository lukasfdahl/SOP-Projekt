using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : PhysicsObject
{
    public float maxSpeed = 7f;
    public float jumpSpeed = 7f;
    [Range(0,1)]public float jumpCancelRate = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        Vector2 move = Vector2.zero;
        move.x = Input.GetAxis("Horizontal");

        if (Input.GetButtonDown("Jump") && grounded)
        {
            velocity.y = jumpSpeed;
        }
        else if (Input.GetButtonUp("Jump")) //for canceling jump
        {
            if (velocity.y > 0)
            {
                velocity.y = velocity.y * jumpCancelRate;
            }
        }

        targetVelocity = move * maxSpeed;
    }
}
