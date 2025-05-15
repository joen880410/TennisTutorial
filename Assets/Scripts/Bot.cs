using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bot : MonoBehaviour
{

    public float speed = 40; // moveSpeed
    private Animator animator;
    public Transform ball;
    public Transform aimTarget; // aiming gameObject

    public Transform[] targets; // array of targets to aim at

    private Vector3 targetPosition; // position to where the bot will want to move
    public Vector3 initPos;
    private ShotManager shotManager; // shot manager class/component

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
        targetPosition = transform.position; // initialize the targetPosition to its initial position in the court
        animator = GetComponent<Animator>(); // reference to our animator for animations 
        shotManager = GetComponent<ShotManager>(); // reference to our shot manager to acces shots
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
        Move(); // calling the move method
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

    private Vector3 PickTarget() // picks a random target from the targets array to be aimed at
    {
        int randomValue = Random.Range(0, targets.Length); // get a random value from 0 to length of our targets array-1
        return targets[randomValue].position; // return the chosen target
    }

    private Shot PickShot() // picks a random shot to be played
    {
        int randomValue = Random.Range(0, 2); // pick a random value 0 or 1 since we have 2 shots possible currently
        if (randomValue == 0) // if equals to 0 return a top spin shot type
            return shotManager.topSpin;
        else                   // else return a flat shot type
            return shotManager.flat;
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
            //targetPosition.x = ball.position.x; // update the target position to the ball's x position so the bot only moves on the x axis
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
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball")) // if it collides with the ball
        {
            Shot currentShot = PickShot(); // pick a random shot to be played

            Vector3 dir = PickTarget() - transform.position; // get the direction to where to send the ball
            other.GetComponent<Rigidbody>().velocity = dir.normalized * currentShot.hitForce + new Vector3(0, currentShot.upForce, 0); // set force to the ball

            Vector3 ballDir = ball.position - transform.position; // get the ball direction from the bot's position
            if (ballDir.x >= 0) // if it is on the right
            {
                animator.Play("forehand"); // play a forehand animation
            }
            else
            {
                animator.Play("backhand"); // otherwise play a backhand animation
            }
            StopChase();
        }
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