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
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16]; 
    protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(); //only has the RaycastHit2Ds that hit something

    protected const float minMoveDistance = 0.001f;
    protected const float shellRadius = 0.01f; //the padding between 2 colliders. It prevents colliders from getting stuck in each other
    protected const float stepExtra = 0.05f;

    protected float relativeBottomY = float.MaxValue; //the lowest y point on the GravityObject in relation to the position of the GravityObject

    #region collision Actions
    //collision actions
    public Action<RaycastHit2D> onCollisionEnter;
    public Action<RaycastHit2D> onCollisionStay;
    public Action<RaycastHit2D> onCollisionExit;

    //collision with hitTag actions
    public Dictionary<string, Action<RaycastHit2D>> onTagCollisionEnter = new Dictionary<string, Action<RaycastHit2D>>();
    public Dictionary<string, Action<RaycastHit2D>> onTagCollisionStay = new Dictionary<string, Action<RaycastHit2D>>();
    public Dictionary<string, Action<RaycastHit2D>> onTagCollisionExit = new Dictionary<string, Action<RaycastHit2D>>();

    protected List<RaycastHit2D> currentHits = new List<RaycastHit2D>();
    protected List<RaycastHit2D> prevHits = new List<RaycastHit2D>();
    #endregion


    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        contactFilter.useTriggers = false; //makes it ignore colliders with isTrigger enabled
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer)); //sets the layermask to the layers the current gameobjects layer can collide with
        contactFilter.useLayerMask = true;

        //for finding bottom point of the rigidbody2d
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
        velocity += gravityModifier * Physics2D.gravity * Level.current.gravity * Time.deltaTime; //adds gravity to an object
        velocity.x = targetVelocity.x; //adds horizontal movement to object

        grounded = false;

        Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x); //finds the cross vector to ground normal (this line goes along the slope of the surface)
        Vector2 deltaPosition = velocity * Time.deltaTime; //the change in position due to velocity

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
    /// handles the movement of a GravityObject
    /// </summary>
    /// <param name="moveAmount">the amount the object should move</param>
    /// <param name="yMovement">true if the given move vector is for the y axis, otherwise false</param>
    protected virtual void Movement(Vector2 moveAmount, bool yMovement)
    {
        #region CollisionCheck

        float distance = moveAmount.magnitude;

        if (distance > minMoveDistance)
        {
            int count = rb2d.Cast(moveAmount, contactFilter, hitBuffer, distance + shellRadius); //casts all coliders attached to a rigidbody in a given direction
            hitBufferList.Clear();
            for (int i = 0; i < count; i++)
            {
                hitBufferList.Add(hitBuffer[i]);
            }

            //til kollision actions
            currentHits.Clear();

            foreach (RaycastHit2D hit in hitBufferList)
            {
                currentHits.Add(hit);

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
                    //for stepping over bumps smaler than shell radius
                    else if (hitY - relativeBottomY + shellRadius > 0)
                    {
                        rb2d.position += new Vector2(0, hitY - relativeBottomY + shellRadius + 0.01f);
                        currentNormal = Vector2.up;
                    }
                }

                //for stepping up
                else if (hitY - relativeBottomY + shellRadius > 0 && hitY - relativeBottomY + shellRadius < stepHeight)
                {
                    rb2d.position += new Vector2(0, hitY - relativeBottomY + shellRadius + stepExtra);
                    currentNormal = Vector2.up;
                }

                //to handle a physicsObject colliding with a sloped roof, and make it slide along, instead of stopping completly
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
        //for at sørge for den ikke køre 2 gange pr. fixed update
        if (yMovement)
        {
            //for at få en liste af alle de kollidere der var collideret med sidste update
            List<Collider2D> prevCollisions = new List<Collider2D>();
            foreach (RaycastHit2D hit in prevHits)
            {
                prevCollisions.Add(hit.collider);
            }



            foreach (RaycastHit2D hit in currentHits)
            {
                string hitTag = hit.collider.tag;

                if (prevCollisions.Contains(hit.collider))
                {
                    //for at køre hitTag kollisions systemet
                    if (onTagCollisionStay.ContainsKey(hitTag))
                    {
                        onTagCollisionStay[hitTag](hit);
                    }
                    //for at køre normal kollision
                    if (onCollisionStay != null)
                        onCollisionStay(hit);
                    prevHits.Remove(hit);
                    prevCollisions.Remove(hit.collider);
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
                }
            }

            foreach (RaycastHit2D hit in prevHits)
            {
                //for at sikre at exit kun bliver kørt hvis objektet forlader kollideren fuldkommen. køre ikke hvis det bare er hit pisitionen der ændre sig
                if (prevCollisions.Contains(hit.collider))
                {
                    string hitTag = hit.collider.tag;

                    //for at køre hitTag kollisions systemet
                    if (onTagCollisionExit.ContainsKey(hitTag))
                    {
                        onTagCollisionExit[hitTag](hit);
                    }

                    //for at køre normal kollision
                    if (onCollisionExit != null)
                        onCollisionExit(hit);
                }
            }

            prevHits = new List<RaycastHit2D>(currentHits);
        }

        #endregion


        rb2d.position = rb2d.position + moveAmount.normalized * distance;

    }

    #region collision Actions

    private void AddAction(Dictionary<string, Action<RaycastHit2D>> actionDictionary, string key, Action<RaycastHit2D> action)
    {
        if (actionDictionary.ContainsKey(key)) 
        {
            actionDictionary[key] += action;
        }
        else
        {
            actionDictionary.Add(key, action);
        }
    }
    private void RemoveAction(Dictionary<string, Action<RaycastHit2D>> actionDictionary, string key, Action<RaycastHit2D> action)
    {
        actionDictionary[key] -= action;
    }

    //adding
    public void AddOnTagCollisionEnterEvent(string tag, Action<RaycastHit2D> action) { AddAction(onTagCollisionEnter, tag, action); }
    public void AddOnTagCollisionStayEvent(string tag, Action<RaycastHit2D> action) { AddAction(onTagCollisionStay, tag, action); }
    public void AddOnTagCollisionExitEvent(string tag, Action<RaycastHit2D> action) { AddAction(onTagCollisionExit, tag, action); }

    //removing
    public void RemoveOnTagCollisionEnterEvent(string tag, Action<RaycastHit2D> action) { RemoveAction(onTagCollisionEnter, tag, action); }
    public void RemoveOnTagCollisionStayEvent(string tag, Action<RaycastHit2D> action) { RemoveAction(onTagCollisionStay, tag, action); }
    public void RemoveOnTagCollisionExitEvent(string tag, Action<RaycastHit2D> action) { RemoveAction(onTagCollisionExit, tag, action); }

    #endregion
}
