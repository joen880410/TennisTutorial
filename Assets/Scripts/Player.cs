using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Analytics.Internal;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform aimTarget; // the target where we aim to land the ball
    public Transform ball; // the ball 
    public float speed = 3f; // move speed
    public bool isAuto;
    //bool hitting; // boolean to know if we are hitting the ball or not 
    private Animator animator;
    public Vector3 initPos;
    private Vector3 aimTargetInitialPosition; // initial position of the aiming gameObject which is the center of the opposite court

    private ShotManager shotManager; // reference to the shotmanager component
    public Shot currentShot; // the current shot we are playing to acces it's attributes
    [Header("追球參數")]
    public float startChaseRadius = 6f;       // 開始追球的觸發距離
    public float maxReachDistance = 8f;       // 正常奔跑可及距離
    public float diveReachDistance = 12f;     // 飛撲可及最遠距離
    public float predictTime = 1.0f;          // 落點預測時間

    [Header("飛撲參數")]
    public float diveDuration = 0.5f;         // 飛撲動作持續時間
    public float diveSpeedMultiplier = 2.5f;  // 飛撲速度倍率
    public float diveCooldown = 2.0f;         // 飛撲後冷卻時間

    private bool isChasing = false;
    private bool isReset = false;
    private void Start()
    {
        animator = GetComponent<Animator>(); // referennce out animator
        aimTargetInitialPosition = aimTarget.position; // initialise the aim position to the center( where we placed it in the editor )
        shotManager = GetComponent<ShotManager>(); // accesing our shot manager component 
        currentShot = shotManager.topSpin; // defaulting our current shot as topspin
        initPos = transform.position;
        Ball.OnBCollided += ResetPosition;
    }

    private void Update()
    {
        if (isReset)
        {
            transform.position = Vector3.MoveTowards(transform.position, initPos, speed * 10 * Time.deltaTime); // lerp it's position
            if (transform.position == initPos)
            {
                isReset = false;
            }
            return;
        }
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
        isReset = true;
        isChasing = false;
    }
    private void Move()
    {
        Vector3 targetPosition = ball.position;// update the target position to the ball's x position so the bot only moves on the x axis
        float distToBall = Vector3.Distance(transform.position, targetPosition);
        // 1. 初步觸發追球
        if (!isChasing && distToBall <= startChaseRadius)
        {
            isChasing = true;
        }
        if (isChasing)
        {
            Chasing();
        }
    }
    private void Chasing()
    {
        // 2. 落點預測
        Vector3 predictedPos = PredictBallPosition(predictTime);
        float distToIntercept = Vector3.Distance(transform.position, predictedPos);

        // 3a. 正常奔跑可及
        if (distToIntercept <= maxReachDistance)
        {
            animator.SetBool("IsRunning", true);
            transform.position = Vector3.MoveTowards(transform.position, predictedPos, speed * Time.deltaTime); // lerp it's position
        }
        // 3b. 飛撲可及
        else if (distToIntercept <= diveReachDistance)
        {
            DoDive(predictedPos);
        }
        // 3c. 超出可及範圍 → 放棄
        else if (distToIntercept > diveReachDistance)
        {
            StopChase();
        }

    }
    Vector3 PredictBallPosition(float t)
    {
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        return ball.position + rb.velocity * t + 0.5f * t * t * Physics.gravity;
    }
    void StopChase()
    {
        isChasing = false;
        transform.position = Vector3.MoveTowards(transform.position, initPos, speed * Time.deltaTime); // lerp it's position
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
            StopChase();
            //aimTarget.position = aimTargetInitialPosition; // reset the position of the aiming gameObject to it's original position ( center)
        }
    }
    private void DoDive(Vector3 targetPos)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 dir = (targetPos - startPos).normalized;
        float diveSpeed = speed * diveSpeedMultiplier;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, diveSpeed * Time.deltaTime); // lerp it's position
        //while (elapsed < diveDuration)
        //{
        //    transform.position += diveSpeed * Time.deltaTime * dir;
        //    elapsed += Time.deltaTime;
        //}

        // 飛撲結束後
    }
    private void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position;
        // 最外圈：放棄範圍
        Gizmos.color = new Color(1f, 1f, 1f, 0.5f); // 橘色半透明
        Gizmos.DrawWireSphere(center, diveReachDistance);
        // 外圈：飛撲範圍
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // 橘色半透明
        Gizmos.DrawWireSphere(center, diveReachDistance);

        // 中圈：正常可達
        Gizmos.color = new Color(0f, 0.7f, 1f, 0.5f); // 藍色半透明
        Gizmos.DrawWireSphere(center, maxReachDistance);

        // 內圈：開始追球
        Gizmos.color = new Color(0f, 1f, 0f, 0.6f); // 綠色半透明
        Gizmos.DrawWireSphere(center, startChaseRadius);
    }
}