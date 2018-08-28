using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotor : MonoBehaviour
{

    private const float LANE_DISTANCE = 2.5f;
    private const float TURN_SPEED = 0.05f;

    // Functionality
    private bool isRunning = false;

    //Movement 
    private CharacterController controller;
    private float jumpForce = 5.0f;
    private float gravity = 12.0f;
    private float verticalVelocity;
    private int desiredLane = 1; // 0 = Left, 1 = Middle, 2 = Right

    // Speed Modifier
    private float originalSpeed = 7.0f;
    private float speed;
    private float speedIncreaseLastTick;
    private float speedIncreaseTime = 2.5f;
    private float speedIncreaseAmount = 0.1f;

    private void Start()
    {
        speed = originalSpeed;
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!isRunning)
            return;

        if (Time.time - speedIncreaseLastTick > speed)
        {
            speedIncreaseLastTick = Time.time;
            speed += speedIncreaseAmount;
            GameManager.Instance.UpdateModifier(speed - originalSpeed);
        }


        // Gather the inputs on which lane we should be
        if (MobileInput.Instance.SwipeLeft)
            MoveLane(false);
        if (MobileInput.Instance.SwipeRight)
            MoveLane(true);

        // Calculate where we should be in the future
        Vector3 targetPosition = transform.position.z * Vector3.forward;

        if (desiredLane == 0)
            targetPosition += Vector3.left * LANE_DISTANCE;
        else if (desiredLane == 2)
            targetPosition += Vector3.right * LANE_DISTANCE;

        // Ler's calculate our move delta
        Vector3 moveVector = Vector3.zero;
        moveVector.x = (targetPosition - transform.position).normalized.x * speed;

        // Calculate Y
        if (IsGrounded()) // if Grounded
        {
            verticalVelocity = -0.1f;

            if (MobileInput.Instance.SwipeUp)
            {
                //Jump
                verticalVelocity = jumpForce;
            }
        }
        else
        {
            verticalVelocity -= (gravity * Time.deltaTime);

            // Fast Falling mechaninc
            if (MobileInput.Instance.SwipeDown)
            {
                verticalVelocity = -jumpForce;
            }
        }

        moveVector.y = verticalVelocity;
        moveVector.z = speed;

        // Move the Bicycle
        controller.Move(moveVector * Time.deltaTime);

        // Rotate the Bicycle to where she is going
        Vector3 dir = controller.velocity;
        if (dir != Vector3.zero)
        {
            dir.y = 0;
            transform.forward = Vector3.Lerp(transform.forward, dir, TURN_SPEED);
        }
    }

    private void MoveLane(bool goingRight)
    {
        desiredLane += (goingRight) ? 1 : -1;
        desiredLane = Mathf.Clamp(desiredLane, 0, 2);
    }

    private bool IsGrounded()
    {
        Ray groundRay = new Ray(new Vector3(controller.bounds.center.x, (controller.bounds.center.y - controller.bounds.extents.y) + 0.2f, controller.bounds.center.z), Vector3.down);
        Debug.DrawRay(groundRay.origin, groundRay.direction, Color.cyan, 1.0f);

        return (Physics.Raycast(groundRay, 0.2f + 0.1f));

    }

    public void StartRunning()
    {
        isRunning = true;
    }

    private void Crash()
    {
        isRunning = false;
        GameManager.Instance.IsDead = true;
    }
}
