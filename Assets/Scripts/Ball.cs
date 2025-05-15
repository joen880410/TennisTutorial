using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public static event Action OnBCollided;
    Vector3 initialPos; // ball's initial position

    private void Start()
    {
        initialPos = transform.position; // default it to where we first place it in the scene
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Wall")) // if the ball hits a wall
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero; // reset it's velocity to 0 so it doesn't move anymore
            transform.position = initialPos; // reset it's position 
            OnBCollided?.Invoke();
        }
    }

}