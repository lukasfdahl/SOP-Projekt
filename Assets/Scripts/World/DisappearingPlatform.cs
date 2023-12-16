using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhysicsObject))]
[RequireComponent(typeof(SpriteRenderer))]
public class DisappearingPlatform : MonoBehaviour
{
    public float timeToDisapper;
    public float reappearTime;

    public Collider2D colliderComponent;

    private PhysicsObject physicsObject;
    private SpriteRenderer spriteRenderer;

    private float currentDisapperAmount;
    private bool isDisapering = false;

    private static float tickTime = 0.01f;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        physicsObject = GetComponent<PhysicsObject>();

        physicsObject.AddOnTagCollisionEnterEvent("Player", OnPlayerCollide);
    }

    private void OnPlayerCollide(GameObject hit)
    {
        if (!isDisapering)
        {
            isDisapering = true;
            currentDisapperAmount = timeToDisapper;
            StartCoroutine(DisapperTimer());
        }
    }

    private IEnumerator DisapperTimer()
    {
        yield return new WaitForSeconds(tickTime);

        if (currentDisapperAmount > 0)
        {
            currentDisapperAmount -= tickTime;
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, currentDisapperAmount / timeToDisapper);
            StartCoroutine(DisapperTimer());
        }
        else
        {
            colliderComponent.enabled = false;
            StartCoroutine(ReapperTime());
        }
    }
    private IEnumerator ReapperTime()
    {
        yield return new WaitForSeconds(reappearTime);
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1);
        colliderComponent.enabled = true;
        isDisapering = false;

    }

}
