﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class JumpPredictionController : MonoBehaviour
{
    [SerializeField]
    public float maxHeight = 1.5f;
    [SerializeField]
    public float maxDistance = 5.0f;
    [SerializeField]
    public float maxTime = 2.0f;

    float turnSmoothVelocity;
    ThirdPersonController controller;

    Vector3 origin;
    Vector3 target;
    float distance = 0;

    public bool showDebug = false;

    bool move = false;

    protected float actualSpeed = 0;

    public int accuracy = 50;

    private void Start()
    {
        controller = GetComponent<ThirdPersonController>();
    }

    void OnDrawGizmos()
    {
        if (!showDebug)
            return;

        if (!Application.isPlaying)
            origin = transform.position;

        if (origin == Vector3.zero)
            return;

        //Draw the parabola by sample a few times
        Vector3 lastP = origin;
        for (float i = 0; i < accuracy; i++)
        {
            Vector3 p = SampleParabola(origin, target, maxHeight, i / accuracy);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(lastP, p);
            Gizmos.DrawWireSphere(p, 0.02f);
            lastP = p;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (hasArrived() && Input.GetKeyDown(KeyCode.Space))
        {
            //FINDPOINTS
            
            
            
            
            if(SetParabola(transform.position, GameObject.Find("Target").transform.position))
            {
                controller.DisableController();
                controller.characterAnimation.animator.CrossFade("Predicted Jump", 0.1f);
            }
        }

        if (!hasArrived())
            FollowParabola(0.7f);

    }

    public void FollowParabola(float length)
    {
        if (move == true)
        {
            if (actualSpeed >= 1.0f)
            {
                actualSpeed = 0.0f;
                controller.EnableController();
                move = false;
            }
            else
            {
                actualSpeed += Time.deltaTime / length;
                transform.position = SampleParabola(origin, target, maxHeight, actualSpeed);

                //Rotate Mesh to Movement
                Vector3 travelDirection = target - origin;
                float targetAngle = Mathf.Atan2(travelDirection.x, travelDirection.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, 0.1f);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }
        }
    }

    public bool hasArrived()
    {
        return !move;
    }

    public bool SetParabola(Transform start, Transform end)
    {
        Vector2 a = new Vector2(start.position.x, start.position.z);
        Vector2 b = new Vector2(end.position.x, end.position.z);

        if (end.position.y - start.position.y > maxHeight || (Vector2.Distance(a, b) > maxDistance))
            return false;

        origin = start.position;
        target = end.position;
        move = true; 
        
        actualSpeed = 0.0f;

        return true;
    }

    public bool SetParabola(Vector3 start, Vector3 end)
    {
        Vector2 a = new Vector2(start.x, start.z);
        Vector2 b = new Vector2(end.x, end.z);
        distance = Vector2.Distance(a, b);

       if (end.y - start.y > maxHeight || ( distance > maxDistance))
           return false;

        origin = start;
        target = end;
        move = true;

        actualSpeed = 0.0f;

        return true;
    }

    Vector3 SampleParabola(Vector3 start, Vector3 end, float height, float t)
    {
        float parabolicT = t * 2 - 1;
        Vector3 travelDirection = end - start;
        Vector3 result = start + t * travelDirection;
        result.y += (-parabolicT * parabolicT + 1) * height;

        //Vector3 travelDirection = end - start;
        //Vector3 result = start + t * travelDirection;
        //result.y += Mathf.Sin(t * Mathf.PI) * height;

        return result;

        /*
        if (Mathf.Abs(start.y - end.y) < 0.1f)
        {
            Vector3 travelDirection = end - start;
            Vector3 result = start + t * travelDirection;
            result.y += (-parabolicT * parabolicT + 1) * height;
            return result;
        }
        else
        {
            Vector3 travelDirection = end - start;
            //Vector3 levelDirection = end - new Vector3(start.x, end.y, start.z);
            //Vector3 right = Vector3.Cross(travelDirection, levelDirection);
            //Vector3 up = Vector3.Cross(right, levelDirection);
            Vector3 up = Vector3.up;
            //if (end.y > start.y) up = -up;
            Vector3 result = start + t * travelDirection;
            result += ((-parabolicT * parabolicT + 1) * height) * Vector3.up;
            return result;
        }
        */
    }

}