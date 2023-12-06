using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public static Level current { get; private set; }

    public float gravity = 1f;

    private void Awake()
    {
        current = this;
    }

}
