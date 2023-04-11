using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform player;
    public Rigidbody rb;
    public EnvController envController;
    public Transform pole;
    // Start is called before the first frame update
    private bool holdingPole = false;
    void Start()
    {

    }

    public void ApplyForce(Vector3 force)
    {
        rb.AddForce(force * rb.mass * 10f);
    }

    // // Update is called once per frame
    // void FixedUpdate()
    // {
    //     var fx = Input.GetAxis("Horizontal");
    //     var fz = Input.GetAxis("Vertical");
    //     var movement = new Vector3(fx, 0, fz);
    //     rb.AddForce(movement * rb.mass * 10f);

    //     if (Input.GetKeyDown(KeyCode.Space))
    //     {
    //         rb.AddForce(new Vector3(0, 1, 0) * rb.mass * 1f, ForceMode.Impulse);
    //     }
    //     if (Input.GetKeyDown(KeyCode.V))
    //     {
    //         if (DistanceToPole() < 1.0f)
    //         {
    //             holdingPole = !holdingPole;
    //         }
    //     }
    // }

    // public void LateUpdate()
    // {
    //     ControlPole();
    // }

    public float DistanceToPole()
    {
        return Vector3.Distance(player.position, pole.position);
    }

    public float DistanceToTarget()
    {
        return Vector3.Distance(player.position, envController.target1Transform.position);
    }

    void ControlPole()
    {
        if (holdingPole)
        {
            pole.position = player.position + new Vector3(0.6f, 0f, 0f);
        }
    }

    internal bool isCloseToTarget()
    {
        return DistanceToTarget() < 3.0f;
    }
}