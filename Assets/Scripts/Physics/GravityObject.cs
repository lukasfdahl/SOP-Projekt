using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class GravityObject : PhysicsObject
{
    [Header("GravityObject")]
    public float minGroundNormalY = 0.65f; //the max angle between a GravityObject and a slope, that will count as grounded. between 0 and 1
    public float gravityModifier = 1f;
    public float terminalVelocityY = 80f; //how much an object is allowed to move on the y axis in units pr. second
    public float stepHeight = 0.1f;

    protected bool grounded = false;
    protected Vector2 groundNormal;

    protected Vector2 targetVelocity; //the velocity that the GravityObject should try to apply. (used for most movement)
    protected Rigidbody2D rb2d;
    protected Vector2 velocity;
    protected ContactFilter2D contactFilter; //used to handle collisions when layers are involved
    protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(); //only has the RaycastHit2Ds that hit something

    protected const float minMoveDistance = 0.001f;
    protected const float shellRadius = 0.01f; //the padding between 2 colliders. It prevents colliders from getting stuck in each other
    protected const float stepExtra = 0.05f;

    protected float relativeBottomY = float.MaxValue; //the lowest y point on the GravityObject in relation to the position of the GravityObject

    #region collision Actions
    protected List<GameObject> currentHits = new List<GameObject>();
    protected List<GameObject> prevHitsX = new List<GameObject>();
    protected List<GameObject> prevHitsY = new List<GameObject>();
    #endregion


    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>(); //får rigidbody komponentet og gemmer det i en variabel
        contactFilter.useTriggers = false; //får den til at ignorære colliders med isTrigger sat til true
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer)); //sætter layermask til de lag gameobjectet kan kollidere med
        contactFilter.useLayerMask = true;

        //finder bund punktet af en rigidbody2d
        List<Collider2D> colliders = new List<Collider2D>();
        rb2d.GetAttachedColliders(colliders);

        foreach (Collider2D collider in colliders)
        {
            float currentBottom = collider.bounds.min.y - transform.position.y;
            if (currentBottom < relativeBottomY)
            {
                relativeBottomY = currentBottom;
            }

        }
    }

    private void FixedUpdate()
    {
        velocity += gravityModifier * Physics2D.gravity * Time.deltaTime; //udregner tyngdekræften for objektet
        velocity.x = targetVelocity.x; //tilføjer horizontal bevægelse baseret på targetVelocity

        grounded = false; //er sat flask her, og så sat sandt i Movement hvis spilleren kollidere med jorden

        Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x); //finder tværvektoren til jordens normal vektor (linjen der går langs en overflade)
        Vector2 deltaPosition = velocity * Time.deltaTime; //ændrigen der skal ske i position pga. velocity, denne frame

        //X og Y movement er håndteret seperart fordi det gør det nemmere at håndtere slopes. dog er det i en funktion da de stadig deler meget af den samme kode
        //X movement
        Vector2 moveAmount = moveAlongGround * deltaPosition.x;
        Movement(moveAmount, false);

        //Y Movement
        moveAmount = Vector2.up * deltaPosition;


        #region TerminalVelocity

        float deltaTerminalVelocityY = terminalVelocityY * Time.deltaTime;

        if (moveAmount.y > deltaTerminalVelocityY)
            moveAmount.y = deltaTerminalVelocityY;

        else if (moveAmount.y < -deltaTerminalVelocityY)
            moveAmount.y = -deltaTerminalVelocityY;

        #endregion


        Movement(moveAmount, true);
    }

    /// <summary>
    /// håndtere bevægelse af GravityObject
    /// </summary>
    /// <param name="moveAmount">mængden den skal bevæge sig</param>
    /// <param name="yMovement">sand hvis det er den skal udregene bevægelse på y aksen, ellers er den falsk</param>
    protected virtual void Movement(Vector2 moveAmount, bool yMovement)
    {
        #region CollisionCheck

        float distance = moveAmount.magnitude;

        if (distance > minMoveDistance)
        {
            hitBufferList.Clear();
            rb2d.Cast(moveAmount, contactFilter, hitBufferList, distance + shellRadius); //caster alle en rigdigdbodys collidere i en hvis renting med en hvis mængde

            //til kollision actions
            currentHits.Clear();

            foreach (RaycastHit2D hit in hitBufferList)
            {
                currentHits.Add(hit.collider.gameObject);

                Vector2 currentNormal = hit.normal;
                float hitY = hit.point.y - transform.position.y;
                if (currentNormal.y > minGroundNormalY)
                {
                    grounded = true;
                    if (yMovement)
                    {
                        groundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                    //for at håndtere setpping over bump der er mindre end shellRadius
                    else if (hitY - relativeBottomY + shellRadius > 0)
                    {
                        rb2d.position += new Vector2(0, hitY - relativeBottomY + shellRadius + 0.01f);
                        currentNormal = Vector2.up;
                    }
                }

                //for at håndtere generel stepping 
                else if (hitY - relativeBottomY + shellRadius > 0 && hitY - relativeBottomY + shellRadius < stepHeight)
                {
                    rb2d.position += new Vector2(0, hitY - relativeBottomY + shellRadius + stepExtra);
                    currentNormal = Vector2.up;
                }

                //hvis man kollidere med tag som er en slope, så glider man langs det istedet for at man mister alt momentum og falder ned.
                //her bruger jeg skalarprodukt til at finde længden af velocity projekteret på currentnormal
                //hvis projektionen er negativ betyder det at der er kollideret med en skrå overflade
                //her fjerner jeg så momentum baseret på projectionen så de ikke ryger igennem taget
                float projection = Vector2.Dot(velocity, currentNormal);
                if (projection < 0)
                {
                    velocity = velocity - projection * currentNormal;
                }

                float modifiedDistance = hit.distance - shellRadius;

                if (modifiedDistance < distance)
                {
                    distance = modifiedDistance;
                }
            }
        }

        #endregion


        #region collisionActions
        foreach (GameObject hit in currentHits)
        {
            string hitTag = hit.tag;

            PhysicsObject other = hit.GetComponent<PhysicsObject>();
            bool isCollidingWithPhysicsObject = other != null;

            if (prevHitsX.Contains(hit) || prevHitsY.Contains(hit))
            {
                //kollsion for dette objeckt
                //for at køre hitTag kollisions systemet
                if (onTagCollisionStay.ContainsKey(hitTag))
                {
                    onTagCollisionStay[hitTag](hit);
                }
                //for at køre normal kollision
                if (onCollisionStay != null)
                    onCollisionStay(hit);


                //kollsion for det den rammer
                if (isCollidingWithPhysicsObject)
                {
                    //for at køre hitTag kollisions systemet
                    if (other.onTagCollisionStay.ContainsKey(tag))
                    {
                        other.onTagCollisionStay[tag](gameObject);
                    }
                    //for at køre normal kollision
                    if (other.onCollisionStay != null)
                        other.onCollisionStay(gameObject);
                }

                if (yMovement)
                    prevHitsX.Remove(hit);
                else
                    prevHitsY.Remove(hit);
            }
            else
            {
                //for at køre hitTag kollisions systemet
                if (onTagCollisionEnter.ContainsKey(hitTag))
                {
                    onTagCollisionEnter[hitTag](hit);
                }

                //for at køre normal kollision
                if (onCollisionEnter != null)
                    onCollisionEnter(hit);


                if (isCollidingWithPhysicsObject)
                {
                    //for at køre hitTag kollisions systemet
                    if (other.onTagCollisionEnter.ContainsKey(tag))
                    {
                        other.onTagCollisionEnter[tag](gameObject);
                    }
                    //for at køre normal kollision
                    if (other.onCollisionEnter != null)
                        other.onCollisionEnter(gameObject);
                }
            }
        }

        if (yMovement)
        {
            foreach (GameObject hit in prevHitsX)
            {
                //for at sikre at exit kun bliver kørt hvis objektet forlader kollideren fuldkommen. køre ikke hvis det bare er hit pisitionen der ændre sig
                if (prevHitsX.Contains(hit) || prevHitsY.Contains(hit))
                {
                    string hitTag = hit.tag;

                    PhysicsObject other = hit.GetComponent<PhysicsObject>();
                    bool isCollidingWithPhysicsObject = other != null;

                    //for at køre hitTag kollisions systemet
                    if (onTagCollisionExit.ContainsKey(hitTag))
                    {
                        onTagCollisionExit[hitTag](hit);
                    }

                    //for at køre normal kollision
                    if (onCollisionExit != null)
                        onCollisionExit(hit);


                    if (isCollidingWithPhysicsObject)
                    {
                        //for at køre hitTag kollisions systemet
                        if (other.onTagCollisionExit.ContainsKey(tag))
                        {
                            other.onTagCollisionExit[tag](gameObject);
                        }
                        //for at køre normal kollision
                        if (other.onCollisionExit != null)
                            other.onCollisionExit(gameObject);
                    }
                }
            }
        }
        else
        {
            foreach (GameObject hit in prevHitsY)
            {
                //for at sikre at exit kun bliver kørt hvis objektet forlader kollideren fuldkommen. køre ikke hvis det bare er hit pisitionen der ændre sig
                if (prevHitsX.Contains(hit) || prevHitsY.Contains(hit))
                {
                    string hitTag = hit.tag;

                    PhysicsObject other = hit.GetComponent<PhysicsObject>();
                    bool isCollidingWithPhysicsObject = other != null;

                    //for at køre hitTag kollisions systemet
                    if (onTagCollisionExit.ContainsKey(hitTag))
                    {
                        onTagCollisionExit[hitTag](hit);
                    }

                    //for at køre normal kollision
                    if (onCollisionExit != null)
                        onCollisionExit(hit);


                    if (isCollidingWithPhysicsObject)
                    {
                        //for at køre hitTag kollisions systemet
                        if (other.onTagCollisionExit.ContainsKey(tag))
                        {
                            other.onTagCollisionExit[tag](gameObject);
                        }
                        //for at køre normal kollision
                        if (other.onCollisionExit != null)
                            other.onCollisionExit(gameObject);
                    }
                }
            }
        }

        if (yMovement)
            prevHitsY = new List<GameObject>(currentHits);
        else
            prevHitsX = new List<GameObject>(currentHits);

        #endregion


        rb2d.position = rb2d.position + moveAmount.normalized * distance;

    }
}