using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMover : GravityObject
{
    public float speed = 5f;

    private void Start()
    {
        targetVelocity = Vector2.left * speed;
    }
}
