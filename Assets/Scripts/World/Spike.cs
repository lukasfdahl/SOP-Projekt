using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhysicsObject))]
public class Spike : MonoBehaviour
{
    public Transform respawnPoint;

    private PhysicsObject physicsObject;

    private void Start()
    {
        physicsObject = GetComponent<PhysicsObject>();

        physicsObject.AddOnTagCollisionEnterEvent("Player", OnPlayerCollide);
    }

    private void OnPlayerCollide(GameObject hit)
    {
        hit.GetComponent<PlayerController>().Respawn(respawnPoint.position);
    }
}
