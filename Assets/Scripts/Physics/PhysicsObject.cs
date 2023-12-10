using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PhysicsObject : MonoBehaviour
{
    //collision actions
    public Action<GameObject> onCollisionEnter;
    public Action<GameObject> onCollisionStay;
    public Action<GameObject> onCollisionExit;

    //collision with hitTag actions
    public Dictionary<string, Action<GameObject>> onTagCollisionEnter = new Dictionary<string, Action<GameObject>>();
    public Dictionary<string, Action<GameObject>> onTagCollisionStay = new Dictionary<string, Action<GameObject>>();
    public Dictionary<string, Action<GameObject>> onTagCollisionExit = new Dictionary<string, Action<GameObject>>();



    private void AddAction(Dictionary<string, Action<GameObject>> actionDictionary, string key, Action<GameObject> action)
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
    private void RemoveAction(Dictionary<string, Action<GameObject>> actionDictionary, string key, Action<GameObject> action)
    {
        actionDictionary[key] -= action;
    }

    //adding
    public void AddOnTagCollisionEnterEvent(string tag, Action<GameObject> action) { AddAction(onTagCollisionEnter, tag, action); }
    public void AddOnTagCollisionStayEvent(string tag, Action<GameObject> action) { AddAction(onTagCollisionStay, tag, action); }
    public void AddOnTagCollisionExitEvent(string tag, Action<GameObject> action) { AddAction(onTagCollisionExit, tag, action); }

    //removing
    public void RemoveOnTagCollisionEnterEvent(string tag, Action<GameObject> action) { RemoveAction(onTagCollisionEnter, tag, action); }
    public void RemoveOnTagCollisionStayEvent(string tag, Action<GameObject> action) { RemoveAction(onTagCollisionStay, tag, action); }
    public void RemoveOnTagCollisionExitEvent(string tag, Action<GameObject> action) { RemoveAction(onTagCollisionExit, tag, action); }
}
