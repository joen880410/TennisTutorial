using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform aimTarget; // the target where we aim to land the ball
    public Transform ball; // the ball 
    public float speed = 3f; // move speed
    public bool isAuto;
    //bool hitting; // boolean to know if we are hitting the ball or not 
    Animator animator;
    public Vector3 initPos;
    Vector3 aimTargetInitialPosition; // initial position of the aiming gameObject which is the center of the opposite court

    ShotManager shotManager; // reference to the shotmanager component
    public Shot currentShot; // the current shot we are playing to acces it's attributes

    private void Start()
    {
        animator = GetComponent<Animator>(); // referennce out animator
        aimTargetInitialPosition = aimTarget.position; // initialise the aim position to the center( where we placed it in the editor )
        shotManager = GetComponent<ShotManager>(); // accesing our shot manager component 
        currentShot = shotManager.topSpin; // defaulting our current shot as topspin
        initPos = transform.position;
        Ball.OnBCollided += ResetPosition;
    }

    void Update()
    {
        if (isAuto)
        {
            Move();
        }
        else
        {
            float h = Input.GetAxisRaw("Horizontal"); // get the horizontal axis of the keyboard
            float v = Input.GetAxisRaw("Vertical"); // get the vertical axis of the keyboard
            transform.Translate(new Vector3(h, 0, v) * speed * Time.deltaTime);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            currentShot = shotManager.topSpin; // set our current shot to top spin
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            currentShot = shotManager.flat; // set our current shot to top spin
        }

    }
    private void ResetPosition()
    {
        transform.position = initPos;
    }
    void Move()
    {
        Vector3 targetPosition = ball.position;// update the target position to the ball's x position so the bot only moves on the x axis
        if (targetPosition.z > -5)
        {
            targetPosition.z = -5;
        }
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime); // lerp it's position
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball")) // if we collide with the ball 
        {
            Vector3 dir = aimTarget.position - transform.position; // get the direction to where we want to send the ball
            other.GetComponent<Rigidbody>().velocity = dir.normalized * currentShot.hitForce + new Vector3(0, currentShot.upForce, 0);
            //add force to the ball plus some upward force according to the shot being played

            Vector3 ballDir = ball.position - transform.position; // get the direction of the ball compared to us to know if it is
            if (ballDir.x >= 0)                                   // on out right or left side 
            {
                animator.Play("forehand");                        // play a forhand animation if the ball is on our right
            }
            else                                                  // otherwise play a backhand animation 
            {
                animator.Play("backhand");
            }

            aimTarget.position = aimTargetInitialPosition; // reset the position of the aiming gameObject to it's original position ( center)

        }
    }


}