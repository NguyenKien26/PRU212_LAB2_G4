using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PlayerController : MonoBehaviour
{
    [SerializeField]
    float torqueAmount = 1f;
    public int stopTime = 10;

    Rigidbody2D rb2d;
    SurfaceEffector2D surfaceEffector;
    bool canMove = true;
    //sound effect if you crash land do a spin 
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        surfaceEffector = FindFirstObjectByType<SurfaceEffector2D>();
    }
    void Update()
    {
        if (canMove)
        {
            RotatePlayer();
        }
    }
    public void DisablePlayer()
    {
        canMove = false;
    }
    void RotatePlayer()
    {
        if (Input.GetKey(KeyCode.LeftArrow)) //front flip
        {
            rb2d.AddTorque(torqueAmount);
        }
        else if (Input.GetKey(KeyCode.RightArrow)) // back flip
        {
            rb2d.AddTorque(-torqueAmount);
        }
    }

}
