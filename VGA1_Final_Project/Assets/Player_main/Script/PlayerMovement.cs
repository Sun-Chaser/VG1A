using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 1;
    public float runSpeed = 1.5f;
    float Speed;
    bool attack;

    private Rigidbody2D rb;

    Vector2 MoveDirction;

    Animator animator;

    public Camera cam;

    public int dir8;

    bool shiftHeld;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        shiftHeld = Input.GetKey(KeyCode.LeftShift);
        attack  = Input.GetMouseButton(0);

        if (shiftHeld)
        {
            Speed = runSpeed;
        }else{
            Speed = walkSpeed;
        }


        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        MoveDirction = new Vector2(moveX, moveY).normalized;

        rb.velocity = MoveDirction * Speed;

        animator.SetFloat("Horizontal", moveX);
        animator.SetFloat("Vertical", moveY);
        animator.SetFloat("Speed", MoveDirction.sqrMagnitude);

        
        animator.SetBool("shiftHeld", shiftHeld);

        Vector3 sp = Input.mousePosition;
        float planeZ = transform.position.z;
        sp.z = cam.orthographic ? 0f : (planeZ - cam.transform.position.z);
        Vector3 mouseW = cam.ScreenToWorldPoint(sp);

        Vector2 d = (Vector2)(mouseW - transform.position);
        if (d.sqrMagnitude < 1e-6f) return;

        float ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
        if (ang < 0f) ang += 360f;
        dir8 = Mathf.RoundToInt(ang / 45f) % 8;

        animator.SetFloat("dir8", dir8);

        animator.SetBool("attack", attack);
        
    }

    private void FixedUpdate()
    {

    }
}
