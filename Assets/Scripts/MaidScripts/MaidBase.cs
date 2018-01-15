using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class MaidBase : ToonBase
{
    protected int nofc = 0; // number of floors climbed by maid
    protected float speedX;
    protected bool isBard = false;
    private float speedY = 4;
    // private float speedRebound = 5;
    private float y, lastY, targetY = 3.8f;
    private Animator animator;
    new private Transform transform;

    public Action action;
    public enum Action
    {
        Climbing,
        Falling,
        WalkingRight,
        WalkingLeft,
        ReboundingLeft,
        ReboundingRight
    }
    ;

    protected void Init()
    {
        ToonInit();
        animator = GetComponent<Animator>();

        // have each Maid move at slightly different speed
        speedX += speedX * UnityEngine.Random.Range(-0.07f, 0.05f);
        speedY += speedY * UnityEngine.Random.Range(-0.02f, 0.03f);

        transform = GetComponent<Transform>();
        lastY = transform.position.y;

        // get maid lvl
        stats[GameManager.LVL] = Camera.main.GetComponent<GameManager>().GetMaidLevel((int)stats[GameManager.ID]);

        SetFalling();
    }

    protected void Update()
    {
        nofc = 1;
        switch (action)
        {
            case Action.Climbing:
                // the box collider 2d was turned off
                // this vertical movement check for after 4.6 movement in y
                // tells when to turn the collider on again
                if (transform.position.y <= lastY + targetY)
                {
                    transform.Translate(Vector2.up * speedY * Time.deltaTime);
                    break;
                }
                else
                    SetFalling();
                break;

            case Action.Falling:
                // gravity from Physics 2D does the work
                break;

            case Action.WalkingLeft:
                transform.Translate(Vector2.left * speedX * Time.deltaTime);
                break;

            case Action.WalkingRight:
                transform.Translate(Vector2.right * speedX * Time.deltaTime);
                break;

            case Action.ReboundingLeft:
                transform.Translate(Vector2.left * speedX * 1.15f * Time.deltaTime);
                transform.Translate(Vector2.up * speedY / 2.5f * Time.deltaTime);
                break;

            case Action.ReboundingRight:
                transform.Translate(Vector2.right * speedX * 1.15f * Time.deltaTime);
                transform.Translate(Vector2.up * speedY / 2.5f * Time.deltaTime);
                break;
        }

        base.ToonBaseUpdate();
    }

    protected void OnTriggerEnter2D(Collider2D trigger)
    {
        if (trigger.name == "Climb_Node")
        {
            SetClimbing();
            Camera.main.GetComponent<GameManager>().CheckIfTopFloor(trigger.GetComponentInParent<FloorManager>().GetFloorNumber());
        }
    }

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        // base.Foo(collision);
        if (collision.gameObject.tag == "Enemy_")
        {
            SetRebounding();
            SetAttacking(collision);
        }

        if (collision.gameObject.tag == "Floor_Collider")
        {
            nofc += 1;
            currentFloor = collision.transform.parent.GetComponent<FloorManager>().GetFloorNumber();
            SetWalking();
        }
    }

    protected void SetAction(int newAction)
    {
        animator.SetBool("climbing", false);
        // animator.SetBool("falling", false);
        animator.SetBool("walkingLeft", false);
        animator.SetBool("walkingRight", false);

        switch (newAction)
        {
            case 0:
                action = Action.Climbing;
                animator.SetBool("climbing", true);
                break;
            case 1:
                action = Action.Falling;
                // animator.SetBool("falling", true);
                break;
            case 2:
                action = Action.WalkingLeft;
                animator.SetBool("walkingLeft", true);
                break;
            case 3:
                action = Action.WalkingRight;
                animator.SetBool("walkingRight", true);
                break;
            case 4:
                action = Action.ReboundingLeft;
                break;
            case 5:
                action = Action.ReboundingRight;
                break;
        }
    }

    protected void SetWalking()
    {
        // walk right
        if (currentFloor % 2 == 0)
            SetAction(3);
        // walk left
        else
            SetAction(2);
    }

    protected void SetClimbing()
    {
        SetAction(0);
        GetComponent<Rigidbody2D>().gravityScale = 0;
        GetComponent<BoxCollider2D>().enabled = false;
        lastY = transform.position.y;
    }

    protected void SetFalling()
    {
        SetAction(1);
        GetComponent<Rigidbody2D>().gravityScale = 1;
        GetComponent<BoxCollider2D>().enabled = true;
    }

    protected void SetRebounding()
    {
        // rebound left
        if (currentFloor % 2 == 0)
            SetAction(4);
        // rebound right
        else
            SetAction(5);
    }
}
