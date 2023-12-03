using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMover : PhysicsObject
{
    public float speed = 5f;

    private void Start()
    {
        targetVelocity = Vector2.left * speed;
    }
}
