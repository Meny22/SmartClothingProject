using UnityEngine;
using System.Collections;
using System.Threading;

public class AIWayPoint : MonoBehaviour
{

    float accel = 0.3f;
    float inertia = 0.9f;
    float speedLimit = 0.5f;
    float minSpeed = 0.5f;
    float stopTime = 1.0f;
    private Animator animator;
    private float currentSpeed = 0.0f;
    private bool triggered = false;

    private float functionState = 0;
    private bool accelState;
    private bool slowState;

    private Transform waypoint;
    private Transform lastWayPoint;
    float rotationDamping = 6.0f;
    bool smoothRotation = true;
    public Transform[] waypoints;
    private int WPindexPointer = 0;

    void Start()
    {
        animator = GetComponentInParent<Animator>();
        animator.SetBool("Walking", true);
        functionState = 0;
        lastWayPoint = waypoints[WPindexPointer];
    }


    void Update()
    {
        if (functionState == 0)
        {
            Accell();
        }
        if (functionState == 1)
        {
            //Slow();
        }
        waypoint = waypoints[WPindexPointer];
    }


    void Accell()
    {
        if (accelState == false)
        {
            accelState = true;
            slowState = false;
        }
        if (waypoint)
        {
            //Debug.Log(waypoint);
            if (smoothRotation && !animator.GetBool("StillInSight") && animator.GetCurrentAnimatorStateInfo(0).IsName("Aim_Walking"))
            {
                Quaternion rotation = Quaternion.LookRotation(waypoint.position - transform.parent.position);
                transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, rotation, Time.deltaTime*50);
                //transform.parent.LookAt(waypoint.position);
            }
            if(lastWayPoint != waypoint)
            {
                triggered = false;
                lastWayPoint = waypoint;
            }
        }
        if (!animator.GetBool("StillInSight") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.0f)
        {
            currentSpeed = currentSpeed + accel * accel;
            transform.parent.Translate(0, 0, Time.deltaTime * currentSpeed);
            if (currentSpeed >= speedLimit)
            {
                currentSpeed = speedLimit;
            }
            animator.SetBool("Walking", true);
            animator.SetBool("PlayerInSight", false);
        } else
        {
            animator.SetBool("Walking", false);
        }
    }


    void OnTriggerEnter(Collider other)
    {
        bool isCorrectTrigger = false;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (other.gameObject.transform == waypoints[i])
            {
                isCorrectTrigger = true;
                break;
            }
        }
        if (waypoint != null && !triggered && isCorrectTrigger)
        {
            WPindexPointer++;
            if (WPindexPointer >= waypoints.Length)
            {
                WPindexPointer = 0;
            }
            Debug.Log(WPindexPointer);
            triggered = true;
        }
        //Debug.Log("TRIGGERED" + gameObject);
    }


    //void Slow()
    //{
    //    if (slowState == false)
    //    {
    //        accelState = false;
    //        slowState = true;
    //    }

    //    currentSpeed = currentSpeed * inertia;
    //    transform.Translate(0, 0, Time.deltaTime * currentSpeed);

    //    if (currentSpeed <= minSpeed)
    //    {
    //        currentSpeed = 0.0f;
    //        Thread.Sleep(100);
    //        functionState = 0;
    //    }
    //}

}
